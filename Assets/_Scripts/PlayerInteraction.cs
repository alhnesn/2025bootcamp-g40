using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera playerCamera;
    public float interactionDistance = 3f;
    public Transform handPosition;
    public float dropDistance = 1.5f;
    public LayerMask interactableLayers;

    private GameObject heldItem = null;
    private Rigidbody heldItemRb = null;

    // A reference to the original layer of the item we picked up.
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();

    // Store disabled colliders to re-enable them later
    private List<Collider> disabledColliders = new List<Collider>();
    
    
    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, interactionDistance, interactableLayers))
        {
            if (heldItem != null) // HANDS FULL
            {
                // Check for container to place item on
                Container container = hitInfo.collider.GetComponent<Container>();
                if (container != null && Input.GetMouseButtonDown(0))
                {
                    if (container.TryAddItem(heldItem))
                    {
                        heldItem = null;
                        heldItemRb = null;
                    }
                    return;
                }
                
                Stove stove = hitInfo.collider.GetComponent<Stove>();
                if (stove != null && Input.GetMouseButtonDown(0))
                {
                    if (stove.PlaceTool(heldItem))
                    {
                        heldItem = null;
                        heldItemRb = null;
                    }
                    return;
                }

                // Check for stations or drop
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hitInfo.collider.GetComponent<CuttingBoard>() != null)
                    {
                        hitInfo.collider.GetComponent<CuttingBoard>().Process(this);
                    }
                    else
                    {
                        DropItem();
                    }
                }
            }
            else // HANDS EMPTY
            {
                Container container = hitInfo.collider.GetComponent<Container>();
                if (container != null && Input.GetMouseButtonDown(0))
                {
                    GameObject topItem = container.TakeTopItem();
                    if (topItem != null)
                    {
                        PickupItem(topItem);
                    }
                }

                Stove stove = hitInfo.collider.GetComponent<Stove>();
                if (stove != null && Input.GetMouseButtonDown(0))
                {
                    GameObject tool = stove.RemoveToolFromPosition(hitInfo.point);
                    if (tool != null)
                    {
                        PickupItem(tool);
                    }
                }

                else if (hitInfo.collider.GetComponent<Interactable>() != null)
                {
                    // Regular item pickup
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        PickupItem(hitInfo.collider.gameObject);
                    }
                }
            }
        }
        else if (heldItem != null && Input.GetKeyDown(KeyCode.E))
        {
            // Drop item if looking at nothing
            DropItem();
        }
    }
    
    public void PickupItem(GameObject itemToPickup)
    {
        // Check if item is holdable
        Holdable holdable = itemToPickup.GetComponent<Holdable>();
        if (holdable == null)
        {
            Debug.Log("This item cannot be picked up!");
            return; // Exit early if not holdable
        }
        
        heldItem = itemToPickup;
        heldItemRb = heldItem.GetComponent<Rigidbody>();

        if (heldItemRb != null)
        {
            heldItemRb.isKinematic = true;
        }

        // Set parent first
        heldItem.transform.SetParent(handPosition);
        
        // Use the holdable component
        holdable.SetLocalHoldingTransform();

        // Disable colliders
        DisableCollidersRecursively(heldItem);
    }

    public void DropItem()
    {
        if (heldItem == null) return;

        if (heldItemRb != null)
        {
            heldItemRb.isKinematic = false;
        }
        
        // RestoreLayerRecursively(heldItem); 
        
        // NEW: Re-enable colliders instead of restoring layers
        EnableDisabledColliders();

        Vector3 dropPosition = playerCamera.transform.position + (playerCamera.transform.forward * dropDistance);

        heldItem.transform.SetParent(null);
        
        heldItem.transform.position = dropPosition;

        heldItem = null;
        heldItemRb = null;
    }

    // NEW: Helper methods for collider management
    private void DisableCollidersRecursively(GameObject obj)
    {
        if (obj == null) return;
        
        // Disable collider on this object
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null && collider.enabled)
        {
            collider.enabled = false;
            disabledColliders.Add(collider);
        }
        
        // Disable colliders on all children
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            DisableCollidersRecursively(child.gameObject);
        }
    }

    private void EnableDisabledColliders()
    {
        // Re-enable all colliders we disabled
        foreach (Collider collider in disabledColliders)
        {
            if (collider != null) // Check if collider still exists
            {
                collider.enabled = true;
            }
        }
        
        // Clear the list
        disabledColliders.Clear();
    }


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