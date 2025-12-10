using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GameStateMachine
{
    private const int BonusForCapture = 20;
    private const int BonusForReachingHome = 10;

    private GamePhase _phase;
    public GamePhase gamePhase
    {
        get { return _phase; }
        private set
        {
            OnGamePhaseChanged?.Invoke(value);
            _phase = value;
        }
    }
    public int currentPlayerIndex { get; private set; }
    private Dictionary<Piece, List<MoveOption>> currentLegalMoves = new Dictionary<Piece, List<MoveOption>>();
    private int lastDice1Roll;
    private int lastDice2Roll;
    private bool dice1Used;
    private bool dice2Used;
    private int rolledDoublesInARow;
    private List<int> bonusStepsToMove = new List<int>();
    private bool playerHadABlockade;

    private readonly List<Piece> pieces;
    private readonly BoardRules boardRules;
    private readonly BoardView boardView;

    public Action<int, int> OnDiceRolled;
    public Action OnAvailableMovesUpdated;
    public Action<int> OnTurnChanged;
    public Action OnMoveStarted;
    public Action<Piece, List<int>, Action> OnMoveAnimationRequested;
    public Action OnMoveEnded;
    public Action<GamePhase> OnGamePhaseChanged;
    public Action<Piece> OnMovePieceToStart;
    public Action<int> OnPlayerFinishedTheGame;

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

    public void EndGame()
    {
        gamePhase = GamePhase.GameOver;
    }

    private void StartTurn()
    {
        gamePhase = GamePhase.WaitingForRoll;
        OnTurnChanged?.Invoke(currentPlayerIndex);
    }

    public void RollDice()
    {
        if (gamePhase != GamePhase.WaitingForRoll)
        {
            Debug.Log("It's not time to roll the dice");
            return;
        }

        int d1 = UnityEngine.Random.Range(1, 7);
        int d2 = UnityEngine.Random.Range(1, 7);

        RollDiceWithValues(d1, d2);
    }

    public void RollDiceWithValues(int d1, int d2)
    {
        playerHadABlockade = false;
        dice1Used = false;
        dice2Used = false;
        lastDice1Roll = d1;
        lastDice2Roll = d2;
        OnDiceRolled?.Invoke(lastDice1Roll, lastDice2Roll);
        if (d1 == d2)
        {
            rolledDoublesInARow++;
            if (PenaltyForRollingDoubles())
            {
                NextPlayer();
                return;
            }
        }

        int totalMoves = CalculateLegalMovesForCurrentPlayer();

        if (totalMoves == 0)
        {
            if (lastDice1Roll == lastDice2Roll && !playerHadABlockade)
            {
                gamePhase = GamePhase.WaitingForRoll;
                Debug.Log($"Player {currentPlayerIndex} can roll again!");
                return;
            }
            Debug.Log($"Player {currentPlayerIndex} has no legal moves with roll {lastDice1Roll}, {lastDice2Roll}. Skipping turn.");
            NextPlayer();
            return;
        }

        if (totalMoves == 1)
        {
            AutoRunTheOnlyAvailableMove();
            return;
        }

        gamePhase = GamePhase.WaitingForMove;
        Debug.Log($"Player {currentPlayerIndex}, pick a Piece with available moves!");
    }

    public void OnPieceClicked(Piece piece)
    {
        if (gamePhase != GamePhase.WaitingForMove)
        {
            Debug.Log("We are not waiting for move yet!");
            return;
        }
        if (piece.ownerPlayerIndex != currentPlayerIndex)
        {
            Debug.Log($"That's not your piece: {piece}");
            return;
        }

        if (!currentLegalMoves.TryGetValue(piece, out var options) || options.Count == 0)
        {
            Debug.Log($"{piece} has NO legal moves.");
            return;
        }

        MoveOption chosenOption = ChooseMoveOption(options);

        MoveResult moveResult = boardRules.TryResolveMove(piece, chosenOption.steps, pieces);
        ExecuteResolvedMove(chosenOption, moveResult);
    }

    public MoveOption ChooseMoveOption(List<MoveOption> options)
    {
        if (options.Count == 1)
        {
            return options[0];
        }
        else
        {
            // TODO show options to the user so they can tap on one.
            return options.First();
        }
    }

    private void ExecuteResolvedMove(MoveOption moveOption, MoveResult moveResult)
    {
        if (moveOption.usesDice1) dice1Used = true;
        if (moveOption.usesDice2) dice2Used = true;

        if (moveOption.bonusIndex >= 0)
        {
            bonusStepsToMove.RemoveAt(moveOption.bonusIndex);
        }

        OnMoveStarted?.Invoke();

        List<int> path = boardRules.GetPathIndices(moveOption.piece, moveOption.steps);

        OnMoveAnimationRequested?.Invoke(
            moveOption.piece,
            path,
            () => FinalizeMoveAfterAnimation(moveOption, moveResult)
        );
    }

    private void FinalizeMoveAfterAnimation(MoveOption moveOption, MoveResult moveResult)
    {
        Debug.Log($"Moving {moveOption.piece} from Tiles {moveOption.piece.currentTileIndex} to {moveOption.targetTileIndex}");
        moveOption.piece.MoveToTile(moveOption.targetTileIndex, boardView);

        if (moveResult.status == MoveStatus.Capture)
        {
            Debug.Log($"Capturing {moveResult.capturedPiece}");
            OnMovePieceToStart?.Invoke(moveResult.capturedPiece);
            bonusStepsToMove.Add(BonusForCapture);
        }

        boardView.LayoutPieces(pieces);

        OnMoveEnded?.Invoke();

        HandleEndOfMove(moveOption, moveResult);
    }

    private void HandleEndOfMove(MoveOption moveOption, MoveResult moveResult)
    {
        if (moveResult.status == MoveStatus.ReachedHome)
        {
            Debug.Log($"{moveOption.piece} reached Home!");
            bonusStepsToMove.Add(BonusForReachingHome);

            if (HasCurrentPlayerFinishedTheGame())
            {
                OnPlayerFinishedTheGame?.Invoke(currentPlayerIndex);
                //NextPlayer();
                EndGame();
                return;
            }
        }

        if (dice1Used && dice2Used && bonusStepsToMove.Count == 0)
        {
            if (lastDice1Roll == lastDice2Roll)
            {
                gamePhase = GamePhase.WaitingForRoll;
                Debug.Log($"Player {currentPlayerIndex} can roll again!");
                return;
            }
            NextPlayer();
            return;
        }

        int totalMoves = CalculateLegalMovesForCurrentPlayer();

        if (totalMoves == 0)
        {
            bonusStepsToMove.Clear(); // Loses bonus if you cant move with the previous roll

            if (lastDice1Roll == lastDice2Roll)
            {
                gamePhase = GamePhase.WaitingForRoll;
                Debug.Log($"Player {currentPlayerIndex} can roll again!");
                return;
            }
            NextPlayer();
            return;
        }

        if (totalMoves == 1)
        {
            AutoRunTheOnlyAvailableMove();
            return;
        }

        gamePhase = GamePhase.WaitingForMove;
        Debug.Log($"Player {currentPlayerIndex}, pick a Piece with available moves!");
    }

    private void AutoRunTheOnlyAvailableMove()
    {
        Debug.Log("Auto moving piece");
        var onlyMove = currentLegalMoves.First().Value.First();
        var onlyMoveResult = boardRules.TryResolveMove(onlyMove.piece, onlyMove.steps, pieces);
        ExecuteResolvedMove(onlyMove, onlyMoveResult);
    }

    private bool PenaltyForRollingDoubles()
    {
        Debug.Log($"Handling rolling doubles. Count={rolledDoublesInARow}");
        if (rolledDoublesInARow >= 3)
        {
            Debug.Log($"Oh oh, Player {currentPlayerIndex} have rolled 3 doubles in a row. There will be a penalty");

            Piece mostAdvancePiece = null;
            int maxProgressScore = 0;
            foreach (var piece in pieces)
            {
                if (piece.ownerPlayerIndex != currentPlayerIndex
                || piece.currentTileIndex == -1
                || piece.currentTileIndex == boardRules.GetHomeTile(currentPlayerIndex))
                {
                    continue;
                }

                var currentProgressScore = boardRules.GetProgressScore(piece.currentTileIndex, piece.ownerPlayerIndex);

                if (maxProgressScore < currentProgressScore)
                {
                    maxProgressScore = currentProgressScore;
                    mostAdvancePiece = piece;
                }
            }
            if (mostAdvancePiece != null)
            {
                Debug.Log($"Sent back home: {mostAdvancePiece}");
                OnMovePieceToStart?.Invoke(mostAdvancePiece);
            }
            return true;
        }
        return false;
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
                MoveResult moveResult = boardRules.TryResolveMove(piece, lastDice1Roll, pieces);
                if (moveResult.targetTileIndex != -1)
                {
                    options.Add(new MoveOption(piece, moveResult.targetTileIndex, lastDice1Roll, true, false));
                    Debug.Log($"Available Move: {options.Last()}");
                }
            }

            if (lastDice2Roll > 0 && !dice2Used)
            {
                MoveResult moveResult = boardRules.TryResolveMove(piece, lastDice2Roll, pieces);
                if (moveResult.targetTileIndex != -1)
                {
                    options.Add(new MoveOption(piece, moveResult.targetTileIndex, lastDice2Roll, false, true));
                    Debug.Log($"Available Move: {options.Last()}");
                }
            }

            if (lastDice1Roll > 0 && lastDice2Roll > 0 && !dice1Used && !dice2Used)
            {
                MoveResult moveResult = boardRules.TryResolveMove(piece, lastDice1Roll + lastDice2Roll, pieces);
                if (moveResult.targetTileIndex != -1)
                {
                    options.Add(new MoveOption(piece, moveResult.targetTileIndex, lastDice1Roll + lastDice2Roll, true, true));
                    Debug.Log($"Available Move: {options.Last()}");
                }
            }

            if (bonusStepsToMove.Count > 0)
            {
                for (int i = 0; i < bonusStepsToMove.Count; i++)
                {
                    MoveResult moveResult = boardRules.TryResolveMove(piece, bonusStepsToMove[i], pieces);
                    if (moveResult.targetTileIndex != -1)
                    {
                        options.Add(new MoveOption(piece, moveResult.targetTileIndex, bonusStepsToMove[i], i));
                        Debug.Log($"Available Move: {options.Last()}");
                    }
                }
            }

            if (options.Count > 0)
            {
                currentLegalMoves[piece] = options;
            }
        }

        if (lastDice1Roll == lastDice2Roll)
        {
            EnforceBreakOwnBlockadeRule();
        }

        foreach (var piece in pieces)
        {
            if (currentLegalMoves.Keys.Contains(piece))
            {
                totalMoves += currentLegalMoves[piece].Count;
            }
        }

        OnAvailableMovesUpdated?.Invoke();

        return totalMoves;
    }

    private void EnforceBreakOwnBlockadeRule()
    {
        var blockadePieces = new HashSet<Piece>();

        var blockadeGroups = pieces
        .Where(
            p => p.ownerPlayerIndex == currentPlayerIndex &&
            p.currentTileIndex >= 0 &&
            p.currentTileIndex != boardRules.GetHomeTile(currentPlayerIndex)
        )
        .GroupBy(p => p.currentTileIndex);

        foreach (var group in blockadeGroups)
        {
            if (group.Count() >= 2)
            {
                foreach (var piece in group)
                {
                    Debug.Log($"Blockage piece: {piece}");
                    blockadePieces.Add(piece);
                }
            }
        }

        if (blockadePieces.Count == 0)
        {
            return; // no blockade
        }

        playerHadABlockade = true;

        foreach (var piece in pieces)
        {
            if (!blockadePieces.Contains(piece) && currentLegalMoves.Keys.Contains(piece))
            {
                Debug.Log($"Piece cannot move because player has a blockade: {piece}");
                currentLegalMoves.Remove(piece);
            }
        }
    }

    private bool HasCurrentPlayerFinishedTheGame()
    {
        return pieces.Find(p => p.ownerPlayerIndex == currentPlayerIndex && boardRules.GetHomeTile(currentPlayerIndex) != p.currentTileIndex) == null;
    }

    private bool ThereIsOnlyOnePlayerLeft()
    {
        var playersStillPlayingGroups = pieces.Where(p => p.currentTileIndex != boardRules.GetHomeTile(p.ownerPlayerIndex)).GroupBy(p => p.ownerPlayerIndex);
        var playersWithActivePieces = playersStillPlayingGroups.Count();
        return playersWithActivePieces <= 1;
    }

    private void NextPlayer()
    {
        rolledDoublesInARow = 0;
        bonusStepsToMove.Clear();
        currentLegalMoves.Clear();
        currentPlayerIndex = (currentPlayerIndex + 1) % BoardDefinition.PLAYERS;

        if (ThereIsOnlyOnePlayerLeft())
        {
            EndGame();
            return;
        }

        if (HasCurrentPlayerFinishedTheGame())
        {
            // Skip already finished Player
            NextPlayer();
            return;
        }

        Debug.Log($"[SM] Next Player {currentPlayerIndex}");
        StartTurn();
    }

    public void ResetGame()
    {
        gamePhase = GamePhase.WaitingForRoll;
        currentPlayerIndex = 0;
    }
}
