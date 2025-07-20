using System.Collections.Generic;
using UnityEngine;

public class Container : MonoBehaviour
{
    [Header("Container Settings")]
    public int maxItems = -1;  // -1 for infinite
    public Transform stackingPoint;  // Where to stack items (if null, uses this transform)
    
    // Track items in this container (acts as a stack)
    private List<GameObject> itemStack = new List<GameObject>();
    
    // Events
    public System.Action<GameObject> OnItemAdded;
    public System.Action<GameObject> OnItemRemoved;

    public System.Action<GameObject, GameObject> OnItemAddedToContainer;    // (container, item)
    public System.Action<GameObject, GameObject> OnItemRemovedFromContainer; // (container, item)


    /// <summary>
    /// Add item to top of stack
    /// </summary>
    public bool TryAddItem(GameObject item)
    {
        // Check capacity
        if (maxItems != -1 && itemStack.Count >= maxItems)
        {
            Debug.Log($"Container {gameObject.name} is full! (Max: {maxItems})");
            return false;
        }
        
        // Check if item has Stackable component
        Stackable stackable = item.GetComponent<Stackable>();
        if (stackable == null)
        {
            Debug.Log($"Item {item.name} cannot be stacked (no Stackable component)!");
            return false;
        }
        
        // Add to stack
        itemStack.Add(item);
        
        // Set up item
        SetupStackedItem(item);
        
        // Position the item
        PositionItemOnStack(item); // TODO: if the item is not a Stackable, it thinks it added it succesfully 
        
        // Notify
        OnItemAdded?.Invoke(item);
        OnItemAddedToContainer?.Invoke(gameObject, item);  // NEW
        
        return true;
    }

    /// <summary>
    /// Remove an item from this container
    /// </summary>
    public GameObject RemoveItem(GameObject item)
    {
        if (!itemStack.Contains(item))
        {
            return null;
        }
        
        itemStack.Remove(item);
        
        // Clean up item
        CleanupStackedItem(item);
        
        // Notify specific container
        OnItemRemoved?.Invoke(item);
        
        return item;
    }

    /// <summary>
    /// Remove and return the topmost item
    /// </summary>
    public GameObject TakeTopItem()
    {
        if (itemStack.Count == 0)
        {
            return null;
        }
        
        GameObject topItem = itemStack[itemStack.Count - 1];
        itemStack.RemoveAt(itemStack.Count - 1);
        
        // Clean up item
        CleanupStackedItem(topItem);
        
        // Notify
        OnItemRemoved?.Invoke(topItem);
        OnItemRemovedFromContainer?.Invoke(gameObject, topItem);  // NEW
        
        return topItem;
    }

    /// <summary>
    /// Remove item by index
    /// </summary>
    public GameObject RemoveItemAt(int index)
    {
        if (index < 0 || index >= itemStack.Count)
        {
            return null;
        }
        
        GameObject item = itemStack[index];
        return RemoveItem(item);
    }

    /// <summary>
    /// Calculate stack height up to (but not including) the specified item
    /// </summary>
    private float CalculateStackHeight(GameObject excludeItem = null)
    {
        float totalHeight = 0f;
        
        foreach (GameObject item in itemStack)
        {
            if (item == excludeItem) break;  // Stop before this item
            
            if (item != null)
            {
                Renderer renderer = item.GetComponent<Renderer>();
                if (renderer != null)
                {
                    totalHeight += renderer.bounds.size.y;
                }
            }
        }
        
        return totalHeight;
    }

    /// <summary>
    /// Position item on the stack using Stackable points
    /// </summary>
    private void PositionItemOnStack(GameObject item)
    {
        Stackable stackable = item.GetComponent<Stackable>();
        if (stackable == null) return;
        
        Transform stackBase = stackingPoint != null ? stackingPoint : transform;
        
        // Calculate total height of items below this one
        float stackHeight = CalculateStackHeight(item);
        
        // Calculate position using bottom point
        Vector3 bottomOffset = Vector3.zero;
        if (stackable.bottomPoint != null)
        {
            bottomOffset = item.transform.position - stackable.GetBottomPosition();
        }
        
        Vector3 targetPosition = stackBase.position + Vector3.up * stackHeight + bottomOffset;
        item.transform.position = targetPosition;
        item.transform.rotation = stackBase.rotation;
    }

    /// <summary>
    /// Refresh collision state for all items in container (fixes stove removal bug)
    /// </summary>
    public void RefreshInternalCollisionState()
    {
        // Re-establish collision ignoring between all items and container
        foreach (GameObject item in itemStack)
        {
            if (item != null)
            {
                CollisionManager.SetCollisionBetweenObjects(item, gameObject, true);
                CollisionManager.SetCollisionBetweenObjectAndList(item, itemStack, true);
            }
        }
    }

    private void SetupStackedItem(GameObject item)
    {
        // Parent to container
        item.transform.SetParent(transform);
        
        // Collision management
        CollisionManager.SetCollisionBetweenObjects(item, gameObject, true);
        CollisionManager.SetCollisionBetweenObjectAndList(item, itemStack, true);
        
        // Make kinematic
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = true;
        }
        
        // Remove interactable
        Interactable interactable = item.GetComponent<Interactable>();
        if (interactable != null)
        {
            Destroy(interactable);
        }
    }

    private void CleanupStackedItem(GameObject item)
    {
        // Restore collisions
        CollisionManager.SetCollisionBetweenObjects(item, gameObject, false);
        CollisionManager.SetCollisionBetweenObjectAndList(item, itemStack, false);
        
        // Restore physics
        Rigidbody itemRb = item.GetComponent<Rigidbody>();
        if (itemRb != null)
        {
            itemRb.isKinematic = false;
        }
        
        // Restore interactable
        if (item.GetComponent<Interactable>() == null)
        {
            item.AddComponent<Interactable>();
        }
        
        // Remove parent
        item.transform.SetParent(null);
    }

    // Getters
    public int GetItemCount() => itemStack.Count;
    public bool IsFull() => maxItems != -1 && itemStack.Count >= maxItems;
    public bool IsEmpty() => itemStack.Count == 0;
    public GameObject GetTopItem() => itemStack.Count > 0 ? itemStack[itemStack.Count - 1] : null;
    public List<GameObject> GetAllItems() => new List<GameObject>(itemStack);

}
