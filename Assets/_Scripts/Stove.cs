using UnityEngine;

public class Stove : MonoBehaviour
{
    [Header("Cooking Spots")]
    public CookingSpot[] cookingSpots;
    
    void Start()
    {
        // Auto-find cooking spots if not assigned
        if (cookingSpots == null || cookingSpots.Length == 0)
        {
            cookingSpots = GetComponentsInChildren<CookingSpot>();
        }
        
        Debug.Log($"Stove initialized with {cookingSpots.Length} cooking spots");
    }
    
    // Optional utility methods
    public bool HasAvailableSpots()
    {
        foreach (CookingSpot spot in cookingSpots)
        {
            if (spot != null && !spot.IsOccupied())
            {
                return true;
            }
        }
        return false;
    }
    
    public int GetAvailableSpotCount()
    {
        int count = 0;
        foreach (CookingSpot spot in cookingSpots)
        {
            if (spot != null && !spot.IsOccupied())
            {
                count++;
            }
        }
        return count;
    }
}
