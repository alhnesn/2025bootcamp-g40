using System.Collections;
using UnityEngine;

public class FridgeDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openAngle = -100f; // Negative Y rotation
    [SerializeField] private float animationDuration = 1f; // Time to open/close
    [SerializeField] private float autoCloseDelay = 20f; // Auto-close after this many seconds
    [SerializeField] private bool startsOpen = false;
    
    [Header("Animation Curve")]
    [SerializeField] private AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool isOpen;
    private bool isAnimating;
    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine autoCloseCoroutine;

    void Start()
    {
        // Store the initial rotation as closed position
        closedRotation = transform.localRotation;
        
        // Calculate open rotation
        openRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);
        
        // Set initial state
        isOpen = startsOpen;
        if (isOpen)
        {
            transform.localRotation = openRotation;
            StartAutoCloseTimer();
        }
        else
        {
            transform.localRotation = closedRotation;
        }
        
        if (showDebugLogs)
            Debug.Log($"FridgeDoor '{gameObject.name}' initialized. Starts open: {startsOpen}");
    }

    /// <summary>
    /// Toggle the door open/closed. Called by PlayerInteraction.
    /// </summary>
    public void ToggleDoor()
    {
        if (isAnimating)
        {
            if (showDebugLogs)
                Debug.Log("Door is animating, ignoring click.");
            return;
        }
        
        if (isOpen)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Open the door
    /// </summary>
    public void OpenDoor()
    {
        if (isOpen || isAnimating) return;
        
        if (showDebugLogs)
            Debug.Log("Opening fridge door...");
            
        StartCoroutine(AnimateDoor(closedRotation, openRotation, () => {
            isOpen = true;
            StartAutoCloseTimer();
        }));
    }
    
    /// <summary>
    /// Close the door
    /// </summary>
    public void CloseDoor()
    {
        if (!isOpen || isAnimating) return;
        
        if (showDebugLogs)
            Debug.Log("Closing fridge door...");
            
        StopAutoCloseTimer();
        StartCoroutine(AnimateDoor(openRotation, closedRotation, () => {
            isOpen = false;
        }));
    }

    /// <summary>
    /// Smoothly animate the door between two rotations
    /// </summary>
    private IEnumerator AnimateDoor(Quaternion fromRotation, Quaternion toRotation, System.Action onComplete)
    {
        isAnimating = true;
        
        float elapsedTime = 0f;
        
        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            
            // Use animation curve for smooth easing
            float curvedProgress = openCurve.Evaluate(progress);
            
            // Interpolate rotation
            transform.localRotation = Quaternion.Lerp(fromRotation, toRotation, curvedProgress);
            
            yield return null;
        }
        
        // Ensure final rotation is exact
        transform.localRotation = toRotation;
        
        isAnimating = false;
        onComplete?.Invoke();
    }

    /// <summary>
    /// Start the auto-close timer
    /// </summary>
    private void StartAutoCloseTimer()
    {
        StopAutoCloseTimer(); // Stop any existing timer
        
        if (autoCloseDelay > 0f)
        {
            autoCloseCoroutine = StartCoroutine(AutoCloseAfterDelay());
        }
    }
    
    /// <summary>
    /// Stop the auto-close timer
    /// </summary>
    private void StopAutoCloseTimer()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }
    }

    /// <summary>
    /// Auto-close coroutine
    /// </summary>
    private IEnumerator AutoCloseAfterDelay()
    {
        yield return new WaitForSeconds(autoCloseDelay);
        
        if (isOpen && !isAnimating)
        {
            if (showDebugLogs)
                Debug.Log("Auto-closing fridge door after delay.");
            CloseDoor();
        }
    }
    
    /// <summary>
    /// Get current door state
    /// </summary>
    public bool IsOpen => isOpen;
    
    /// <summary>
    /// Get if door is currently animating
    /// </summary>
    public bool IsAnimating => isAnimating;
    
    /// <summary>
    /// Manually set the animation duration (useful for runtime adjustments)
    /// </summary>
    public void SetAnimationDuration(float duration)
    {
        animationDuration = Mathf.Max(0.1f, duration); // Minimum 0.1 seconds
    }

    /// <summary>
    /// Reset door to closed position (useful for testing)
    /// </summary>
    [ContextMenu("Reset to Closed")]
    public void ResetToClosed()
    {
        StopAllCoroutines();
        isAnimating = false;
        isOpen = false;
        transform.localRotation = closedRotation;
        StopAutoCloseTimer();
    }
    
    /// <summary>
    /// Force door to open position (useful for testing)
    /// </summary>
    [ContextMenu("Force Open")]
    public void ForceOpen()
    {
        StopAllCoroutines();
        isAnimating = false;
        isOpen = true;
        transform.localRotation = openRotation;
        StartAutoCloseTimer();
    }
    
    void OnDestroy()
    {
        StopAutoCloseTimer();
    }
}
