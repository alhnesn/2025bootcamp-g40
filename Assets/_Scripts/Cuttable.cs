using UnityEngine;

[System.Serializable]
public class CuttablePiece
{
    public GameObject prefab;
    public string pieceName;  // Optional, for debugging
}

public class Cuttable : MonoBehaviour
{
    [Header("Cutting Configuration")]
    public CuttablePiece[] resultingPieces;
    
    [Header("Cutting Board Placement")]
    public bool useCustomPlacement = false;
    public Vector3[] customPositions;  // Optional: manual positioning for special cases

    public int GetPieceCount()
    {
        return resultingPieces != null ? resultingPieces.Length : 0;
    }

    public GameObject GetPiece(int index)
    {
        if (resultingPieces != null && index >= 0 && index < resultingPieces.Length)
        {
            return resultingPieces[index].prefab;
        }
        return null;
    }

    public string GetPieceName(int index)
    {
        if (resultingPieces != null && index >= 0 && index < resultingPieces.Length)
        {
            return resultingPieces[index].pieceName;
        }
        return "Unknown";
    }
    
    public bool CanBeCut()
    {
        return resultingPieces != null && resultingPieces.Length > 0;
    }
}
