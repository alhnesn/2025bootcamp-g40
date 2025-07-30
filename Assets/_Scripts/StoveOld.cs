// using UnityEngine;

// public class StoveOld : MonoBehaviour
// {
//     [Header("Cooking Spots")]
//     public Transform[] cookingSpots;  // Array of cooking spot transforms (CookingSpot1, 2, 3, 4)
    
//     [Header("Stove Settings")]
//     public float detectionRadius = 10f;  // How close to place tools on spots
    
//     // Track what's on each cooking spot
//     private GameObject[] toolsOnSpots;
    
//     // Track cooking state for each spot
//     private bool[] isCookingAtSpot;
//     private float[] cookingTimeAtSpot;

//     void Start()
//     {
//         // Initialize arrays based on cooking spots
//         if (cookingSpots != null)
//         {
//             int spotCount = cookingSpots.Length;
//             toolsOnSpots = new GameObject[spotCount];
//             isCookingAtSpot = new bool[spotCount];
//             cookingTimeAtSpot = new float[spotCount];
//         }
//     }

//     void Update()
//     {
//         // Handle cooking for each spot
//         for (int i = 0; i < cookingSpots.Length; i++)
//         {
//             if (isCookingAtSpot[i])
//             {
//                 UpdateCookingAtSpot(i);
//             }
//         }
//     }

//     public bool PlaceTool(GameObject tool)
//     {
//         // Check if tool has StoveTool component
//         StoveTool stoveTool = tool.GetComponent<StoveTool>();
//         if (stoveTool == null)
//         {
//             Debug.Log("This item cannot be placed on the stove!");
//             return false;
//         }
        
//         // Find the closest available cooking spot
//         int spotIndex = FindBestCookingSpot(tool.transform.position);
//         if (spotIndex == -1)
//         {
//             Debug.Log("No available cooking spots!");
//             return false;
//         }
        
//         // Place the tool on the cooking spot
//         toolsOnSpots[spotIndex] = tool;
        
//         // Set up the tool's transform
//         tool.transform.SetParent(cookingSpots[spotIndex]);
//         stoveTool.SetLocalStovePlacement();
        
//         // SIMPLIFIED: Two lines instead of multiple methods
//         CollisionManager.SetCollisionBetweenObjects(tool, gameObject, true);
//         CollisionManager.SetCollisionBetweenObjectAndArray(tool, toolsOnSpots, true);
        
//         // Make it kinematic
//         Rigidbody toolRb = tool.GetComponent<Rigidbody>();
//         if (toolRb != null)
//         {
//             toolRb.isKinematic = true;
//         }
        
//         // Remove interactable so it can't be picked up directly // TODO: both 'Interactable' and 'Holdable' controls whether an item can be picked up or not. This should only be controlled by 'Holdable'
//         Interactable interactable = tool.GetComponent<Interactable>();
//         if (interactable != null)
//         {
//             Destroy(interactable);
//         }
        
//         // Connect the tool to this stove
//         stoveTool.SetStoveConnection(this, spotIndex);
        
//         Debug.Log($"Placed {tool.name} on cooking spot {spotIndex + 1}");
//         return true;
//     }

//     public GameObject RemoveTool(int spotIndex)
//     {
//         if (spotIndex < 0 || spotIndex >= toolsOnSpots.Length || toolsOnSpots[spotIndex] == null)
//         {
//             return null;
//         }
        
//         GameObject tool = toolsOnSpots[spotIndex];
//         toolsOnSpots[spotIndex] = null;

//         // Stop cooking at this spot
//         StopCookingAtSpot(spotIndex);

//         toolsOnSpots[spotIndex] = null;
        
//         // Disconnect from stove
//         StoveTool stoveTool = tool.GetComponent<StoveTool>();
//         if (stoveTool != null)
//         {
//             stoveTool.ClearStoveConnection();
//         }

//         // SIMPLIFIED: Two lines for collision restoration
//         CollisionManager.SetCollisionBetweenObjects(tool, gameObject, false);
//         CollisionManager.SetCollisionBetweenObjectAndArray(tool, toolsOnSpots, false);

//         // ADDED: Refresh container's internal collision state
//         Container container = tool.GetComponent<Container>();
//         if (container != null)
//         {
//             container.RefreshInternalCollisionState();
//         }
        
//         // Restore interactable
//         if (tool.GetComponent<Interactable>() == null)
//         {
//             tool.AddComponent<Interactable>();
//         }
        
//         // Restore physics
//         Rigidbody toolRb = tool.GetComponent<Rigidbody>();
//         if (toolRb != null)
//         {
//             toolRb.isKinematic = false;
//         }
        
//         tool.transform.SetParent(null);
        
//         Debug.Log($"Removed {tool.name} from cooking spot {spotIndex + 1}");
//         return tool;
//     }


//     public GameObject RemoveToolFromPosition(Vector3 worldPosition)
//     {
//         // Find which cooking spot is closest to the world position
//         int spotIndex = FindClosestCookingSpot(worldPosition);
//         if (spotIndex != -1 && toolsOnSpots[spotIndex] != null)
//         {
//             return RemoveTool(spotIndex);
//         }
//         return null;
//     }

//     // NEW: Cooking Logic
//     private void StartCookingAtSpot(int spotIndex)
//     {
//         GameObject tool = toolsOnSpots[spotIndex];
//         if (tool == null) return;
        
//         // Check if tool has food that can be cooked
//         Container container = tool.GetComponent<Container>();
//         if (container != null && !container.IsEmpty())
//         {
//             GameObject food = container.GetTopItem();
//             Cookable cookable = food.GetComponent<Cookable>();
            
//             if (cookable != null && cookable.CanCookFurther())
//             {
//                 isCookingAtSpot[spotIndex] = true;
//                 cookingTimeAtSpot[spotIndex] = 0f;
//                 Debug.Log($"Started cooking {food.name} at spot {spotIndex + 1} ({cookable.currentCookingState})");
//             }
//         }
//     }

//     private void StopCookingAtSpot(int spotIndex)
//     {
//         if (isCookingAtSpot[spotIndex])
//         {
//             isCookingAtSpot[spotIndex] = false;
//             cookingTimeAtSpot[spotIndex] = 0f;
//             Debug.Log($"Stopped cooking at spot {spotIndex + 1}");
//         }
//     }

//     private void UpdateCookingAtSpot(int spotIndex)
//     {
//         GameObject tool = toolsOnSpots[spotIndex];
//         if (tool == null)
//         {
//             StopCookingAtSpot(spotIndex);
//             return;
//         }
        
//         Container container = tool.GetComponent<Container>();
//         if (container == null || container.IsEmpty())
//         {
//             StopCookingAtSpot(spotIndex);
//             return;
//         }
        
//         GameObject food = container.GetTopItem();
//         if (food == null)
//         {
//             StopCookingAtSpot(spotIndex);
//             return;
//         }
        
//         Cookable cookable = food.GetComponent<Cookable>();
//         if (cookable == null || !cookable.CanCookFurther())
//         {
//             StopCookingAtSpot(spotIndex);
//             return;
//         }
        
//         // Progress cooking
//         cookingTimeAtSpot[spotIndex] += Time.deltaTime;
        
//         // Check if cooking stage is complete
//         if (cookingTimeAtSpot[spotIndex] >= cookable.GetCurrentCookingTime())
//         {
//             CookFoodAtSpot(spotIndex, container, food, cookable);
//         }
//     }

//     private void CookFoodAtSpot(int spotIndex, Container container, GameObject currentFood, Cookable cookable)
//     {
//         GameObject nextStage = cookable.GetNextCookingStage();
//         if (nextStage == null)
//         {
//             StopCookingAtSpot(spotIndex);
//             return;
//         }
        
//         // Remove current food from container
//         GameObject removedFood = container.TakeTopItem();

//         // ADDED: Destroy the old food object
//         if (removedFood != null)
//         {
//             Destroy(removedFood);
//         }
        
//         // Create new cooked version
//         GameObject cookedFood = Instantiate(nextStage);
        
//         // Add cooked food back to container
//         container.TryAddItem(cookedFood);
        
//         Debug.Log($"Food progressed to next cooking stage at spot {spotIndex + 1}!");
        
//         // Check if we can continue cooking
//         Cookable newCookable = cookedFood.GetComponent<Cookable>();
//         if (newCookable != null && newCookable.CanCookFurther())
//         {
//             // Reset timer for next cooking stage
//             cookingTimeAtSpot[spotIndex] = 0f;
//             Debug.Log($"Continuing to cook {cookedFood.name} at spot {spotIndex + 1}");
//         }
//         else
//         {
//             // Stop cooking - food is fully cooked or burnt
//             StopCookingAtSpot(spotIndex);
//             Debug.Log($"Finished cooking {cookedFood.name} at spot {spotIndex + 1}");
//         }
//     }

//     // NEW: Public method to manually start cooking (when food is added to a tool already on stove)
//     public void NotifyFoodAddedToTool(GameObject tool)
//     {
//         // Find which spot this tool is on
//         for (int i = 0; i < toolsOnSpots.Length; i++)
//         {
//             if (toolsOnSpots[i] == tool)
//             {
//                 StartCookingAtSpot(i);
//                 break;
//             }
//         }
//     }

//     // NEW: Public method to stop cooking (when food is removed from a tool on stove)
//     public void NotifyFoodRemovedFromTool(GameObject tool)
//     {
//         // Find which spot this tool is on
//         for (int i = 0; i < toolsOnSpots.Length; i++)
//         {
//             if (toolsOnSpots[i] == tool)
//             {
//                 StopCookingAtSpot(i);
//                 break;
//             }
//         }
//     }

//     private int FindBestCookingSpot(Vector3 toolPosition)
//     {
//         int bestSpot = -1;
//         float closestDistance = float.MaxValue;
        
//         for (int i = 0; i < cookingSpots.Length; i++)
//         {
//             // Skip occupied spots
//             if (toolsOnSpots[i] != null) continue;
            
//             float distance = Vector3.Distance(toolPosition, cookingSpots[i].position);
//             if (distance < closestDistance && distance <= detectionRadius)
//             {
//                 closestDistance = distance;
//                 bestSpot = i;
//             }
//         }
        
//         return bestSpot;
//     }

//     private int FindClosestCookingSpot(Vector3 position)
//     {
//         int closestSpot = -1;
//         float closestDistance = float.MaxValue;
        
//         for (int i = 0; i < cookingSpots.Length; i++)
//         {
//             float distance = Vector3.Distance(position, cookingSpots[i].position);
//             if (distance < closestDistance)
//             {
//                 closestDistance = distance;
//                 closestSpot = i;
//             }
//         }
        
//         return closestSpot;
//     }

//     public bool HasAvailableSpots()
//     {
//         for (int i = 0; i < toolsOnSpots.Length; i++)
//         {
//             if (toolsOnSpots[i] == null) return true;
//         }
//         return false;
//     }
// }
