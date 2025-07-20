using UnityEngine;

public enum CookingState { Raw, Cooked, Burnt }

public class Cookable : MonoBehaviour
{
    [Header("Cooking Properties")]
    public CookingState currentCookingState = CookingState.Raw;
    public float nextTime = 5f;        // Time to go to next state
    
    [Header("Cooking Results")]
    public GameObject nextPrefab;     // What this becomes when cooked for 1 state
    

    // Get the next cooking stage prefab
    public GameObject GetNextCookingStage()
    {
        return nextPrefab;
    }

    // Get the cooking time for the current stage
    public float GetCurrentCookingTime() // TODO: might rename this bcs it's confusing
    {
        return nextTime;
    }

    // Check if this item can cook further
    public bool CanCookFurther()
    {
        return currentCookingState != CookingState.Burnt && GetNextCookingStage() != null;
    }

}
