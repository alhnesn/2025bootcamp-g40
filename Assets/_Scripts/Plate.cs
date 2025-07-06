using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    public Transform stackingPoint;

    private List<GameObject> stackedIngredients = new List<GameObject>();
    private float currentTopOfStack;

    void Start()
    {
        // No change here, this part is correct.
        Collider plateCollider = GetComponent<Collider>();
        currentTopOfStack = stackingPoint.position.y + (plateCollider.bounds.size.y / 2f);
    }

    // A helper function to change an object's layer. We'll need this for the second fix.
    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    public bool AddIngredient(GameObject ingredientObject)
    {
        Ingredient ingredient = ingredientObject.GetComponent<Ingredient>();
        if (ingredient == null || !IsCorrectIngredient(ingredient))
        {
            Debug.Log("Wrong ingredient or order!");
            return false;
        }

        Collider ingredientCollider = ingredientObject.GetComponent<Collider>();
        if (ingredientCollider == null) return false;

        // --- REVISED PLACEMENT LOGIC ---
        // This logic is now based on the collider's bounds, not the transform's pivot.
        float ingredientHeight = ingredientCollider.bounds.size.y;
        Vector3 ingredientCenterOffset = ingredientObject.transform.position - ingredientCollider.bounds.center;

        Vector3 placementPosition = new Vector3(
            stackingPoint.position.x,
            currentTopOfStack + (ingredientHeight / 2f),
            stackingPoint.position.z
        ) + ingredientCenterOffset;

        ingredientObject.transform.position = placementPosition;
        ingredientObject.transform.rotation = stackingPoint.rotation;
        ingredientObject.transform.SetParent(this.transform);
        if (ingredientObject.GetComponent<Rigidbody>() != null)
        {
            ingredientObject.GetComponent<Rigidbody>().isKinematic = true;
        }

        currentTopOfStack += ingredientHeight;
        stackedIngredients.Add(ingredientObject);

        // This is for Fix #2: Move the placed ingredient to a new layer so it doesn't block clicks.
        SetLayerRecursively(ingredientObject, LayerMask.NameToLayer("PlacedItem"));

        Debug.Log(ingredient.ingredientName + " added to the plate!");
        return true;
    }

    private bool IsCorrectIngredient(Ingredient ingredient)
    {
        int currentStep = stackedIngredients.Count;
        switch (currentStep)
        {
            case 0: return ingredient.ingredientName == "Bun";
            case 1: return ingredient.ingredientName == "BurgerPatty" && ingredient.currentState == IngredientState.Cooked;
            case 2: return ingredient.ingredientName == "Bun";
            default: return false;
        }
    }
}