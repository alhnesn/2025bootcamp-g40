using System.Collections.Generic;
using UnityEngine;

public class Plate : MonoBehaviour
{
    [Header("Plate Setup")]
    public Transform plateTop;  // This becomes the stacking point
    
    private Container container;

    void Start()
    {
        container = GetComponent<Container>();
        if (container == null)
        {
            Debug.LogError("Plate requires a Container component!");
            return;
        }
    }

    // Optional wrapper methods for clarity (you can remove these if you prefer)
    public bool AddIngredient(GameObject ingredient)
    {
        return container.TryAddItem(ingredient);
    }

    public GameObject TakeTopIngredient()
    {
        return container.TakeTopItem();
    }

    public bool IsEmpty()
    {
        return container.IsEmpty();
    }
}