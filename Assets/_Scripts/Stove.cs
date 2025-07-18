using UnityEngine;

public class Stove : MonoBehaviour
{
    [Header("Cooking Spots")]
    public Transform[] cookingSpots;  // Array of cooking spot transforms (CookingSpot1, 2, 3, 4)
    
    [Header("Stove Settings")]
    public float detectionRadius = 0.5f;  // How close to place tools on spots
    
    // Track what's on each cooking spot
    private GameObject[] toolsOnSpots;

    void Start()
    {
        // Initialize arrays based on cooking spots
        if (cookingSpots != null)
        {
            toolsOnSpots = new GameObject[cookingSpots.Length];
        }
    }

    public bool PlaceTool(GameObject tool)
    {
        // Check if tool has StoveTool component
        StoveTool stoveTool = tool.GetComponent<StoveTool>();
        if (stoveTool == null)
        {
            Debug.Log("This item cannot be placed on the stove!");
            return false;
        }
        
        // Find the closest available cooking spot
        int spotIndex = FindBestCookingSpot(tool.transform.position);
        if (spotIndex == -1)
        {
            Debug.Log("No available cooking spots!");
            return false;
        }
        
        // Place the tool on the cooking spot
        toolsOnSpots[spotIndex] = tool;
        
        // Set up the tool's transform
        tool.transform.SetParent(cookingSpots[spotIndex]);
        stoveTool.SetLocalStovePlacement();
        
        // Set layer to PlacedItem
        SetLayerRecursively(tool, LayerMask.NameToLayer("PlacedItem")); // TODO: this should not be like this
        
        // Make it kinematic
        Rigidbody toolRb = tool.GetComponent<Rigidbody>();
        if (toolRb != null)
        {
            toolRb.isKinematic = true;
        }
        
        // Remove interactable so it can't be picked up directly // TODO: what?
        Interactable interactable = tool.GetComponent<Interactable>();
        if (interactable != null)
        {
            Destroy(interactable);
        }
        
        // Connect the tool to this stove
        stoveTool.SetStoveConnection(this, spotIndex);
        
        // Start cooking if the tool supports it
        Pan pan = tool.GetComponent<Pan>();
        if (pan != null)
        {
            pan.SetOnStove(true, this); // TODO: I think the cooking should be made on the stove rather than the pan as the pan is only a tool to store the Cookable
        }
        
        Debug.Log($"Placed {tool.name} on cooking spot {spotIndex + 1}");
        return true;
    }

    public GameObject RemoveTool(int spotIndex)
    {
        if (spotIndex < 0 || spotIndex >= toolsOnSpots.Length || toolsOnSpots[spotIndex] == null)
        {
            return null;
        }
        
        GameObject tool = toolsOnSpots[spotIndex];
        toolsOnSpots[spotIndex] = null;
        
        // Disconnect from stove
        StoveTool stoveTool = tool.GetComponent<StoveTool>();
        if (stoveTool != null)
        {
            stoveTool.ClearStoveConnection();
        }
        
        // Stop cooking
        Pan pan = tool.GetComponent<Pan>();
        if (pan != null)
        {
            pan.SetOnStove(false, null);
        }
        
        // Restore layer
        SetLayerRecursively(tool, LayerMask.NameToLayer("Default")); // TODO: the objects on the pan should stay as 'PlacedItem'
        
        // Restore interactable
        if (tool.GetComponent<Interactable>() == null)
        {
            tool.AddComponent<Interactable>();
        }
        
        // Restore physics
        Rigidbody toolRb = tool.GetComponent<Rigidbody>();
        if (toolRb != null)
        {
            toolRb.isKinematic = false;
        }
        
        tool.transform.SetParent(null);
        
        Debug.Log($"Removed {tool.name} from cooking spot {spotIndex + 1}");
        return tool;
    }


    public GameObject RemoveToolFromPosition(Vector3 worldPosition)
    {
        // Find which cooking spot is closest to the world position
        int spotIndex = FindClosestCookingSpot(worldPosition);
        if (spotIndex != -1 && toolsOnSpots[spotIndex] != null)
        {
            return RemoveTool(spotIndex);
        }
        return null;
    }

    private int FindBestCookingSpot(Vector3 toolPosition)
    {
        int bestSpot = -1;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < cookingSpots.Length; i++)
        {
            // Skip occupied spots
            if (toolsOnSpots[i] != null) continue;
            
            float distance = Vector3.Distance(toolPosition, cookingSpots[i].position);
            if (distance < closestDistance && distance <= detectionRadius)
            {
                closestDistance = distance;
                bestSpot = i;
            }
        }
        
        return bestSpot;
    }

    private int FindClosestCookingSpot(Vector3 position)
    {
        int closestSpot = -1;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < cookingSpots.Length; i++)
        {
            float distance = Vector3.Distance(position, cookingSpots[i].position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpot = i;
            }
        }
        
        return closestSpot;
    }

    // Helper method to set layer recursively
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        
        obj.layer = newLayer;
        
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public bool HasAvailableSpots()
    {
        for (int i = 0; i < toolsOnSpots.Length; i++)
        {
            if (toolsOnSpots[i] == null) return true;
        }
        return false;
    }
}
