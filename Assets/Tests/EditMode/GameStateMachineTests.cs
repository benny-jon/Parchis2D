using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;

public class GameStateMachineTests
{
    private BoardDefinition boardDefinition;
    private BoardRules boardRules;
    private BoardView boardView;
    private List<Piece> pieces;
    
    private GameStateMachine stateMachine;

    [SetUp]
    public void SetUp()
    {
        boardDefinition = new BoardDefinition();
        BoardDefinitionGenerator.GenerateFullBoard(boardDefinition);
        boardRules = new BoardRules(boardDefinition);
        boardView = CreateTestBoardView(BoardDefinition.TOTAL_TILES);
        pieces = new List<Piece>
        {
            CreateTestPiece(owner: 0, tileIndex: -1),
            CreateTestPiece(owner: 0, tileIndex: 10),
        };

        stateMachine = new GameStateMachine(pieces, boardView, boardRules);

        stateMachine.OnMovePieceToStart += (piece) =>
        {
           piece.currentTileIndex = -1;  
        };
    }

    [Test]
    public void RollFive_AllowBasePieceToLeave_ToStartTile()
    {
        stateMachine.StartGame();

        stateMachine.RollDiceWithValues(5, 2);

        var basePiece = pieces[0];

        Assert.AreEqual(GamePhase.WaitingForMove, stateMachine.gamePhase);

        stateMachine.OnPieceClicked(basePiece);

        Assert.AreEqual(boardDefinition.GetStartTilesIndex()[0], basePiece.currentTileIndex);
    }

    [Test]
    public void TwoConsecutiveDoubles_ShouldBeFine()
    {
        stateMachine.StartGame();
        Assert.AreEqual(-1, pieces[0].currentTileIndex);

        var trackPiece = pieces[1];
        trackPiece.currentTileIndex = 20;

        stateMachine.RollDiceWithValues(4, 4);
        stateMachine.OnPieceClicked(trackPiece);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(3, 3);
        stateMachine.OnPieceClicked(trackPiece);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(2, 3);
    
        Assert.AreNotEqual(-1, trackPiece.currentTileIndex);
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
    }

    [Test]
    public void ThreeConsecutiveDoubles_SendsFurthestPieceBackToBase()
    {
        stateMachine.StartGame();
        Assert.AreEqual(-1, pieces[0].currentTileIndex);

        var trackPiece = pieces[1];
        trackPiece.currentTileIndex = 20;

        stateMachine.RollDiceWithValues(4, 4);
        stateMachine.OnPieceClicked(trackPiece);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(3, 3);
        stateMachine.OnPieceClicked(trackPiece);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(2, 2);
    
        Assert.AreEqual(-1, trackPiece.currentTileIndex);
        Assert.AreEqual(1, stateMachine.currentPlayerIndex);
    }

    [Test]
    public void MoveToNextPlayer_WhenMovesAreAvailable()
    {
        pieces.ForEach(p => p.currentTileIndex = -1); // move all to base
        stateMachine.StartGame();
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);

        stateMachine.RollDiceWithValues(1, 3);

        Assert.AreEqual(1, stateMachine.currentPlayerIndex);
    }

    [Test]
    public void Verify_CaptureEnemyPiece()
    {
        int enemyPieceTileIndex = 8;
        Assert.False(boardRules.IsTileSafe(enemyPieceTileIndex));

        pieces.Insert(2, CreateTestPiece(1, enemyPieceTileIndex));
        pieces[1].currentTileIndex = enemyPieceTileIndex - 1;

        stateMachine.RollDiceWithValues(1, 0);
        stateMachine.OnPieceClicked(pieces[1]);

        Assert.AreEqual(-1, pieces[2].currentTileIndex);
    }

    [Test]
    public void Verify_CannotCaptureEnemyPiece_OnSafeTile()
    {
        int enemyPieceTileIndex = boardDefinition.tiles.First(t => t.type == TileType.Safe).index;
        Assert.True(boardRules.IsTileSafe(enemyPieceTileIndex));

        pieces.Insert(2, CreateTestPiece(1, enemyPieceTileIndex));
        pieces[1].currentTileIndex = enemyPieceTileIndex - 1;

        stateMachine.RollDiceWithValues(1, 0);
        stateMachine.OnPieceClicked(pieces[1]);

        Assert.AreEqual(enemyPieceTileIndex, pieces[2].currentTileIndex);
    }

    private BoardView CreateTestBoardView(int tileCount)
    {
        var go = new GameObject("BoardView");
        var view = go.AddComponent<BoardView>();
        view.boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
        BoardDefinitionGenerator.GenerateFullBoard(view.boardDefinition);

        view.tilePoints = new Transform[tileCount];

        for (int i = 0; i < tileCount; i++)
        {
            var tileGo = new GameObject($"Tile_{i}");
            tileGo.transform.parent = go.transform;
            tileGo.transform.position = new Vector3(i, 0f, 0f);
            view.tilePoints[i] = tileGo.transform;
        }

        return view;
    }

    private Piece CreateTestPiece(int owner, int tileIndex)
    {
        var go = new GameObject($"Piece_P{owner}_T{tileIndex}");
        var piece = go.AddComponent<Piece>();
        piece.ownerPlayerIndex = owner;
        piece.currentTileIndex = tileIndex;
        return piece;
    }
}
