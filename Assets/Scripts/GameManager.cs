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
    #endregion

    private int pendingBonusToAnnounce = 0;
    private int pendingBonusPlayer = -1;

    private bool pendingShowForceStartTooltip = true;

    public GameStateMachine GetStateMachine()
    {
        return stateMachine;
    }

    private void Awake()
    {
        ResetPieces();
        ClearPlayersActionHints();

        boardRules = new BoardRules(boardDefinition);

        List<Piece> activePieces = GetActivePlayersPieces();
        allPieces.ForEach(p => p.gameObject.SetActive(activePieces.Contains(p))); // disable innactive pieces
        stateMachine = new GameStateMachine(activePieces, boardView, boardRules);

        SubscribeToStateMachineEvents();
        SubscribeToParchisUiEvents();

        ClearOtherPlayersDiceHints();
    }

    public List<Piece> GetActivePlayersPieces()
    {
        if (gameSettings == null)
        {
            // default to all 4 players
            return allPieces;
        }

        if (gameSettings.playerCount == 2)
        {
            return GetPiecesForPlayers(new int[] { 0, 2 });
        }
        if (gameSettings.playerCount == 3)
        {
            return GetPiecesForPlayers(new int[] { 0, 1, 2 });
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
        UnsubscribeFromStateMachineEvents();
        UnsubscribeFromParchisUiEvents();
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
        stateMachine.OnPlayerEnforcedToSpecialCase += HandlePlayerForcedToSpecialCase;
    }

    private void UnsubscribeFromStateMachineEvents()
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
        stateMachine.OnPlayerEnforcedToSpecialCase -= HandlePlayerForcedToSpecialCase;
    }

    private void SubscribeToParchisUiEvents()
    {
        Debug.Log("Game Manager, SubscribeToParchisUiEvents");
        parchisUI.OnPlayerDiceClicked += OnUiDicePanelClicked;
    }

    private void UnsubscribeFromParchisUiEvents()
    {
        parchisUI.OnPlayerDiceClicked -= OnUiDicePanelClicked;
    }

    // ---------------------------
    //  EVENT HANDLERS
    // ---------------------------

    private void OnUiDicePanelClicked(int player)
    {
        Debug.Log($"GameManager: OnUiDicePanelClicked {player}");
        if (stateMachine == null)
        {
            Debug.LogError("StateMachine is NULL");
            return;
        }
        if (player != stateMachine.currentPlayerIndex)
        {
            return;
        }

        stateMachine.RollDice();
    }

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
        Debug.Log("On Move Ended. With status: " + moveResult.status);
        soundManager?.PlayPieceMoveEnd();
        if (moveResult.status == MoveStatus.ReachedHome)
        {
            soundManager?.PlayHome();
            pendingBonusToAnnounce = GameStateMachine.BonusForReachingHome;
            pendingBonusPlayer = stateMachine.currentPlayerIndex;
        }
        if (moveResult.status == MoveStatus.Capture)
        {
            soundManager?.PlayCapture();
            pendingBonusToAnnounce = GameStateMachine.BonusForCapture;
            pendingBonusPlayer = stateMachine.currentPlayerIndex;
        }
    }

    private void HandleAvailableMovesUpdated(int movesCount)
    {
        AnnounceBonusMessageIfAvailable();
        ClearMoveHints();
        UpdateMoveHints();
    }

    private void AnnounceBonusMessageIfAvailable()
    {
        if (pendingBonusToAnnounce > 0 && pendingBonusPlayer == stateMachine.currentPlayerIndex)
        {
            if (stateMachine.HasCurrentLegalBonusMove())
            {
                var message = GetBonusMessage(pendingBonusToAnnounce);
                if (message != null)
                {
                    parchisUI?.ShowNotification(message);
                }
            }
            else
            {
                // lost the bonus for not having available legal moves
                var message = GetBonusLostMessage(pendingBonusToAnnounce);
                if (message != null)
                {
                    parchisUI?.ShowNotification(message);
                }
            }
        }
        pendingBonusToAnnounce = 0;
        pendingBonusPlayer = -1;
    }

    private string GetBonusMessage(int bonusSteps)
    {
        if (bonusSteps == GameStateMachine.BonusForCapture)
        {
            return $"{GameStateMachine.BonusForCapture} bonus moves for capturing a piece";
        }
        if (bonusSteps == GameStateMachine.BonusForReachingHome)
        {
            return $"{GameStateMachine.BonusForReachingHome} bonus moves for reaching Home";
        }
        return null;
    }

    private string GetBonusLostMessage(int bonusSteps)
    {
        if (bonusSteps == GameStateMachine.BonusForCapture)
        {
            return $"Sorry, cannot use the {GameStateMachine.BonusForCapture} bonus moves for capturing a piece :(";
        }
        if (bonusSteps == GameStateMachine.BonusForReachingHome)
        {
            return $"Nice!, but cannot use the {GameStateMachine.BonusForReachingHome} bonus moves now";
        }
        return null;
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
            ClearPlayersActionHints();
            ClearMoveHints();
            Debug.LogWarning("GAME OVER!");
            soundManager?.PlayPlayerWin();

            if (parchisUI != null)
            {
                Debug.Log("Show Game Over message");
                parchisUI.ShowGameOver($"Game Over\n{GetPlayersName(stateMachine.playersFinishRanking[0])}\n Won first Place!");
            }
        }
    }

    private void HandleMovePieceToStart(Piece piece)
    {
        if (piece.ownerPlayerIndex == stateMachine.currentPlayerIndex)
        {
            soundManager?.PlayPieceToStart();
            parchisUI.ShowNotification($"Penalty {GetPlayersName(piece.ownerPlayerIndex)} for rolling 3 consecutives doubles");
        }

        animationManager.PlayResetPiece(piece, boardView.pieceSpawnPoints[allPieces.IndexOf(piece)].position, () =>
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

        Piece currentPiece = piece; // to be used from the lambda response
        movePopupUI?.Show(piece.transform, options.ToArray(), onPickIndex: (optionIndex) =>
        {
            Debug.Log($"Calling OnMoveOptionSelected for {optionIndex}");
            currentPiece.SetMoveHints(new List<MoveOption>() { options[optionIndex] });
            stateMachine.OnMoveOptionSelected(currenOptionRequestId, currentPiece, optionIndex);
        });
    }

    private void HandlePlayerForcedToSpecialCase(int player, MoveSpecialCaseType caseType)
    {
        switch (caseType)
        {
            case MoveSpecialCaseType.ForceToBreakBlockade:
                {
                    parchisUI?.ShowNotification($"{GetPlayersName(player)} rolled double and is forced to break the blockade");
                    break;
                }
            case MoveSpecialCaseType.ForceToStart:
                {
                    if (pendingShowForceStartTooltip)
                    {
                        parchisUI?.ShowNotification($"Rolling a 5 forces a token to Start");
                        pendingShowForceStartTooltip = false;
                    }
                    break;
                }
        }
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

    private void ClearOthersMoveHints(Piece pieceOfInterest)
    {
        foreach (Piece piece in allPieces)
        {
            if (piece != null && piece != pieceOfInterest) piece.ClearMoveHints();
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

        if (stateMachine.OnPieceClicked(piece))
        {
            soundManager?.PlayPieceClicked();
        }
    }

    public GamePhase CurrentGamePhase()
    {
        return stateMachine != null ? stateMachine.gamePhase : GamePhase.GameOver;
    }

    // TODO: move to game settings to let user choose color and position
    private string GetPlayersName(int player)
    {
        switch(player)
        {
            case 0: return "Yellow";
            case 1: return "Blue";
            case 2: return "Red";
            case 3: return "Green";
        }
        return "Player " + player;
    }

    #region DEBUG METHODS

    [ContextMenu("Re-Layout Pieces Positions")]
    private void LayoutPieces() 
    {
        if (boardView != null)
        {
            boardView.LayoutPieces(GetActivePlayersPieces());
        }
    }

    #endregion
}
