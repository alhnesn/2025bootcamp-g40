using UnityEngine;

public class Holdable : MonoBehaviour
{
    [Header("Holding Configuration")]
    public Transform holdPositionPoint;   // Child object defining where to hold (position only)
    public Transform holdRotationPoint;   // Child object defining how to hold (rotation only)
    
    [Header("Holding Behavior")]
    public bool useCustomHolding = true;

    public void SetLocalHoldingTransform()
    {
        if (useCustomHolding && holdPositionPoint != null)
        {
            // Get the raw position offset
            Vector3 positionOffset = holdPositionPoint.localPosition;
            
            // Get the rotation we want to apply
            Quaternion finalRotation;
            if (holdRotationPoint != null)
            {
                finalRotation = holdRotationPoint.localRotation;
            }
            else
            {
                finalRotation = Quaternion.identity;
            }
            
            // APPLY THE ROTATION TO THE POSITION OFFSET
            Vector3 rotatedPositionOffset = finalRotation * positionOffset;
            
            // Set the transforms
            transform.localPosition = -rotatedPositionOffset;  // Negate to get hand-to-object offset
            transform.localRotation = Quaternion.Inverse(finalRotation);
        }
        else
        {
            // Default behavior
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }
    
}
