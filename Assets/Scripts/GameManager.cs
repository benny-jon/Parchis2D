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

    [SerializeField] public List<Piece> allPieces;

    [SerializeField] public GameSettings gameSettings;

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
        
        List<Piece> activePieces = GetActivePlayersPieces();
        allPieces.ForEach(p => p.gameObject.SetActive(activePieces.Contains(p))); // disable innactive pieces
        stateMachine = new GameStateMachine(activePieces, boardView, boardRules);

        SubscribeToStateMachineEvents();

        ClearOtherPlayersDiceHints();
    }

    private List<Piece> GetActivePlayersPieces()
    {
        if (gameSettings == null)
        {
            // default to all 4 players
            return allPieces;
        }

        if (gameSettings.playerCount == 2)
        {
            return GetPiecesForPlayers(new int[] {0, 2});
        }
        if (gameSettings.playerCount == 3)
        {
            return GetPiecesForPlayers(new int[] {0, 1, 2});
        }

        return allPieces;
    }

    private List<Piece> GetPiecesForPlayers(int[] players)
    {
        List<Piece> result = new List<Piece>();
        for (int i = 0; i < allPieces.Count; i++)
        {
            if (players.Contains(allPieces[i].ownerPlayerIndex))
            {
                result.Add(allPieces[i]);
            }
        }
        return result;
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
            parchisUI.ShowNotification("10 bonus moves when a piece reaches Home");
        }
        if (moveResult.status == MoveStatus.Capture)
        {
            soundManager?.PlayCapture();
            parchisUI.ShowNotification("20 bonus moves for capturing a piece");
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

            if (parchisUI != null)
            {
                Debug.Log("Show Game Over message");
                parchisUI.ShowGameOver(stateMachine.playersFinishRanking[0]);
            }
        }
    }

    private void HandleMovePieceToStart(Piece piece)
    {
        if (piece.ownerPlayerIndex == stateMachine.currentPlayerIndex)
        {
            soundManager?.PlayPieceToStart();
            parchisUI.ShowNotification("Penalty for rolling 3 consecutives doubles");
        }

        piece.MoveToTile(-1);

        animationManager.PlayResetPiece(piece, boardView.pieceSpawnPoints[allPieces.FindIndex(p => p == piece)].position, () =>
        {
            ResetPiece(piece);
        });
    }

    private void HandlePlayerFinished(int player, Medal medal)
    {
        Debug.Log($"Player {player} has finished the game");
        //SetPlayerActionHint(player, "Winner!");

        ClearMoveHints();
        ClearOtherPlayersDiceHints();

        if (parchisUI != null)
        {
            parchisUI.ShowMedal(player, medal);
        }

        soundManager?.PlayPlayerWin();
    }

    private void HandleMoveAnimationRequested(Piece piece, List<int> path, System.Action onComplete)
    {
        Debug.Log($"Animating piece: {piece}");
        ClearOthersMoveHints(piece);
        animationManager.PlayMove(piece, path, onComplete);
    }

    public void HandleSelectMoveOptionRequest(int requestId, Piece piece, IReadOnlyList<MoveOption> options)
    {
        currenOptionRequestId = requestId;

        movePopupUI?.Show(activePieceTransform, options.ToArray(), onPickIndex: (optionIndex) =>
        {
            Debug.Log($"Calling OnMoveOptionSelected for {optionIndex}");
            activePiece.SetMoveHints(new List<MoveOption>() { options[optionIndex] });
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

    private void ClearOthersMoveHints(Piece activePiece)
    {
        foreach (Piece piece in allPieces)
        {
            if (piece != null && piece != activePiece) piece.ClearMoveHints();
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

    public GamePhase CurrentGamePhase()
    {
        return stateMachine != null ? stateMachine.gamePhase : GamePhase.GameOver;
    }
}
