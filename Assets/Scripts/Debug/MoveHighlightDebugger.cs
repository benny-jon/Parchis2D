using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveHighlightDebugger : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private MoveHighlightLayer moveHighlightLayer;
    [SerializeField] private BoardDefinition boardDefinition;
    [SerializeField] private Piece piecePrefab;

    private readonly List<MoveOption> allTilesOptions = new();
    private Piece piece;

    private void Setup()
    {
        piece = CreateTestPiece(0, -1);
        if (boardDefinition != null)
        {
            for (int i = 0; i < boardDefinition.tiles.Count; i++)
            {
                allTilesOptions.Add(new MoveOption(piece, boardDefinition.tiles[i].index, 1, 0));
            }
        }
    }

    void Update()
    {
        if (isActiveAndEnabled && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (allTilesOptions.Count == 0)
            {
                Debug.Log($"Setup highlight debugger!");
                Setup();   
            }
            else
            {
                Debug.Log($"Show {allTilesOptions.Count} highlights");
                moveHighlightLayer.ShowHighlights(allTilesOptions.ToArray());
            }
        }
    }

    private Piece CreateTestPiece(int owner, int tileIndex)
    {
        Piece piece = Instantiate(piecePrefab, transform);
        piece.transform.position = new Vector3(0, 0, 0);
        piece.ownerPlayerIndex = 0;
        piece.playerPieceIndex = 0;
        piece.MoveToTile(-1);
        return piece;
    }
}
