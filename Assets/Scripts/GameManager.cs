using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] public BoardDefinition boardDefinition;
    [SerializeField] public BoardView boardView;
    [SerializeField] public AnimationManager animationManager;
    [SerializeField] public SoundManager soundManager;

    [SerializeField] public ParchisUI parchisUI;
    [SerializeField] public MovePopupUI movePopupUI;

    public List<Piece> allPieces;

    private BoardRules boardRules;
    private GameStateMachine stateMachine;

    #region Option Selection Variables
    private int currenOptionRequestId;
    private Piece activePiece;
    private Transform activePieceTransform;
    #endregion

    private void Awake()
    {
        ResetPieces();
        ClearPlayersActionHints();

        boardRules = new BoardRules(boardDefinition);
        stateMachine = new GameStateMachine(allPieces, boardView, boardRules);

        SubscribeToStateMachineEvents();

        ClearOtherPlayersDiceHints();
    }

    void Start()
    {
        stateMachine.StartGame();
        Debug.Log("GameManager Initialized " + GetHashCode());
    }

    private void OnDestroy()
    {
        UbsubscribeFromStateMachineEvents();
    }

    // ---------------------------
    //  SUBSCRIBE / UNSUBSCRIBE
    // ---------------------------

    private void SubscribeToStateMachineEvents()
    {
        if (stateMachine == null) return;

        stateMachine.OnDiceRolled += HandleDiceRolled;
        stateMachine.OnTurnChanged += HandleTurnChanged;
        stateMachine.OnMoveStarted += HandleMoveStarted;
        stateMachine.OnMoveEnded += HandleMoveEnded;
        stateMachine.OnAvailableMovesUpdated += HandleAvailableMovesUpdated;
        stateMachine.OnGamePhaseChanged += HandleGamePhaseChanged;
        stateMachine.OnMovePieceToStart += HandleMovePieceToStart;
        stateMachine.OnPlayerFinishedTheGame += HandlePlayerFinished;
        stateMachine.OnMoveAnimationRequested += HandleMoveAnimationRequested;
        stateMachine.OnMoveOptionSelectionRequest += HandleSelectMoveOptionRequest;
    }

    private void UbsubscribeFromStateMachineEvents()
    {
        if (stateMachine == null) return;

        stateMachine.OnDiceRolled -= HandleDiceRolled;
        stateMachine.OnTurnChanged -= HandleTurnChanged;
        stateMachine.OnMoveStarted -= HandleMoveStarted;
        stateMachine.OnMoveEnded -= HandleMoveEnded;
        stateMachine.OnAvailableMovesUpdated -= HandleAvailableMovesUpdated;
        stateMachine.OnGamePhaseChanged -= HandleGamePhaseChanged;
        stateMachine.OnMovePieceToStart -= HandleMovePieceToStart;
        stateMachine.OnPlayerFinishedTheGame -= HandlePlayerFinished;
        stateMachine.OnMoveAnimationRequested -= HandleMoveAnimationRequested;
        stateMachine.OnMoveOptionSelectionRequest -= HandleSelectMoveOptionRequest;
    }

    // ---------------------------
    //  EVENT HANDLERS
    // ---------------------------

    private void HandleDiceRolled(int dice1, int dice2)
    {
        soundManager?.PlayDiceRoll();
        Debug.Log($"Dice rolled: {dice1}, {dice2}");
        ClearPlayersActionHints();
        SetCurrentPlayerDiceHint(dice1, dice2);
    }

    private void HandleTurnChanged(int player)
    {
        Debug.Log($"Turn changed to Player {player}");
        ClearMoveHints();
    }

    private void HandleMoveStarted()
    {
        Debug.Log("On Move Started");
    }

    private void HandleMoveEnded(MoveResult moveResult)
    {
        Debug.Log("On Move Ended");
        soundManager?.PlayPieceMoveEnd();
        if (moveResult.status == MoveStatus.ReachedHome)
        {
            soundManager?.PlayHome();
        }
    }

    private void HandleAvailableMovesUpdated()
    {
        ClearMoveHints();
        UpdateMoveHints();
    }

    private void HandleGamePhaseChanged(GamePhase phase)
    {
        if (phase == GamePhase.WaitingForRoll)
        {
            ClearPlayersActionHints();
            ClearMoveHints();
            SetPlayerActionHint(stateMachine.currentPlayerIndex, phase);
        }
        else if (phase == GamePhase.WaitingForMove)
        {
            ClearOtherPlayersDiceHints();
            SetPlayerActionHint(stateMachine.currentPlayerIndex, phase);
        }
        else if (phase == GamePhase.GameOver)
        {
            Debug.LogWarning("GAME OVER!");
            soundManager?.PlayPlayerWin();
        }
    }

    private void HandleMovePieceToStart(Piece piece)
    {
        if (piece.ownerPlayerIndex != stateMachine.currentPlayerIndex)
        {
            // it was a capture
            soundManager?.PlayCapture();
        }
        else
        {
            soundManager?.PlayPieceToStart();
        }
        
        piece.MoveToTile(-1);

        animationManager.PlayResetPiece(piece, boardView.pieceSpawnPoints[allPieces.FindIndex(p => p == piece)].position, () =>
        {
            ResetPiece(piece);
        });
    }

    private void HandlePlayerFinished(int player)
    {
        Debug.Log($"Player {player} has finished the game");
        //SetPlayerActionHint(player, "Winner!");

        if (parchisUI != null)
        {
            parchisUI.ShowGameOver(player);
        }

        soundManager?.PlayPlayerWin();
    }

    private void HandleMoveAnimationRequested(Piece piece, List<int> path, System.Action onComplete)
    {
        Debug.Log($"Animating piece: {piece}");
        animationManager.PlayMove(piece, path, onComplete);
    }

    public void HandleSelectMoveOptionRequest(int requestId, Piece piece, IReadOnlyList<MoveOption> options)
    {
        currenOptionRequestId = requestId;

        movePopupUI?.Show(activePieceTransform, options.ToArray(), onPickIndex: (optionIndex) =>
        {
            Debug.Log($"Calling OnMoveOptionSelected for {optionIndex}");
            stateMachine.OnMoveOptionSelected(currenOptionRequestId, activePiece, optionIndex);
        });
    }

    // ---------------------------
    //  EXISTING HELPERS
    // ---------------------------

    private void ClearPlayersActionHints()
    {
        if (parchisUI != null)
        {
            parchisUI.ClearTurnHints();
        }
    }

    private void ClearOtherPlayersDiceHints()
    {
        if (parchisUI != null)
        {
            parchisUI.ClearOtherPlayersDice(stateMachine.currentPlayerIndex);
        }
    }

    private void SetPlayerActionHint(int player, GamePhase phase)
    {
        if (parchisUI != null)
        {
            parchisUI.ShowTurnHintForPlayer(player, phase);
        }
    }

    private void SetCurrentPlayerDiceHint(int dice1, int dice2)
    {
        if (parchisUI != null)
        {
            parchisUI.ShowDiceForPlayer(stateMachine.currentPlayerIndex, dice1, dice2);
        }
    }

    private void UpdateMoveHints()
    {
        var moves = stateMachine.CurrentLegalMoves;
        if (moves == null) return;
        //if (moves.Count <= 1) return; // we auto-move the piece

        foreach (var kvp in moves)
        {
            var piece = kvp.Key;
            var options = kvp.Value;
            if (piece != null)
            {
                piece.SetMoveHints(options);
            }
        }
    }

    private void ClearMoveHints()
    {
        foreach (Piece piece in allPieces)
        {
            if (piece != null) piece.ClearMoveHints();
        }
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
            allPieces[i].ClearMoveHints();
        }
    }

    void ResetPiece(Piece piece)
    {
        for (int i = 0; i < allPieces.Count; i++)
        {
            if (allPieces[i] == piece)
            {
                piece.MoveToStart(boardView.pieceSpawnPoints[i].position);
                piece.ClearMoveHints();
                break;
            }
        }
    }

    public void OnPieceClicked(Piece piece, Transform pieceTransform)
    {
        if (stateMachine == null)
        {
            Debug.LogError("StateMachine is NULL " + GetHashCode());
            return;
        }

        activePiece = piece;
        activePieceTransform = pieceTransform;

        if (stateMachine.OnPieceClicked(piece))
        {
            soundManager?.PlayPieceClicked();
        }
    }

    public void OnDiceRollButton()
    {
        if (stateMachine == null)
        {
            Debug.LogError("StateMachine is NULL");
            return;
        }

        stateMachine.RollDice();
    }
}
