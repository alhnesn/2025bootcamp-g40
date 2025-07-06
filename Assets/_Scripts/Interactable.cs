using UnityEngine;

public class Interactable : MonoBehaviour
{
    public void Interact()
    {
        // This is where the magic will happen.
        // For now, we'll just print a message to the console.
        Debug.Log("Interacted with " + gameObject.name);
    }
}