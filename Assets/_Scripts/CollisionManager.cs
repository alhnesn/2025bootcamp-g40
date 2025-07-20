using System.Collections.Generic;
using UnityEngine;

public class CollisionManager
{
    /// <summary>
    /// Ignore or restore collisions between two GameObjects and all their children
    /// </summary>
    public static void SetCollisionBetweenObjects(GameObject obj1, GameObject obj2, bool ignore)
    {
        if (obj1 == null || obj2 == null) return;
        
        Collider[] colliders1 = obj1.GetComponentsInChildren<Collider>();
        Collider[] colliders2 = obj2.GetComponentsInChildren<Collider>();
        
        foreach (Collider col1 in colliders1)
        {
            foreach (Collider col2 in colliders2)
            {
                if (col1 != null && col2 != null)
                {
                    Physics.IgnoreCollision(col1, col2, ignore);
                }
            }
        }
    }

    /// <summary>
    /// Ignore or restore collisions between one object and a list of other objects
    /// </summary>
    public static void SetCollisionBetweenObjectAndList(GameObject target, List<GameObject> objectList, bool ignore)
    {
        if (target == null || objectList == null) return;
        
        foreach (GameObject obj in objectList)
        {
            if (obj != null && obj != target)
            {
                SetCollisionBetweenObjects(target, obj, ignore);
            }
        }
    }

    /// <summary>
    /// Ignore or restore collisions between one object and an array of other objects
    /// </summary>
    public static void SetCollisionBetweenObjectAndArray(GameObject target, GameObject[] objectArray, bool ignore)
    {
        if (target == null || objectArray == null) return;
        
        foreach (GameObject obj in objectArray)
        {
            if (obj != null && obj != target)
            {
                SetCollisionBetweenObjects(target, obj, ignore);
            }
        }
    }
}
