// using UnityEngine;

// public class CuttingBoard : MonoBehaviour
// {
//     // This is the main interaction function. We'll call this from the player.
//     public void Process(PlayerInteraction player)
//     {
//         // First, check if the player is actually holding an item.
//         if (!player.IsHoldingItem())
//         {
//             Debug.Log("Player is not holding anything.");
//             return;
//         }

//         // Get the Ingredient component from the item the player is holding.
//         GameObject heldItem = player.GetHeldItem();
//         Ingredient ingredient = heldItem.GetComponent<Ingredient>();

//         // Check if the held item is a valid ingredient and if it's in the "Whole" state.
//         if (ingredient != null && ingredient.currentState == IngredientState.Whole)
//         {
//             Debug.Log("Processing the " + ingredient.ingredientName);

//             // Check if a "processed" version of this ingredient exists.
//             if (ingredient.processedPrefab != null)
//             {
//                 // Destroy the "Whole" ingredient object.
//                 player.DestroyHeldItem();

//                 // Create the new "Chopped" ingredient.
//                 GameObject choppedItem = Instantiate(ingredient.processedPrefab);

//                 // Tell the player to pick up the new chopped item.
//                 player.PickupItem(choppedItem);
//             }
//         }
//         else
//         {
//             Debug.Log("This item cannot be chopped!");
//         }
//     }
// }

using UnityEngine;

public class CuttingBoard : MonoBehaviour
{
    // We now have two spots for the resulting items.
    public Transform placementPoint1;
    public Transform placementPoint2;

    public void Process(PlayerInteraction player)
    {
        if (!player.IsHoldingItem()) return;

        GameObject heldItem = player.GetHeldItem();
        Ingredient ingredient = heldItem.GetComponent<Ingredient>();

        // Check if the item is a whole "BreadLoaf".
        if (ingredient != null && ingredient.ingredientName == "BreadLoaf" && ingredient.currentState == IngredientState.Whole)
        {
            Debug.Log("Slicing the BreadLoaf.");

            // Get the "TopBun" prefab from the ingredient script.
            GameObject BunPrefab = ingredient.processedPrefab;

            if (BunPrefab != null)
            {
                // Destroy the whole loaf the player was holding.
                player.DestroyHeldItem();

                // Create the top and bottom buns and place them on the board.
                Instantiate(BunPrefab, placementPoint1.position, placementPoint1.rotation);
                Instantiate(BunPrefab, placementPoint2.position, placementPoint2.rotation);
            }
        }
        // This is the original logic for other items like the tomato.
        else if (ingredient != null && ingredient.currentState == IngredientState.Whole)
        {
            Debug.Log("Chopping the " + ingredient.ingredientName);
            if (ingredient.processedPrefab != null)
            {
                player.DestroyHeldItem();
                GameObject choppedItem = Instantiate(ingredient.processedPrefab);
                player.PickupItem(choppedItem);
            }
        }
    }
}