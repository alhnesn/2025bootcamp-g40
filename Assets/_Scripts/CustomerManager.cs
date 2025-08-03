using System.Collections;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform waitPoint;  // Position + rotation for window
    [SerializeField] private Transform despawnPoint;
    
    [Header("Customer Settings")]
    [SerializeField] private GameObject[] customerPrefabs; // Changed from single prefab to array
    [SerializeField] private string feetObjectName = "Feet"; // Name of the feet child object
    [SerializeField] private float minSpawnDelay = 5f;
    [SerializeField] private float maxSpawnDelay = 30f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 180f; // degrees per second
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool showWaypoints = true;
    
    // State tracking
    private Customer currentCustomer;
    private bool windowOccupied = false;
    private CustomerState currentState = CustomerState.WaitingToSpawn;
    private Coroutine spawnTimerCoroutine;
    private Coroutine movementCoroutine;

    private enum CustomerState
    {
        WaitingToSpawn,
        MovingToWindow,
        AtWindow,
        LeavingWindow,
        Empty
    }

    void Start()
    {
        ValidateSetup();
        StartSpawnTimer();
    }

    /// <summary>
    /// Validate that all required components are assigned
    /// </summary>
    private void ValidateSetup()
    {
        if (spawnPoint == null || waitPoint == null || despawnPoint == null)
        {
            Debug.LogError("CustomerManager: Missing waypoint assignments!");
            enabled = false;
            return;
        }
        
        // Updated validation for multiple prefabs
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("CustomerManager: No customer prefabs assigned!");
            enabled = false;
            return;
        }
        
        // Check that all prefabs have Customer component
        for (int i = 0; i < customerPrefabs.Length; i++)
        {
            if (customerPrefabs[i] != null && customerPrefabs[i].GetComponent<Customer>() == null)
            {
                Debug.LogError($"CustomerManager: Prefab {customerPrefabs[i].name} is missing Customer component!");
            }
        }
        
        if (showDebugLogs)
            Debug.Log($"CustomerManager initialized with {customerPrefabs.Length} customer types.");
    }

    /// <summary>
    /// Calculate the offset needed to position character's feet on ground
    /// </summary>
    private Vector3 GetFeetOffset(GameObject customer)
    {
        Transform feetTransform = customer.transform.Find(feetObjectName);
        if (feetTransform != null)
        {
            // Calculate offset from customer pivot to feet
            Vector3 offset = customer.transform.position - feetTransform.position;
            return offset;
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning($"No '{feetObjectName}' child found on customer. Using zero offset.");
            return Vector3.zero;
        }
    }
    
    
    /// <summary>
    /// Start the spawn timer
    /// </summary>
    private void StartSpawnTimer()
    {
        if (spawnTimerCoroutine != null)
            StopCoroutine(spawnTimerCoroutine);
            
        spawnTimerCoroutine = StartCoroutine(SpawnTimerCoroutine());
    }

    /// <summary>
    /// Spawn timer coroutine
    /// </summary>
    private IEnumerator SpawnTimerCoroutine()
    {
        while (true)
        {
            // Wait for random interval
            float delay = Random.Range(minSpawnDelay, maxSpawnDelay);
            
            if (showDebugLogs)
                Debug.Log($"CustomerManager: Next customer in {delay:F1} seconds");
                
            yield return new WaitForSeconds(delay);
            
            // Try to spawn customer
            if (!windowOccupied && currentState == CustomerState.WaitingToSpawn)
            {
                SpawnCustomer();
            }
        }
    }

    /// <summary>
    /// Spawn a new customer
    /// </summary>
    private void SpawnCustomer()
    {
        if (windowOccupied)
        {
            if (showDebugLogs)
                Debug.Log("Window occupied, cannot spawn customer");
            return;
        }
        
        // Select random customer prefab
        GameObject selectedPrefab = GetRandomCustomerPrefab();
        if (selectedPrefab == null) return;
        
        // Instantiate customer at spawn point
        GameObject customerObj = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Apply feet offset
        Vector3 feetOffset = GetFeetOffset(customerObj);
        customerObj.transform.position = spawnPoint.position + feetOffset;
        
        currentCustomer = customerObj.GetComponent<Customer>();
        
        if (currentCustomer == null)
        {
            Debug.LogError("Customer prefab is missing Customer component!");
            Destroy(customerObj);
            return;
        }
        
        // Set up customer
        windowOccupied = true;
        currentState = CustomerState.MovingToWindow;
        
        // Start movement to window
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
        movementCoroutine = StartCoroutine(MoveCustomerToWindow());
        
        if (showDebugLogs)
            Debug.Log($"Spawned customer: {currentCustomer.customerName}");
    }

    /// <summary>
    /// Get a random customer prefab from the available ones
    /// </summary>
    private GameObject GetRandomCustomerPrefab()
    {
        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            Debug.LogError("No customer prefabs available!");
            return null;
        }
        
        // Filter out null prefabs
        GameObject[] validPrefabs = System.Array.FindAll(customerPrefabs, prefab => prefab != null);
        
        if (validPrefabs.Length == 0)
        {
            Debug.LogError("All customer prefabs are null!");
            return null;
        }
        
        // Return random valid prefab
        return validPrefabs[Random.Range(0, validPrefabs.Length)];
    }

    /// <summary>
    /// Move customer from spawn to window
    /// </summary>
    private IEnumerator MoveCustomerToWindow()
    {
        if (currentCustomer == null) yield break;
        
        Vector3 feetOffset = GetFeetOffset(currentCustomer.gameObject);
        Vector3 startPos = spawnPoint.position + feetOffset;
        Vector3 endPos = waitPoint.position + feetOffset;
        Quaternion finalRotation = waitPoint.rotation;
        
        // Calculate movement duration based on distance and speed
        float distance = Vector3.Distance(startPos, endPos);
        float moveDuration = distance / moveSpeed;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < moveDuration)
        {
            if (currentCustomer == null) yield break;
            
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;
            float curvedProgress = movementCurve.Evaluate(progress);
            
            // Move position
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, curvedProgress);
            currentCustomer.transform.position = currentPos;
            
            // Rotate to face movement direction
            if (progress < 0.95f) // Don't rotate in the last 5% to avoid snapping
            {
                Vector3 direction = (endPos - startPos).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    currentCustomer.transform.rotation = Quaternion.Slerp(
                        currentCustomer.transform.rotation, 
                        lookRotation, 
                        rotationSpeed * Time.deltaTime / 180f
                    );
                }
            }
            
            yield return null;
        }
        
        // Ensure final position
        currentCustomer.transform.position = endPos;
        
        // Rotate to face window
        yield return StartCoroutine(RotateCustomer(finalRotation));
        
        // Customer has arrived at window
        currentState = CustomerState.AtWindow;
        
        // Tell customer they can start taking orders
        if (currentCustomer != null)
        {
            currentCustomer.OnArrivedAtWindow();
        }
        
        if (showDebugLogs)
            Debug.Log("Customer arrived at window");
    }

    /// <summary>
    /// Smoothly rotate customer to target rotation
    /// </summary>
    private IEnumerator RotateCustomer(Quaternion targetRotation)
    {
        if (currentCustomer == null) yield break;
        
        Quaternion startRotation = currentCustomer.transform.rotation;
        float rotationTime = Quaternion.Angle(startRotation, targetRotation) / rotationSpeed;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < rotationTime)
        {
            if (currentCustomer == null) yield break;
            
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / rotationTime;
            
            currentCustomer.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, progress);
            
            yield return null;
        }
        
        // Ensure final rotation
        if (currentCustomer != null)
            currentCustomer.transform.rotation = targetRotation;
    }

    /// <summary>
    /// Called by Customer.cs when they're ready to leave
    /// </summary>
    public void RequestCustomerLeave()
    {
        if (currentCustomer != null && currentState == CustomerState.AtWindow)
        {
            if (showDebugLogs)
                Debug.Log("Customer requesting to leave");
                
            currentState = CustomerState.LeavingWindow;
            
            if (movementCoroutine != null)
                StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(MoveCustomerAway());
        }
    }

    /// <summary>
    /// Move customer from window to despawn point
    /// </summary>
    private IEnumerator MoveCustomerAway()
    {
        if (currentCustomer == null) yield break;
        
        Vector3 feetOffset = GetFeetOffset(currentCustomer.gameObject);
        Vector3 startPos = waitPoint.position + feetOffset;
        Vector3 endPos = despawnPoint.position + feetOffset;
        
        // First, rotate to face despawn direction
        Vector3 direction = (endPos - startPos).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            yield return StartCoroutine(RotateCustomer(lookRotation));
        }
        
        // Then move to despawn point
        float distance = Vector3.Distance(startPos, endPos);
        float moveDuration = distance / moveSpeed;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < moveDuration)
        {
            if (currentCustomer == null) yield break;
            
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / moveDuration;
            float curvedProgress = movementCurve.Evaluate(progress);
            
            Vector3 currentPos = Vector3.Lerp(startPos, endPos, curvedProgress);
            currentCustomer.transform.position = currentPos;
            
            yield return null;
        }
        
        // Customer has left
        if (currentCustomer != null)
        {
            if (showDebugLogs)
                Debug.Log($"Customer {currentCustomer.customerName} has left");
                
            Destroy(currentCustomer.gameObject);
        }
        
        // Reset state
        currentCustomer = null;
        windowOccupied = false;
        currentState = CustomerState.WaitingToSpawn;
        
        if (showDebugLogs)
            Debug.Log("Window is now free for next customer");
    }

    /// <summary>
    /// Force current customer to leave (useful for testing or emergency)
    /// </summary>
    [ContextMenu("Force Customer Leave")]
    public void ForceCustomerLeave()
    {
        if (currentCustomer != null)
        {
            RequestCustomerLeave();
        }
    }
    
    /// <summary>
    /// Get current customer (useful for debugging)
    /// </summary>
    public Customer GetCurrentCustomer()
    {
        return currentCustomer;
    }

    /// <summary>
    /// Check if window is occupied
    /// </summary>
    public bool IsWindowOccupied()
    {
        return windowOccupied;
    }
    
    /// <summary>
    /// Manually spawn customer (useful for testing)
    /// </summary>
    [ContextMenu("Spawn Customer Now")]
    public void SpawnCustomerNow()
    {
        if (!windowOccupied)
        {
            SpawnCustomer();
        }
        else
        {
            Debug.Log("Cannot spawn - window occupied");
        }
    }

    void OnDrawGizmos()
    {
        if (!showWaypoints) return;
        
        // Draw waypoints
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawRay(spawnPoint.position, spawnPoint.forward * 1f);
        }
        
        if (waitPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(waitPoint.position, 0.5f);
            Gizmos.DrawRay(waitPoint.position, waitPoint.forward * 1f);
        }
        
        if (despawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(despawnPoint.position, 0.5f);
            Gizmos.DrawRay(despawnPoint.position, despawnPoint.forward * 1f);
        }
        
        // Draw path
        if (spawnPoint != null && waitPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(spawnPoint.position, waitPoint.position);
        }
        
        if (waitPoint != null && despawnPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(waitPoint.position, despawnPoint.position);
        }
    }
    
    void OnDestroy()
    {
        if (spawnTimerCoroutine != null)
            StopCoroutine(spawnTimerCoroutine);
        if (movementCoroutine != null)
            StopCoroutine(movementCoroutine);
    }
}
