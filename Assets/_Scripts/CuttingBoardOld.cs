// using UnityEngine;

// public class CuttingBoard : MonoBehaviour
// {
//     // We now have two spots for the resulting items.
//     public Transform placementPoint1;
//     public Transform placementPoint2;

//     public void Process(PlayerInteraction player)
//     {
//         if (!player.IsHoldingItem()) return;

//         GameObject heldItem = player.GetHeldItem();
//         Ingredient ingredient = heldItem.GetComponent<Ingredient>();

//         // Check if the item is a whole "BreadLoaf".
//         if (ingredient != null && ingredient.ingredientName == "BunFull" && ingredient.currentState == IngredientState.Whole)
//         {
//             Debug.Log("Slicing the bread into TopBun and BottomBun.");

//             // You'll need to create separate TopBun and BottomBun prefabs
//             // and reference them in your LoafBread ingredient
            
//             // For now, we can get references from the LoafBread ingredient
//             // You'll need to add these fields to your LoafBread prefab's Ingredient component
//             FullBunIngredient fullBunIngredient = heldItem.GetComponent<FullBunIngredient>();
            
//             if (fullBunIngredient != null && fullBunIngredient.bunTopPrefab != null && fullBunIngredient.bunBottomPrefab != null)
//             {
//                 // Destroy the whole loaf
//                 player.DestroyHeldItem();

//                 // Create TopBun and BottomBun
//                 Instantiate(fullBunIngredient.bunTopPrefab, placementPoint1.position, placementPoint1.rotation);
//                 Instantiate(fullBunIngredient.bunBottomPrefab, placementPoint2.position, placementPoint2.rotation);
//             }
//         }
//         // This is the original logic for other items like the tomato.
//         else if (ingredient != null && ingredient.currentState == IngredientState.Whole)
//         {
//             Debug.Log("Chopping the " + ingredient.ingredientName);
//             if (ingredient.processedPrefab != null)
//             {
//                 player.DestroyHeldItem();
//                 GameObject choppedItem = Instantiate(ingredient.processedPrefab);
//                 player.PickupItem(choppedItem);
//             }
//         }
//     }
// }