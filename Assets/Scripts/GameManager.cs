using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardDefinition boardDefinition;
    public BoardView boardView;

    public List<Piece> allPieces;
    public TMP_Text[] actionHintPerPlayer;
    public TMP_Text[] diceHintPerPlayer;
    public TMP_Text gameOverText;

    private BoardRules boardRules;
    private GameStateMachine stateMachine;

    void Start()
    {
        if (gameOverText != null)
        {
            gameOverText.enabled = false;
        }
        ResetPieces();
        ClearPlayersActionHints();

        boardRules = new BoardRules(boardDefinition);
        stateMachine = new GameStateMachine(allPieces, boardView, boardRules);

        stateMachine.OnDiceRolled += (dice1, dice2) =>
        {
            Debug.Log($"Dice rolled: {dice1}, {dice2}");
            ClearPlayersActionHints();
            SetCurrentPlayerDiceHint($"{dice1},{dice2}");
        };
        stateMachine.OnTurnChanged += player =>
        {
            Debug.Log($"Turn changed to Player {player}");
            ClearMoveHints();
        };
        stateMachine.OnMoveStarted += () =>
        {
            Debug.Log($"On Move Started");
        };
        stateMachine.OnMoveEnded += () =>
        {
            Debug.Log($"On Move Ended");
        };
        stateMachine.OnAvailableMovesUpdated += () =>
        {
            ClearMoveHints();
            UpdateMoveHints();
        };
        stateMachine.OnGamePhaseChanged += (phase) =>
        {
            if (phase == GamePhase.WaitingForRoll)
            {
                ClearPlayersActionHints();
                ClearMoveHints();
                SetPlayerActionHint(stateMachine.currentPlayerIndex, "Roll");
            }
            if (phase == GamePhase.WaitingForMove)
            {
                ClearOtherPlayersDiceHints();
                SetPlayerActionHint(stateMachine.currentPlayerIndex, "Move");
            }
        };
        stateMachine.OnMovePieceToStart += (piece) =>
        {
            ResetPiece(piece);
        };
        stateMachine.OnPlayerFinishedTheGame += (player) =>
        {
            Debug.Log($"Player {player} has finished the game");  
            SetPlayerActionHint(player, "Winner!");
            if (gameOverText != null)
            {
                gameOverText.text = $"Player {player}\nhas Won!";
                gameOverText.enabled = true;
            }
        };
        stateMachine.OnGameOver += () =>
        {
            Debug.LogWarning("GAME OVER!");  
        };

        ClearOtherPlayersDiceHints();
        stateMachine.StartGame();

        Debug.Log("StateMachine Initialized " + GetHashCode());
    }

    private void ClearPlayersActionHints()
    {
        if (actionHintPerPlayer != null)
        {
            for (int i = 0; i < actionHintPerPlayer.Length; i++)
            {
                actionHintPerPlayer[i].text = "";
            }
        }
    }

    private void ClearOtherPlayersDiceHints()
    {
        if (diceHintPerPlayer != null)
        {
            for (int i = 0; i < diceHintPerPlayer.Length; i++)
            {
                if (i != stateMachine.currentPlayerIndex)
                {
                    diceHintPerPlayer[i].text = "";
                }
            }
        }
    }

    private void SetPlayerActionHint(int player, String hintMessage)
    {
        if (actionHintPerPlayer != null && actionHintPerPlayer.Length >= player + 1)
        {
            actionHintPerPlayer[player].text = hintMessage;
        }
        else
        {
            Debug.LogError("Forgot to assigned values to the actionHintPerPlayer list");
        }
    }

    private void SetCurrentPlayerDiceHint(String hintMessage)
    {
        if (diceHintPerPlayer != null && diceHintPerPlayer.Length >= stateMachine.currentPlayerIndex + 1)
        {
            diceHintPerPlayer[stateMachine.currentPlayerIndex].text = hintMessage;
        }
        else
        {
            Debug.LogError("Forgot to assigned values to the diceHintPerPlayer list");
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

    public void OnPieceClicked(Piece piece)
    {
        if (stateMachine == null)
        {
            Debug.LogError("StateMachine is NULL " + GetHashCode());
            return;
        }

        stateMachine.OnPieceClicked(piece);
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
