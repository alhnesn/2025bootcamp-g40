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

    // Helper function to set the layer on an object and all its children.
    private void StoreAndSetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        
        // Store the original layer before changing it
        originalLayers[obj] = obj.layer;
        obj.layer = newLayer;
        
        // Do the same for all children
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            StoreAndSetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private void RestoreLayerRecursively(GameObject obj)
    {
        if (obj == null) return;
        
        // Restore the original layer if we have it stored
        if (originalLayers.ContainsKey(obj))
        {
            obj.layer = originalLayers[obj];
            originalLayers.Remove(obj); // Clean up the dictionary
        }
        
        // Do the same for all children
        foreach (Transform child in obj.transform)
        {
            if (child == null) continue;
            RestoreLayerRecursively(child.gameObject);
        }
    }
    
    // Your updated Update method - this is great!
    void Update()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, interactionDistance, interactableLayers))
        {
            if (heldItem != null) // HANDS FULL
            {
                // Check for plate to place item on
                Plate plate = hitInfo.collider.GetComponent<Plate>();
                if (plate != null && Input.GetMouseButtonDown(0))
                {
                    plate.AddIngredient(heldItem);
                    heldItem = null; // We no longer hold the item
                    heldItemRb = null;
                    return; // Interaction complete
                }
                Pan pan = hitInfo.collider.GetComponent<Pan>();
                if (pan != null && Input.GetMouseButtonDown(0))
                {
                    pan.AddFood(heldItem);
                    heldItem = null;
                    heldItemRb = null;
                    return;
                }

                // Check for stations or drop
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (hitInfo.collider.GetComponent<CuttingBoard>() != null)
                    {
                        hitInfo.collider.GetComponent<CuttingBoard>().Process(this);
                    }
                    else if (hitInfo.collider.GetComponent<Stove>() != null)
                    {
                        hitInfo.collider.GetComponent<Stove>().Interact(this);
                    }
                    else
                    {
                        DropItem();
                    }
                }
            }
            else // HANDS EMPTY
            {
                Plate plate = hitInfo.collider.GetComponent<Plate>();
                if (plate != null)
                {
                    // Interaction with a plate
                    if (Input.GetMouseButtonDown(0)) // Left Click to take top item
                    {
                        GameObject topItem = plate.TakeTopIngredient();
                        if (topItem != null)
                        {
                            PickupItem(topItem);
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.E)) // 'E' to pick up whole plate
                    {
                        PickupItem(hitInfo.collider.gameObject);
                    }
                }

                Pan pan = hitInfo.collider.GetComponent<Pan>();
                if (pan != null && Input.GetMouseButtonDown(0))
                {
                    GameObject food = pan.RemoveFood();
                    if (food != null)
                    {
                        PickupItem(food);
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
        heldItem = itemToPickup;
        heldItemRb = heldItem.GetComponent<Rigidbody>();

        if (heldItemRb != null)
        {
            heldItemRb.isKinematic = true;
        }

        heldItem.transform.SetParent(handPosition);
        heldItem.transform.localPosition = Vector3.zero;
        heldItem.transform.localRotation = Quaternion.identity;

        StoreAndSetLayerRecursively(heldItem, LayerMask.NameToLayer("HeldItem"));
    }

    public void DropItem()
    {
        if (heldItem == null) return;

        if (heldItemRb != null)
        {
            heldItemRb.isKinematic = false;
        }
        
        RestoreLayerRecursively(heldItem); 

        Vector3 dropPosition = playerCamera.transform.position + (playerCamera.transform.forward * dropDistance);

        heldItem.transform.SetParent(null);
        
        heldItem.transform.position = dropPosition;

        heldItem = null;
        heldItemRb = null;
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