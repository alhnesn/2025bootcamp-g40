// TODO: Stove-Pan system

using System.Collections.Generic;
using UnityEngine;

public class Pan : MonoBehaviour
{
    [Header("Pan Setup")]
    public Transform cookingSpot;  // The child object you created (inside point of pan)
    
    [Header("Stove Connection")]
    public bool isOnStove = false;
    
    // Track what's cooking in the pan
    private GameObject currentFood = null;
    private bool isCooking = false;
    private float currentCookTime = 0f;

    //------------------------------------------------

    public bool AddFood(GameObject food)
    {
        // Only allow one item at a time in the pan
        if (currentFood != null)
        {
            Debug.Log("Pan is already occupied!");
            return false;
        }
        
        // No restrictions - any item can be put in the pan
        currentFood = food;
    
        // Position the food using its bottom point if it has a Stackable component
        PositionFoodInPan();
        
        currentFood.transform.SetParent(transform);
        
        // Set layer to PlacedItem
        SetLayerRecursively(currentFood, LayerMask.NameToLayer("PlacedItem"));
        
        // Make it kinematic so it doesn't fall out
        Rigidbody foodRb = currentFood.GetComponent<Rigidbody>();
        if (foodRb != null)
        {
            foodRb.isKinematic = true;
        }
        
        // Remove interactable so it can't be picked up directly
        Interactable interactable = currentFood.GetComponent<Interactable>();
        if (interactable != null)
        {
            Destroy(interactable);
        }
        
        // Start cooking if we're on a stove
        if (isOnStove)
        {
            StartCooking();
        }

        return true;
    }

    public GameObject RemoveFood()
    {
        if (currentFood == null) return null;
        
        GameObject food = currentFood;
        
        // Stop cooking
        StopCooking();
        
        // REPLACE complex layer restoration with simple reset:
        SetLayerRecursively(food, LayerMask.NameToLayer("Default"));

        // Restore interactable component
        if (food.GetComponent<Interactable>() == null)
        {
            food.AddComponent<Interactable>();
        }
        
        // Restore physics
        Rigidbody foodRb = food.GetComponent<Rigidbody>();
        if (foodRb != null)
        {
            foodRb.isKinematic = false;
        }
        
        food.transform.SetParent(null);
        currentFood = null;
        
        return food;
    }

    private void PositionFoodInPan()
    {
        if (currentFood == null) return;
        
        // Try to use bottom point from Stackable component
        Stackable stackable = currentFood.GetComponent<Stackable>();
        if (stackable != null && stackable.bottomPoint != null)
        {
            // Calculate offset from center to bottom point
            Vector3 bottomOffset = currentFood.transform.position - stackable.GetBottomPosition();
            currentFood.transform.position = cookingSpot.position + bottomOffset;
        }
        else
        {
            // Fallback to center positioning
            currentFood.transform.position = cookingSpot.position;
        }
        
        currentFood.transform.rotation = cookingSpot.rotation;
    }

    public void StartCooking()
    {
        Cookable cookable = currentFood.GetComponent<Cookable>();
        if (cookable != null && cookable.CanCookFurther())
        {
            isCooking = true;
            currentCookTime = 0f;
            Debug.Log("Started cooking " + currentFood.name + " (" + cookable.currentCookingState + ")");
        }
        else
        {
            Debug.Log("This food cannot cook further!");
        }
    }
    
    public void StopCooking()
    {
        isCooking = false;
        currentCookTime = 0f;
        Debug.Log("Stopped cooking");
    }

    void Update()
    {
        if (isCooking && currentFood != null)
        {
            Cookable cookable = currentFood.GetComponent<Cookable>();
            if (cookable != null)
            {
                currentCookTime += Time.deltaTime;
                
                // Check if food is done cooking to next stage
                if (currentCookTime >= cookable.GetCurrentCookingTime())
                {
                    CookFood();
                }
            }
        }
    }

    private void CookFood()
    {
        if (currentFood == null) return;
        
        Cookable cookable = currentFood.GetComponent<Cookable>();
        if (cookable != null)
        {
            GameObject nextStage = cookable.GetNextCookingStage();
            if (nextStage != null)
            {
                // Destroy current food
                Destroy(currentFood);
                
                // Create next stage
                currentFood = Instantiate(nextStage);
                currentFood.transform.SetParent(transform);
                
                // Position the new food properly
                PositionFoodInPan();

                SetLayerRecursively(currentFood, LayerMask.NameToLayer("PlacedItem"));
                
                // Make it kinematic
                Rigidbody foodRb = currentFood.GetComponent<Rigidbody>();
                if (foodRb != null)
                {
                    foodRb.isKinematic = true;
                }
                
                // Remove interactable
                Interactable interactable = currentFood.GetComponent<Interactable>();
                if (interactable != null)
                {
                    Destroy(interactable);
                }
                
                Debug.Log("Food progressed to next cooking stage!");
                
                // Continue cooking if it can cook further
                Cookable newCookable = currentFood.GetComponent<Cookable>();
                if (newCookable != null && newCookable.CanCookFurther())
                {
                    currentCookTime = 0f; // Reset timer for next stage
                }
                else
                {
                    StopCooking(); // Stop if fully burnt
                }
            }
        }
    }

    // Helper method to set layer recursively (like in Plate.cs)
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
    
    public bool HasFood()
    {
        return currentFood != null;
    }

}
