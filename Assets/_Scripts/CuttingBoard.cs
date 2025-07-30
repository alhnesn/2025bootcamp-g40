using UnityEngine;

public class CuttingBoard : MonoBehaviour
{
    [Header("Cutting Board Setup")]
    public Transform placementCenter;     // Center of the cutting board
    public float boardWidth = 1.0f;       // Width of the cutting board for positioning
    public float stackHeight = 0.1f;      // Height offset for stacking pieces
    
    public void Process(PlayerInteraction player)
    {
        if (!player.IsHoldingItem()) return;

        GameObject heldItem = player.GetHeldItem();
        Cuttable cuttable = heldItem.GetComponent<Cuttable>();

        // Check if the item can be cut
        if (cuttable == null || !cuttable.CanBeCut())
        {
            Debug.Log("This item cannot be cut!");
            return;
        }

        Debug.Log($"Cutting {heldItem.name} into {cuttable.GetPieceCount()} pieces");

        // Destroy the original item
        player.DestroyHeldItem();

        // Create the cut pieces
        CreateCutPieces(cuttable);
    }

    private void CreateCutPieces(Cuttable cuttable)
    {
        int pieceCount = cuttable.GetPieceCount();
        
        if (pieceCount == 1)
        {
            CreateSinglePiece(cuttable);
        }
        else if (pieceCount == 2)
        {
            CreateTwoPieces(cuttable);
        }
        else if (pieceCount >= 3)
        {
            CreateStackedPieces(cuttable);
        }
    }

    private void CreateSinglePiece(Cuttable cuttable)
    {
        GameObject piece = Instantiate(cuttable.GetPiece(0));
        Vector3 centerPosition = placementCenter != null ? placementCenter.position : transform.position;
        
        piece.transform.position = centerPosition;
        piece.transform.rotation = transform.rotation;
        
        // Add Interactable component
        if (piece.GetComponent<Interactable>() == null)
        {
            piece.AddComponent<Interactable>();
        }
        
        Debug.Log($"Created single piece: {cuttable.GetPieceName(0)}");
    }

    private void CreateTwoPieces(Cuttable cuttable)
    {
        // Create both pieces first to calculate their sizes
        GameObject piece1 = Instantiate(cuttable.GetPiece(0));
        GameObject piece2 = Instantiate(cuttable.GetPiece(1));
        
        // Get their widths (half-widths for positioning)
        float halfWidth1 = GetObjectWidth(piece1) * 0.5f;
        float halfWidth2 = GetObjectWidth(piece2) * 0.5f;
        
        // Calculate positions to fit side by side on the board
        Vector3 centerPosition = placementCenter != null ? placementCenter.position : transform.position;
        Vector3 rightDirection = transform.right;
        
        // Place them so their edges just touch at the center
        Vector3 piece1Position = centerPosition - rightDirection * halfWidth1;
        Vector3 piece2Position = centerPosition + rightDirection * halfWidth2;
        
        piece1.transform.position = piece1Position;
        piece1.transform.rotation = transform.rotation;
        
        piece2.transform.position = piece2Position;
        piece2.transform.rotation = transform.rotation;
        
        // Add Interactable components
        if (piece1.GetComponent<Interactable>() == null)
        {
            piece1.AddComponent<Interactable>();
        }
        if (piece2.GetComponent<Interactable>() == null)
        {
            piece2.AddComponent<Interactable>();
        }
        
        Debug.Log($"Created two pieces: {cuttable.GetPieceName(0)} and {cuttable.GetPieceName(1)}");
    }

    private void CreateStackedPieces(Cuttable cuttable)
    {
        Vector3 centerPosition = placementCenter != null ? placementCenter.position : transform.position;
        float currentHeight = 0f;
        
        for (int i = 0; i < cuttable.GetPieceCount(); i++)
        {
            GameObject piece = Instantiate(cuttable.GetPiece(i));
            
            // Calculate piece height
            float pieceHeight = GetObjectHeight(piece);
            
            // Position piece: current height + half of piece height (so bottom sits on current height)
            Vector3 stackPosition = centerPosition + Vector3.up * (currentHeight + pieceHeight * 0.5f);
            piece.transform.position = stackPosition;
            piece.transform.rotation = transform.rotation;
            
            // Update current height for next piece
            currentHeight += pieceHeight;
            
            // Add Interactable component
            if (piece.GetComponent<Interactable>() == null)
            {
                piece.AddComponent<Interactable>();
            }
            
            Debug.Log($"Created stacked piece {i + 1}: {cuttable.GetPieceName(i)} at height {currentHeight}");
        }
    }

    private float GetObjectWidth(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.x;
        }
        
        // Fallback: check for Collider bounds
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.size.x;
        }
        
        // Default fallback
        return 0.3f;
    }

    private float GetObjectHeight(GameObject obj)
    {
        // First, try to use Stackable component for precise height
        Stackable stackable = obj.GetComponent<Stackable>();
        if (stackable != null && stackable.bottomPoint != null && stackable.topPoint != null)
        {
            // Calculate height from stackable points
            Vector3 bottomPos = stackable.bottomPoint.position;
            Vector3 topPos = stackable.topPoint.position;
            return Vector3.Distance(bottomPos, topPos);
        }
        
        // Fallback to Renderer bounds
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size.y;
        }
        
        // Fallback to Collider bounds
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.size.y;
        }
        
        // Default fallback
        return 0.1f;
    }
}