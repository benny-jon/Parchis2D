using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardDefinition boardDefinition;
    public BoardView boardView;

    public List<Piece> allPieces;

    private int currentPlayerIndex = 0;
    private int lastDiceRoll = 0;

    private bool waitingForMove = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ResetPieces();
        StartTurn();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ResetPieces()
    {
        if (allPieces.Count != boardView.pieceSpawnPoints.Length)
        {
            Debug.LogError("Pieces count doesnt match Spawn points count");
            return;
        }

        for (int i = 0; i < allPieces.Count; i++)
        {
            allPieces[i].MoveToStart(boardView.pieceSpawnPoints[i].position);
        }
    }

    void StartTurn()
    {
        RollDice();
        waitingForMove = true;
        Debug.Log($"Player {currentPlayerIndex} rolled {lastDiceRoll}");
    }

    void RollDice()
    {
        lastDiceRoll = Random.Range(1, 7);
    }

    public void EndTurn()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % 4;
        StartTurn();
    }

    public void OnPieceClicked(Piece piece)
    {
        if (!waitingForMove) {
            Debug.Log("There was not move in progress");
            return;
        }

        if (piece.ownerPlayerIndex != currentPlayerIndex) { 
            Debug.Log("Not your piece");
            return; // other players pieces
        }

        int targetIndex = piece.currentTileIndex + lastDiceRoll;

        // TODO handle captures, blocks, extra turns, etc;
        if (IsMoveValid(piece, targetIndex))
        {
            Debug.Log($"Move {piece.ToString()} to {targetIndex}");
            piece.MoveToTile(targetIndex, boardView);
            // TODO handle captures, blocks, extra turns, etc;
            waitingForMove = false;
            EndTurn();
        } else
        {
            Debug.Log($"Invalid Move {piece.ToString()} to {targetIndex}");
        }
    }

    bool IsMoveValid(Piece piece, int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= boardDefinition.tiles.Count) return false;
        return true;
    }
}
