using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    // PUBLIC FIELDS (visible in Inspector)
    public Transform plateTop;  // A child GameObject marking where ingredients go
    public Transform ingredientParent;
    
    // PRIVATE FIELDS (internal logic only)
    private List<GameObject> stackedIngredients;  // This stores our ingredient stack

    void Start()
    {
        // Initialize the list - this creates an empty list that can hold GameObjects
        stackedIngredients = new List<GameObject>();
    }

    public bool AddIngredient(GameObject ingredient)
    {
        Stackable stackableComponent = ingredient.GetComponent<Stackable>();
        if (stackableComponent == null || !stackableComponent.canStackOnOthers)
        {
            Debug.Log("This item cannot be stacked!");
            return false; // Don't add non-stackable items
        }
        
        // Add the ingredient to our list
        stackedIngredients.Add(ingredient);
        
        // Calculate the position using stacking points
        Vector3 newPosition = CalculateStackPosition(ingredient);
        // Check if stacking was allowed
        if (newPosition == Vector3.zero)
        {
            // Remove from list since we can't stack it
            stackedIngredients.RemoveAt(stackedIngredients.Count - 1);
            Debug.Log("Cannot stack this item - the top item doesn't allow stacking!");
            return false;
        }
        ingredient.transform.position = newPosition;

        // Reset the rotation to sit properly on the plate
        ingredient.transform.rotation = plateTop.rotation;
        
        // Make the ingredient a child of the plate so it moves with the plate
        ingredient.transform.SetParent(ingredientParent);
        
        // Set the ingredient to the 'PlacedItem' layer to prevent collisions
        SetLayerRecursively(ingredient, LayerMask.NameToLayer("PlacedItem"));
        
        // Remove Interactable component so it can't be picked up directly
        Interactable interactableComponent = ingredient.GetComponent<Interactable>();
        if (interactableComponent != null)
        {
            Destroy(interactableComponent);
        }

        // Make sure the ingredient doesn't fall through physics
        Rigidbody ingredientRb = ingredient.GetComponent<Rigidbody>();
        if (ingredientRb != null)
        {
            ingredientRb.isKinematic = true;
        }

        return true;
    }

    public GameObject TakeTopIngredient()
    {
        // Check if there are any ingredients on the plate
        if (stackedIngredients.Count == 0)
        {
            return null; // No ingredients to take
        }
        
        // Get the topmost ingredient (last item in the list)
        GameObject topIngredient = stackedIngredients[stackedIngredients.Count - 1];
        
        // Remove it from our list
        stackedIngredients.RemoveAt(stackedIngredients.Count - 1);
        
        // REPLACE complex layer restoration:
        SetLayerRecursively(topIngredient, LayerMask.NameToLayer("Default"));
        
        // Remove parent relationship so it's no longer attached to the plate
        topIngredient.transform.SetParent(null);
        
        // Restore the Interactable component so it can be picked up normally
        if (topIngredient.GetComponent<Interactable>() == null)
        {
            topIngredient.AddComponent<Interactable>();
        }

        // Restore physics so it can be picked up normally
        Rigidbody ingredientRb = topIngredient.GetComponent<Rigidbody>();
        if (ingredientRb != null)
        {
            ingredientRb.isKinematic = false;
        }
        
        return topIngredient;
    }

    private Vector3 CalculateStackPosition(GameObject newIngredient)
    {
        Stackable newStackable = newIngredient.GetComponent<Stackable>();
        
        // Start from the plate surface
        Vector3 basePosition = plateTop.position;

        Vector3 bottomOffset = Vector3.zero;
        
        // If this is the first item, place it directly on the plate
        if (stackedIngredients.Count == 1)
        {
            // Calculate the offset from ingredient center to its bottom point
            
            if (newStackable != null && newStackable.bottomPoint != null)
            {
                bottomOffset = newIngredient.transform.position - newStackable.GetBottomPosition();
            }
            
            return basePosition + bottomOffset;
        }
        
        // Check if the topmost item allows stacking on top
        GameObject topMostItem = stackedIngredients[stackedIngredients.Count - 2]; // -2 because we already added the new item
        Stackable topMostStackable = topMostItem.GetComponent<Stackable>();
        
        if (topMostStackable == null || !topMostStackable.canBeStackedOn)
        {
            Debug.Log("Cannot stack on top of " + topMostItem.name + " - stacking not allowed!");
            return Vector3.zero; // This will need to be handled in AddIngredient
        }
        
        // If we can stack, use the topmost item's top position
        Vector3 topPosition = topMostStackable.GetTopPosition();
        
        // Calculate offset between new ingredient's bottom point and its center
        if (newStackable != null && newStackable.bottomPoint != null)
        {
            bottomOffset = newIngredient.transform.position - newStackable.GetBottomPosition();
        }
        
        return topPosition + bottomOffset;
    }

    private float CalculateStackHeight()
    {
        float totalHeight = 0f;
        
        // Loop through all ingredients except the one we just added
        for (int i = 0; i < stackedIngredients.Count - 1; i++)
        {
            GameObject ingredient = stackedIngredients[i];
            
            // Get the ingredient's height using its Renderer bounds
            Renderer renderer = ingredient.GetComponent<Renderer>();
            if (renderer != null)
            {
                totalHeight += renderer.bounds.size.y;
            }
        }
        
        return totalHeight;
    }

    // Helper method to set layer on object and all its children
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        
        obj.layer = newLayer;
        
        // Set the layer for all child objects too
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}