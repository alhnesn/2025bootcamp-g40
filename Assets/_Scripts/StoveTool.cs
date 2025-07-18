using UnityEngine;

public class StoveTool : MonoBehaviour
{
    [Header("Stove Placement Configuration")]
    public Transform stovePositionPoint;   // Where this tool touches the stove
    public Transform stoveRotationPoint;   // How this tool should be oriented on stove
    
    [Header("Placement Behavior")]
    public bool useCustomPlacement = true;
    
    // Reference to the stove this tool is currently on (null if not on stove)
    private Stove currentStove = null;
    private int currentCookingSpot = -1;

    public void SetLocalStovePlacement()
    {
        if (useCustomPlacement && stovePositionPoint != null)
        {
            // Get the raw position offset
            Vector3 positionOffset = stovePositionPoint.localPosition;
            
            // Get the rotation we want to apply
            Quaternion finalRotation;
            if (stoveRotationPoint != null)
            {
                finalRotation = stoveRotationPoint.localRotation;
            }
            else
            {
                finalRotation = Quaternion.identity;
            }
            
            // Apply the rotation to the position offset
            Vector3 rotatedPositionOffset = finalRotation * positionOffset;
            
            // Set the transforms
            transform.localPosition = -rotatedPositionOffset;
            transform.localRotation = Quaternion.Inverse(finalRotation);
        }
        else
        {
            // Default behavior - center on cooking spot
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
    }

    // Getters and setters for stove connection
    public void SetStoveConnection(Stove stove, int spotIndex)
    {
        currentStove = stove;
        currentCookingSpot = spotIndex;
    }

    public void ClearStoveConnection()
    {
        currentStove = null;
        currentCookingSpot = -1;
    }

    public bool IsOnStove()
    {
        return currentStove != null;
    }
    
    public Stove GetCurrentStove()
    {
        return currentStove;
    }
}
