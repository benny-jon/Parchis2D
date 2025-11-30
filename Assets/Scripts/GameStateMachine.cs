using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameStateMachine
{
    private GamePhase phase;
    private int currentPlayerIndex;
    private Dictionary<Piece, List<MoveOption>> currentLegalMoves = new Dictionary<Piece, List<MoveOption>>();
    private int lastDice1Roll;
    private int lastDice2Roll;
    private bool dice1Used;
    private bool dice2Used;

    private readonly List<Piece> pieces;
    private readonly BoardRules boardRules;
    private readonly BoardView boardView;

    public Action<int, int> OnDiceRolled;
    public Action OnAvailableMovesUpdated;
    public Action<int> OnTurnChanged;
    public Action OnMoveStarted;
    public Action OnMoveEnded;

    public IReadOnlyDictionary<Piece, List<MoveOption>> CurrentLegalMoves => currentLegalMoves;

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
    }

    public void RollDice()
    {
        if (phase != GamePhase.WaitingForRoll)
        {
            Debug.Log("It's not time to roll the dice");
            return;
        }

        dice1Used = false;
        dice2Used = false;
        lastDice1Roll = UnityEngine.Random.Range(1, 7);
        lastDice2Roll = UnityEngine.Random.Range(1, 7);
        OnDiceRolled?.Invoke(lastDice1Roll, lastDice2Roll);

        int totalMoves = CalculateLegalMovesForCurrentPlayer();

        if (totalMoves == 0)
        {
            Debug.Log($"Player {currentPlayerIndex} has no legal moves with roll {lastDice1Roll}, {lastDice2Roll}. Skipping turn.");
            NextPlayer();
            return;
        }

        if (totalMoves == 1)
        {
            // Auto-move
            Debug.Log("Auto moving piece");
            var onlyOption = currentLegalMoves.First().Value.First();
            ExecuteMoveOption(onlyOption);

            NextPlayer();
            return;
        }

        phase = GamePhase.WaitingForMove;
        Debug.Log($"Player {currentPlayerIndex}, pick a Piece with available moves!");
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

        if (!currentLegalMoves.TryGetValue(piece, out var options) || options.Count == 0)
        {
            Debug.Log($"{piece} has NO legal moves.");
            return;
        }

        MoveOption chosenOption;

        if (options.Count == 1)
        {
            chosenOption = options[0];
        }
        else
        {
            // TODO show options to the user so they can tap on one.
            chosenOption = options.First();
        }

        ExecuteMoveOption(chosenOption);
    }

    private void ExecuteMoveOption(MoveOption moveOption)
    {
        if (moveOption.usesDice1) dice1Used = true;
        if (moveOption.usesDice2) dice2Used = true;

        OnMoveStarted?.Invoke();
        Debug.Log($"Moving {moveOption.piece} from Tiles {moveOption.piece.currentTileIndex} to {moveOption.targetTileIndex}");
        moveOption.piece.MoveToTile(moveOption.targetTileIndex, boardView);
        OnMoveEnded?.Invoke();

        // TODO check captures & win conditions

        if (dice1Used && dice2Used)
        {
            NextPlayer();
            return;
        }

        int totalMoves = CalculateLegalMovesForCurrentPlayer();

        if (totalMoves == 0)
        {
            NextPlayer();
            return;
        }

        if (totalMoves == 1)
        {
            Debug.Log("Auto moving piece");
            var onlyMove = currentLegalMoves.First().Value.First();
            ExecuteMoveOption(onlyMove);
            return;
        }

        phase = GamePhase.WaitingForMove;
        Debug.Log($"Player {currentPlayerIndex}, pick a Piece with available moves!");
    }

    private int CalculateLegalMovesForCurrentPlayer()
    {
        currentLegalMoves.Clear();

        int totalMoves = 0;

        foreach (Piece piece in pieces)
        {
            if (piece.ownerPlayerIndex != currentPlayerIndex) continue;

            var options = new List<MoveOption>();

            if (lastDice1Roll > 0 && !dice1Used)
            {
                int targetIndex = boardRules.TryGetTargetTileIndex(piece, lastDice1Roll);
                if (targetIndex != -1)
                {
                    options.Add(new MoveOption(piece, targetIndex, lastDice1Roll, true, false));
                    Debug.Log($"Available Move: {options.Last()}");
                }
            }

            if (lastDice2Roll > 0 && !dice2Used)
            {
                int targetIndex = boardRules.TryGetTargetTileIndex(piece, lastDice2Roll);
                if (targetIndex != -1)
                {
                    options.Add(new MoveOption(piece, targetIndex, lastDice2Roll, false, true));
                    Debug.Log($"Available Move: {options.Last()}");
                }
            }

            if (lastDice1Roll > 0 && lastDice2Roll > 0 && !dice1Used && !dice2Used)
            {
                int targetIndex = boardRules.TryGetTargetTileIndex(piece, lastDice1Roll + lastDice2Roll);
                if (targetIndex != -1)
                {
                    options.Add(new MoveOption(piece, targetIndex, lastDice1Roll + lastDice2Roll, true, true));
                    Debug.Log($"Available Move: {options.Last()}");
                }
            }

            if (options.Count > 0)
            {
                currentLegalMoves[piece] = options;
                totalMoves += options.Count;
            }
        }

        OnAvailableMovesUpdated?.Invoke();

        return totalMoves;
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
