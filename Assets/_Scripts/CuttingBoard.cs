using UnityEngine;

public class CuttingBoard : MonoBehaviour
{
    // This is the main interaction function. We'll call this from the player.
    public void Process(PlayerInteraction player)
    {
        // First, check if the player is actually holding an item.
        if (!player.IsHoldingItem())
        {
            Debug.Log("Player is not holding anything.");
            return;
        }

        // Get the Ingredient component from the item the player is holding.
        GameObject heldItem = player.GetHeldItem();
        Ingredient ingredient = heldItem.GetComponent<Ingredient>();

        // Check if the held item is a valid ingredient and if it's in the "Whole" state.
        if (ingredient != null && ingredient.currentState == IngredientState.Whole)
        {
            Debug.Log("Processing the " + ingredient.ingredientName);

            // Check if a "processed" version of this ingredient exists.
            if (ingredient.processedPrefab != null)
            {
                // Destroy the "Whole" ingredient object.
                player.DestroyHeldItem();

                // Create the new "Chopped" ingredient.
                GameObject choppedItem = Instantiate(ingredient.processedPrefab);

                // Tell the player to pick up the new chopped item.
                player.PickupItem(choppedItem);
            }
        }
        else
        {
            Debug.Log("This item cannot be chopped!");
        }
    }
}