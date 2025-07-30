using UnityEngine;

public class StoveTool : MonoBehaviour
{
    [Header("Stove Placement Configuration")]
    public Transform stovePositionPoint;   // Where this tool touches the stove
    public Transform stoveRotationPoint;   // How this tool should be oriented on stove
    
    [Header("Placement Behavior")]
    public bool useCustomPlacement = true;
    
    // Cooking spot connection (simplified)
    private CookingSpot currentCookingSpot = null;
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

    // Simplified cooking spot connection
    public void SetCookingSpotConnection(CookingSpot cookingSpot)
    {
        currentCookingSpot = cookingSpot;
        
        // If we already have food when placed, start cooking
        if (container != null && !container.IsEmpty())
        {
            currentCookingSpot.NotifyFoodAdded();
        }
    }

    public void ClearCookingSpotConnection()
    {
        // Stop any cooking before disconnecting
        if (currentCookingSpot != null && container != null && !container.IsEmpty())
        {
            currentCookingSpot.NotifyFoodRemoved();
        }
        
        currentCookingSpot = null;
    }

    // Event handlers for container integration
    private void OnItemAddedToContainer(GameObject containerObj, GameObject item)
    {
        if (IsOnCookingSpot())
        {
            currentCookingSpot.NotifyFoodAdded();
            Debug.Log($"Notified cooking spot that food {item.name} was added");
        }
    }
    
    private void OnItemRemovedFromContainer(GameObject containerObj, GameObject item)
    {
        if (IsOnCookingSpot())
        {
            currentCookingSpot.NotifyFoodRemoved();
            Debug.Log($"Notified cooking spot that food {item.name} was removed");
        }
    }

    // Getters
    public bool IsOnCookingSpot()
    {
        return currentCookingSpot != null;
    }
    
    public CookingSpot GetCurrentCookingSpot()
    {
        return currentCookingSpot;
    }
}
