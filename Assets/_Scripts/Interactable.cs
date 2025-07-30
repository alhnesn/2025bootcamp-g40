using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Highlighting")]
    public bool useOutlineHighlight = true;
    
    // Outline highlighting
    private Outline outline;
    
    // State
    private bool isHighlighted = false;
    
    void Start()
    {
        SetupHighlighting();
    }

    private void SetupHighlighting()
    {
        if (useOutlineHighlight)
        {
            // Get or add Outline component
            outline = GetComponent<Outline>();
            if (outline == null)
            {
                outline = gameObject.AddComponent<Outline>();
                
                // Default outline settings
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.OutlineColor = Color.white;
                outline.OutlineWidth = 5f;
            }
            
            // Start disabled
            outline.enabled = false;
        }
    }

    public void StartHighlight()
    {
        if (isHighlighted) return;
        
        isHighlighted = true;
        
        if (useOutlineHighlight && outline != null)
        {
            outline.enabled = true;
        }
    }

    public void StopHighlight()
    {
        if (!isHighlighted) return;
        
        isHighlighted = false;
        
        if (useOutlineHighlight && outline != null)
        {
            outline.enabled = false;
        }
    }

    public bool IsHighlighted()
    {
        return isHighlighted;
    }

    public void Interact()
    {
        // This is where the magic will happen.
        // For now, we'll just print a message to the console.
        Debug.Log("Interacted with " + gameObject.name);
    }
}