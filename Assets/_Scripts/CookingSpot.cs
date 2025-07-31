using UnityEngine;

public class CookingSpot : MonoBehaviour
{
    [Header("Cooking Spot Settings")]
    public Transform toolPlacement;  // Where the tool sits (optional, uses this transform if null)
    public bool isOccupied = false;
    
    // Current tool and cooking state
    private GameObject currentTool = null;
    private bool isCooking = false;
    private float currentCookTime = 0f;

    void Update()
    {
        if (isCooking && currentTool != null)
        {
            UpdateCooking();
        }
    }

    public bool PlaceTool(GameObject tool)
    {
        if (isOccupied)
        {
            Debug.Log("Cooking spot is already occupied!");
            return false;
        }
        
        // Check if tool has StoveTool component
        StoveTool stoveTool = tool.GetComponent<StoveTool>();
        if (stoveTool == null)
        {
            Debug.Log("This item cannot be placed on the cooking spot!");
            return false;
        }
        
        // Place the tool
        currentTool = tool;
        isOccupied = true;
        
        Transform placementPoint = toolPlacement != null ? toolPlacement : transform;
        tool.transform.SetParent(placementPoint);
        stoveTool.SetLocalStovePlacement();
        
        // Collision management
        CollisionManager.SetCollisionBetweenObjects(tool, gameObject, true);

        // Make kinematic and remove interactable
        Rigidbody toolRb = tool.GetComponent<Rigidbody>();
        if (toolRb != null)
        {
            toolRb.isKinematic = true;
        }
        
        Interactable interactable = tool.GetComponent<Interactable>();
        if (interactable != null)
        {
            Destroy(interactable);
        }
        
        // Connect tool to this cooking spot
        stoveTool.SetCookingSpotConnection(this);
        
        // Start cooking if tool has food
        StartCooking();
        
        Debug.Log($"Placed {tool.name} on cooking spot");
        return true;
    }

    public GameObject RemoveTool()
    {
        if (!isOccupied || currentTool == null)
        {
            return null;
        }
        
        GameObject tool = currentTool;
        
        // Stop cooking
        StopCooking();
        
        // Clear state
        currentTool = null;
        isOccupied = false;
        
        // Disconnect from cooking spot
        StoveTool stoveTool = tool.GetComponent<StoveTool>();
        if (stoveTool != null)
        {
            stoveTool.ClearCookingSpotConnection();
        }
        
        // Restore collisions
        CollisionManager.SetCollisionBetweenObjects(tool, gameObject, false);
        
        // Refresh container collision state
        Container container = tool.GetComponent<Container>();
        if (container != null)
        {
            container.RefreshInternalCollisionState();
        }
        
        // Restore interactable and physics
        if (tool.GetComponent<Interactable>() == null)
        {
            tool.AddComponent<Interactable>();
        }
        
        Rigidbody toolRb = tool.GetComponent<Rigidbody>();
        if (toolRb != null)
        {
            toolRb.isKinematic = false;
        }
        
        tool.transform.SetParent(null);
        
        Debug.Log($"Removed {tool.name} from cooking spot");
        return tool;
    }

    // Cooking Logic (moved from Stove)
    public void StartCooking()
    {
        if (currentTool == null) return;
        
        Container container = currentTool.GetComponent<Container>();
        if (container != null && !container.IsEmpty())
        {
            GameObject food = container.GetTopItem();
            Cookable cookable = food.GetComponent<Cookable>();
            
            if (cookable != null && cookable.CanCookFurther())
            {
                isCooking = true;
                currentCookTime = 0f;
                Debug.Log($"Started cooking {food.name} ({cookable.currentCookingState})");
            }
        }
    }
    
    public void StopCooking()
    {
        if (isCooking)
        {
            isCooking = false;
            currentCookTime = 0f;
            Debug.Log("Stopped cooking");
        }
    }

    private void UpdateCooking()
    {
        Container container = currentTool.GetComponent<Container>();
        if (container == null || container.IsEmpty())
        {
            StopCooking();
            return;
        }
        
        GameObject food = container.GetTopItem();
        if (food == null)
        {
            StopCooking();
            return;
        }
        
        Cookable cookable = food.GetComponent<Cookable>();
        if (cookable == null || !cookable.CanCookFurther())
        {
            StopCooking();
            return;
        }
        
        // Progress cooking
        currentCookTime += Time.deltaTime;
        
        // Check if cooking stage is complete
        if (currentCookTime >= cookable.GetCurrentCookingTime())
        {
            CookFood(container, food, cookable);
        }
    }

    private void CookFood(Container container, GameObject currentFood, Cookable cookable)
    {
        GameObject nextStage = cookable.GetNextCookingStage();
        if (nextStage == null)
        {
            StopCooking();
            return;
        }
        
        // Remove current food from container
        GameObject removedFood = container.TakeTopItem();
        
        // Destroy the old food object
        if (removedFood != null)
        {
            Destroy(removedFood);
        }
        
        // Create new cooked version
        GameObject cookedFood = Instantiate(nextStage);
        
        // Add cooked food back to container
        container.TryAddItem(cookedFood);
        
        Debug.Log("Food progressed to next cooking stage!");
        
        // Check if we can continue cooking
        Cookable newCookable = cookedFood.GetComponent<Cookable>();
        if (newCookable != null && newCookable.CanCookFurther())
        {
            // Reset timer for next cooking stage
            currentCookTime = 0f;
            Debug.Log($"Continuing to cook {cookedFood.name}");
        }
        else
        {
            // Stop cooking - food is fully cooked or burnt
            StopCooking();
            Debug.Log($"Finished cooking {cookedFood.name}");
        }
    }

    // Public method for tool to notify about food changes
    public void NotifyFoodAdded() // eger ocakta olan tavaya yemek koyulursa diye
    {
        StartCooking();
    }
    
    public void NotifyFoodRemoved()
    {
        StopCooking();
    }
    
    // Getters
    public bool IsOccupied() => isOccupied;
    public GameObject GetCurrentTool() => currentTool;
    public bool IsCooking() => isCooking;
}
