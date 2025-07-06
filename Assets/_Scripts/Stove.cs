using System.Collections;
using UnityEngine;

public class Stove : MonoBehaviour
{
    public Transform cookingSpot; // The spot on the stove where the burger will sit
    public float cookingTime = 5f; // How many seconds it takes to cook

    private bool isCooking = false;
    private GameObject currentFood;

    // The main interaction function
    public void Interact(PlayerInteraction player)
    {
        // If the stove is busy or the player isn't holding anything, do nothing.
        if (isCooking || !player.IsHoldingItem())
        {
            return;
        }

        GameObject heldItem = player.GetHeldItem();
        Ingredient ingredient = heldItem.GetComponent<Ingredient>();

        // Check if the player is holding a "Whole" uncooked burger patty
        if (ingredient != null && ingredient.ingredientName == "BurgerPatty" && ingredient.currentState == IngredientState.Whole)
        {
            // Take the item from the player
            player.DestroyHeldItem();
            currentFood = Instantiate(heldItem, cookingSpot.position, cookingSpot.rotation);
            currentFood.GetComponent<Rigidbody>().isKinematic = true;

            // Start the cooking process
            StartCoroutine(CookItem(ingredient.processedPrefab));
        }
    }

    private IEnumerator CookItem(GameObject cookedPrefab)
    {
        isCooking = true;
        Debug.Log("Cooking has started...");

        // Wait for the cooking time to pass
        yield return new WaitForSeconds(cookingTime);

        // After cooking is done, replace the raw food with the cooked version
        Destroy(currentFood);
        currentFood = Instantiate(cookedPrefab, cookingSpot.position, cookingSpot.rotation);
        
        // Add an Interactable script to the cooked food so it can be picked up
        if (currentFood.GetComponent<Interactable>() == null)
        {
            currentFood.AddComponent<Interactable>();
        }
        
        Debug.Log("Food is cooked!");
        isCooking = false;
    }
}