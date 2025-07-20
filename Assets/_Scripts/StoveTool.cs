using UnityEngine;

public class StoveTool : MonoBehaviour
{
    [Header("Stove Placement Configuration")]
    public Transform stovePositionPoint;   // Where this tool touches the stove
    public Transform stoveRotationPoint;   // How this tool should be oriented on stove
    
    [Header("Placement Behavior")]
    public bool useCustomPlacement = true;
    
    // Reference to the stove this tool is currently on (null if not on stove)
    private Stove currentStove = null;
    private int currentCookingSpot = -1;

    // Container reference for cooking integration
    private Container container;

    
    void Start()
    {
        // Get container component and subscribe to events
        container = GetComponent<Container>();
        if (container != null)
        {
            container.OnItemAddedToContainer += OnItemAddedToContainer;
            container.OnItemRemovedFromContainer += OnItemRemovedFromContainer;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (container != null)
        {
            container.OnItemAddedToContainer -= OnItemAddedToContainer;
            container.OnItemRemovedFromContainer -= OnItemRemovedFromContainer;
        }
    }
    
    public void SetLocalStovePlacement()
    {
        if (useCustomPlacement && stovePositionPoint != null)
        {
            // Get the raw position offset
            Vector3 positionOffset = stovePositionPoint.localPosition;
            
            // Get the rotation we want to apply
            Quaternion finalRotation;
            if (stoveRotationPoint != null)
            {
                finalRotation = stoveRotationPoint.localRotation;
            }
            else
            {
                finalRotation = Quaternion.identity;
            }
            
            // Apply the rotation to the position offset
            Vector3 rotatedPositionOffset = finalRotation * positionOffset;
            
            // Set the transforms
            transform.localPosition = -rotatedPositionOffset;
            transform.localRotation = Quaternion.Inverse(finalRotation);
        }
        else
        {
            // Default behavior - center on cooking spot
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    // Getters and setters for stove connection
    public void SetStoveConnection(Stove stove, int spotIndex)
    {
        currentStove = stove;
        currentCookingSpot = spotIndex;

        // If we already have food when placed on stove, start cooking
        if (container != null && !container.IsEmpty())
        {
            currentStove.NotifyFoodAddedToTool(gameObject);
        }
    }

    public void ClearStoveConnection()
    {
        // Stop any cooking before disconnecting
        if (currentStove != null && container != null && !container.IsEmpty())
        {
            currentStove.NotifyFoodRemovedFromTool(gameObject);
        }

        currentStove = null;
        currentCookingSpot = -1;
    }

    // Event handlers for container integration
    private void OnItemAddedToContainer(GameObject containerObj, GameObject item)
    {
        // Only react if this tool is currently on a stove
        if (IsOnStove())
        {
            currentStove.NotifyFoodAddedToTool(gameObject);
            Debug.Log($"Notified stove that food {item.name} was added to {gameObject.name}");
        }
    }
    
    private void OnItemRemovedFromContainer(GameObject containerObj, GameObject item)
    {
        // Only react if this tool is currently on a stove
        if (IsOnStove())
        {
            currentStove.NotifyFoodRemovedFromTool(gameObject);
            Debug.Log($"Notified stove that food {item.name} was removed from {gameObject.name}");
        }
    }

    public bool IsOnStove()
    {
        return currentStove != null;
    }
    
    public Stove GetCurrentStove()
    {
        return currentStove;
    }
}
