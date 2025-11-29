using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public BoardDefinition boardDefinition;
    public BoardView boardView;

    public List<Piece> allPieces;

    private BoardRules boardRules;
    private GameStateMachine stateMachine;

    private void Awake() {
        
    }

    void Start()
    {
        ResetPieces();
        
        boardRules = new BoardRules(boardDefinition);
        stateMachine = new GameStateMachine(allPieces, boardView, boardRules);

        stateMachine.OnDiceRolled += roll =>
        {
            Debug.Log($"Dice rolled: {roll}");
        };
        stateMachine.OnTurnChanged += player =>
        {
            Debug.Log($"Turn changed to Player {player}");
        };
        stateMachine.OnMoveStarted += () =>
        {
            Debug.Log($"On Move Started");
        };
        stateMachine.OnMoveEnded += () =>
        {
            Debug.Log($"On Move Ended");
        };

        stateMachine.StartGame();

        Debug.Log("StateMachine Initialized " + GetHashCode());
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
