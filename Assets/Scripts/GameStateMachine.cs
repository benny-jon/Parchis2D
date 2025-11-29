using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateMachine
{
    private GamePhase phase;
    private int currentPlayerIndex;
    private int lastDiceRoll;

    private readonly List<Piece> pieces;
    private readonly BoardRules boardRules;
    private readonly BoardView boardView;

    public Action<int> OnDiceRolled;
    public Action<int> OnTurnChanged;
    public Action OnMoveStarted;
    public Action OnMoveEnded;

    public GameStateMachine(List<Piece> pieces, BoardView boardView, BoardRules boardRules)
    {
        this.pieces = pieces;
        this.boardRules = boardRules;
        this.boardView = boardView;

        ResetGame();
    }

    public void StartGame()
    {
        StartTurn();
    }

    private void StartTurn()
    {
        phase = GamePhase.WaitingForRoll;
        OnTurnChanged?.Invoke(currentPlayerIndex);

        // // hack for testing
        // if (currentPlayerIndex > 0)
        // {
        //     NextPlayer();
        // }
    }

    public void RollDice()
    {
        if (phase != GamePhase.WaitingForRoll)
        {
            Debug.Log("It's not time to roll the dice");
            return;
        }

        lastDiceRoll = UnityEngine.Random.Range(1, 7);
        OnDiceRolled?.Invoke(lastDiceRoll);

        if (HasAnyLegalMoveForCurrentPlayer())
        {
            phase = GamePhase.WaitingForMove;
            Debug.Log($"Player {currentPlayerIndex}, pick a Piece to move {lastDiceRoll} with roll {lastDiceRoll}!");
        }
        else
        {
            Debug.Log($"Player {currentPlayerIndex} has no legal moves with roll {lastDiceRoll}. Skipping turn.");
            NextPlayer();
        }
    }

    public void OnPieceClicked(Piece piece)
    {
        if (phase != GamePhase.WaitingForMove)
        {
            Debug.Log("We are not waiting for move yet!");
            return;
        }
        if (piece.ownerPlayerIndex != currentPlayerIndex)
        {
            Debug.Log("That's not your piece");
            return;
        }

        int targetIndex = boardRules.TryGetTargetTileIndex(piece, lastDiceRoll);
        if (targetIndex == -1)
        {
            Debug.Log($"{piece} CANNOT move {lastDiceRoll} times");
            return;
        }

        OnMoveStarted?.Invoke();
        Debug.Log($"Moving to tile from {piece.currentTileIndex} to {targetIndex}");
        piece.MoveToTile(targetIndex, boardView);
        OnMoveEnded?.Invoke();

        // TODO check captures & win conditions

        NextPlayer();
    }

    private bool HasAnyLegalMoveForCurrentPlayer()
    {
        foreach (var piece in pieces)
        {
            if (piece == null) continue;
            if (piece.ownerPlayerIndex != currentPlayerIndex) continue;

            if (boardRules.TryGetTargetTileIndex(piece, lastDiceRoll) != -1)
            {
                return true;
            }
        }
        return false;
    }

    private void NextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % BoardDefinition.PLAYERS;
        StartTurn();
    }

    public void ResetGame()
    {
        phase = GamePhase.WaitingForRoll;
        currentPlayerIndex = 0;
    }
}
