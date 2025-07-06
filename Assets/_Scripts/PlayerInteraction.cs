using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionDistance = 3f;
    public Transform handPosition;
    public float dropDistance = 1.5f;

    private GameObject heldItem = null;
    private Rigidbody heldItemRb = null;

    // A reference to the original layer of the item we picked up.
    private int originalLayer;

    // Helper function to set the layer on an object and all its children.
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
    
    // Your updated Update method - this is great!
    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hitInfo;

        if (heldItem != null)
        {
            if (Physics.Raycast(ray, out hitInfo, interactionDistance))
            {
                if (hitInfo.collider.GetComponent<CuttingBoard>() != null)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        hitInfo.collider.GetComponent<CuttingBoard>().Process(this);
                    }
                }
                // NEW: Check for a Stove
                else if (hitInfo.collider.GetComponent<Stove>() != null)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        // Call the stove's Interact method
                        hitInfo.collider.GetComponent<Stove>().Interact(this);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.E))
                {
                    DropItem();
                }
            } 
            else if (Input.GetKeyDown(KeyCode.E))
            {
                DropItem();
            }
        }
        else // This is the pickup/interact logic
        {
            if (Physics.Raycast(ray, out hitInfo, interactionDistance))
            {
                if (hitInfo.collider.GetComponent<Interactable>() != null)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        PickupItem(hitInfo.collider.gameObject);
                    }
                }
            }
        }
    }
    
    public void PickupItem(GameObject itemToPickup)
    {
        heldItem = itemToPickup;
        heldItemRb = heldItem.GetComponent<Rigidbody>();

        if (heldItemRb != null)
        {
            heldItemRb.isKinematic = true;
        }

        heldItem.transform.SetParent(handPosition);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        // NEW: Change the object to the "HeldItem" layer.
        originalLayer = heldItem.layer; // Store the original layer.
        SetLayerRecursively(heldItem, LayerMask.NameToLayer("HeldItem"));
    }

    public void DropItem()
    {
        // Re-enable the physics.
        if (heldItemRb != null)
        {
            heldItemRb.isKinematic = false;
        }
        
        // Change the object back to its original layer.
        SetLayerRecursively(heldItem, originalLayer);

        // Calculate the drop position in the middle of the screen, in front of the player.
        Vector3 dropPosition = playerCamera.transform.position + (playerCamera.transform.forward * dropDistance);

        // Un-parent the item from the hand.
        heldItem.transform.SetParent(null);
        
        // Set the item's position to the calculated drop position.
        heldItem.transform.position = dropPosition;

        // Clear our "held item" variables.
        heldItem = null;
        heldItemRb = null;
    }

    // These other methods remain the same.
    public bool IsHoldingItem() { return heldItem != null; }
    public GameObject GetHeldItem() { return heldItem; }
    public void DestroyHeldItem()
    {
        if (heldItem != null)
        {
            Destroy(heldItem);
            heldItem = null;
            heldItemRb = null;
        }
    }
}