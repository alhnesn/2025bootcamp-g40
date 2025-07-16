using UnityEngine;

public class Stackable : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Stacking Points")]
    public Transform bottomPoint;  // Where this object's bottom touches other objects
    public Transform topPoint;     // Where other objects should sit on this object
    
    [Header("Stacking Settings")]
    public bool canBeStackedOn = true;      // Can other objects stack on top of this?
    public bool canStackOnOthers = true;

    // Helper methods to get world positions
    public Vector3 GetBottomPosition()
    {
        return bottomPoint != null ? bottomPoint.position : transform.position;
    }
    
    public Vector3 GetTopPosition()
    {
        return topPoint != null ? topPoint.position : transform.position;
    }
} // test
