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
    public void RollFiveOnDice1_AllowBasePieceToLeave_ToStartTile()
    {
        stateMachine.StartGame();

        stateMachine.RollDiceWithValues(5, 2);

        var basePiece = pieces[0];

        Assert.AreEqual(GamePhase.WaitingForMove, stateMachine.gamePhase);

        stateMachine.OnPieceClicked(basePiece);

        Assert.AreEqual(boardDefinition.GetStartTilesIndex()[0], basePiece.currentTileIndex);
    }

    [Test]
    public void RollFiveOnDice2_AllowBasePieceToLeave_ToStartTile()
    {
        stateMachine.StartGame();
        pieces.Clear();
        pieces.Add(CreateTestPiece(owner: 0, tileIndex: -1));
        pieces.Add(CreateTestPiece(1, -1));
        pieces.Add(CreateTestPiece(1, -1));
        pieces.Add(CreateTestPiece(1, 52));
        pieces.Add(CreateTestPiece(1, -1));

        // Move turn to Player 1 by rollling not starting dice
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        stateMachine.RollDiceWithValues(1, 2);

        Assert.AreEqual(1, stateMachine.currentPlayerIndex);
        stateMachine.RollDiceWithValues(3, 5);

        Assert.AreEqual(GamePhase.WaitingForMove, stateMachine.gamePhase);
        var basePiece = pieces[1];
        stateMachine.OnPieceClicked(basePiece);
        Assert.AreEqual(boardDefinition.GetStartTilesIndex()[1], basePiece.currentTileIndex);
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
    public void Verify_FirstPlayerPenalty_ForMostAdvancePiece_WhenRollingThreeDoubles()
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
    public void Verify_NonFirstPlayerPenalty_ForMostAdvancePiece_WhenRollingThreeDoubles()
    {
        stateMachine.StartGame();
        pieces.Clear();

        var secondMostAdvance = CreateTestPiece(owner: 2, tileIndex: 60);
        var mostAdvance = CreateTestPiece(owner: 2, tileIndex: 3);

        pieces.Add(secondMostAdvance);
        pieces.Add(mostAdvance);

        stateMachine.RollDiceWithValues(1, 2); // moving turn to player 1
        stateMachine.RollDiceWithValues(1, 2); // moving turn to player 2
        Assert.AreEqual(2, stateMachine.currentPlayerIndex);

        stateMachine.RollDiceWithValues(1, 1);
        stateMachine.OnPieceClicked(secondMostAdvance);
        stateMachine.OnPieceClicked(secondMostAdvance);

        stateMachine.RollDiceWithValues(1, 1);
        stateMachine.OnPieceClicked(secondMostAdvance);
        stateMachine.OnPieceClicked(secondMostAdvance);

        stateMachine.RollDiceWithValues(1, 1);
        //Verify penalty
        Assert.AreEqual(-1, mostAdvance.currentTileIndex);
        Assert.AreEqual(64, secondMostAdvance.currentTileIndex);
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

    [Test]
    public void GetBonus_ForReachingHome()
    {
        pieces.Clear();
        var finishingPiece = CreateTestPiece(0, boardRules.GetHomeTile(0) - 1);
        var pieceA = CreateTestPiece(0, 4);
        var pieceB = CreateTestPiece(0, 5);
        pieces.Add(finishingPiece);
        pieces.Add(pieceA);
        pieces.Add(pieceB);

        stateMachine.StartGame();

        stateMachine.RollDiceWithValues(1, 0);
        stateMachine.OnPieceClicked(finishingPiece);

        var bonusMoveOption = stateMachine.CurrentLegalMoves[pieceA][0];
        Assert.AreEqual(GamePhase.WaitingForMove, stateMachine.gamePhase);
        Assert.AreEqual(0, bonusMoveOption.bonusIndex);
        Assert.AreEqual(false, bonusMoveOption.usesDice1);
        Assert.AreEqual(false, bonusMoveOption.usesDice2);
    }

    [Test]
    public void LoseBonusForReachingHome_IfNoMovesAvailable_EvenAfterRollingDoubles()
    {
        pieces.Clear();
        var finishingPiece = CreateTestPiece(0, boardRules.GetHomeTile(0) - 2);
        var pieceA = CreateTestPiece(0, -1);
        var otherPlayerPiece = CreateTestPiece(1, -1);
        pieces.Add(finishingPiece);
        pieces.Add(pieceA);
        pieces.Add(otherPlayerPiece);

        stateMachine.StartGame();

        stateMachine.RollDiceWithValues(1, 1);
        stateMachine.OnPieceClicked(finishingPiece);

        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);
        stateMachine.RollDiceWithValues(2, 3);

        Assert.AreEqual(1, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);
        Assert.AreEqual(boardRules.GetStartTile(0), pieceA.currentTileIndex);
        Assert.AreEqual(0, stateMachine.CurrentLegalMoves.Count);
    }

    [Test]
    public void Blockade_MustBeBroken_WhenRollingDoubles()
    {
        pieces.Clear();
        var pieceInBlockadeA = CreateTestPiece(0, 25);
        var pieceInBlockadeB = CreateTestPiece(0, 25);
        var freePiece = CreateTestPiece(0, 5);
        pieces.AddRange(new List<Piece> { pieceInBlockadeA, pieceInBlockadeB, freePiece });

        stateMachine.StartGame();
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(3, 3);

        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForMove, stateMachine.gamePhase);
        Assert.False(stateMachine.CurrentLegalMoves.Keys.Contains(freePiece));
        Assert.AreEqual(3, stateMachine.CurrentLegalMoves[pieceInBlockadeA].Count);
        Assert.AreEqual(3, stateMachine.CurrentLegalMoves[pieceInBlockadeB].Count);
    }

    [Test]
    public void Blockade_NonNeedToBeBroken_WhenNotRollingDoubles()
    {
        pieces.Clear();
        var pieceInBlockadeA = CreateTestPiece(0, 25);
        var pieceInBlockadeB = CreateTestPiece(0, 25);
        var freePiece = CreateTestPiece(0, 5);
        pieces.AddRange(new List<Piece> { pieceInBlockadeA, pieceInBlockadeB, freePiece });

        stateMachine.StartGame();
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(3, 4);

        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForMove, stateMachine.gamePhase);
        Assert.AreEqual(3, stateMachine.CurrentLegalMoves[freePiece].Count);
        Assert.AreEqual(3, stateMachine.CurrentLegalMoves[pieceInBlockadeA].Count);
        Assert.AreEqual(3, stateMachine.CurrentLegalMoves[pieceInBlockadeB].Count);
    }

    [Test]
    public void ForfeitTurn_IfBlockadeIsBlocked_WhenRollingDoubles()
    {
        pieces.Clear();
        var enemyInBlockadeA = CreateTestPiece(1, 26);
        var enemyInBlockadeB = CreateTestPiece(1, 26);
        // blockage blocked by enemy blockade
        var pieceInBlockadeA = CreateTestPiece(0, 25);
        var pieceInBlockadeB = CreateTestPiece(0, 25);
        var freePiece = CreateTestPiece(0, 5);
        pieces.AddRange(new List<Piece> { enemyInBlockadeA, enemyInBlockadeB, pieceInBlockadeA, pieceInBlockadeB, freePiece });
 
        stateMachine.StartGame();
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(3, 3);

        Assert.AreEqual(1, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);
    }

    [Test]
    public void DontForfeitTurn_IfBlockadeIsBlocked_AndNotRollingDoubles()
    {
        pieces.Clear();
        var enemyInBlockadeA = CreateTestPiece(1, 26);
        var enemyInBlockadeB = CreateTestPiece(1, 26);
        // blockage blocked by enemy blockade
        var pieceInBlockadeA = CreateTestPiece(0, 25);
        var pieceInBlockadeB = CreateTestPiece(0, 25);
        var freePiece = CreateTestPiece(0, 5);
        pieces.AddRange(new List<Piece> { enemyInBlockadeA, enemyInBlockadeB, pieceInBlockadeA, pieceInBlockadeB, freePiece });
 
        stateMachine.StartGame();
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        stateMachine.RollDiceWithValues(3, 4);

        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForMove, stateMachine.gamePhase);
    }

    [Test]
    public void AllowToReformSameBlockade_WhenRollingDoubles()
    {
        pieces.Clear();
        var pieceInBlockadeA = CreateTestPiece(0, 25);
        var pieceInBlockadeB = CreateTestPiece(0, 25);
        var freePiece = CreateTestPiece(0, 5);
        pieces.AddRange(new List<Piece> { pieceInBlockadeA, pieceInBlockadeB, freePiece });

        stateMachine.StartGame();
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);
        Assert.AreEqual(pieceInBlockadeA.currentTileIndex, pieceInBlockadeB.currentTileIndex); // blockade exist

        stateMachine.RollDiceWithValues(3, 3);
        Assert.False(stateMachine.CurrentLegalMoves.Keys.Contains(freePiece));
        Assert.AreEqual(3, stateMachine.CurrentLegalMoves[pieceInBlockadeA].Count);
        Assert.AreEqual(3, stateMachine.CurrentLegalMoves[pieceInBlockadeB].Count);

        stateMachine.OnPieceClicked(pieceInBlockadeA);
        Assert.AreEqual(1, stateMachine.CurrentLegalMoves[pieceInBlockadeB].Count);
        Assert.AreNotEqual(pieceInBlockadeA.currentTileIndex, pieceInBlockadeB.currentTileIndex); // broken blockade

        stateMachine.OnPieceClicked(pieceInBlockadeB);
        Assert.AreEqual(pieceInBlockadeA.currentTileIndex, pieceInBlockadeB.currentTileIndex); // blockade reform 3 steps forward
    }

    [Test]
    public void Verify_GameOver_WhenOnePlayerFinishes()
    {
        pieces.Clear();
        var finishingPiece = CreateTestPiece(0, boardRules.GetHomeTile(0) - 3);
        var otherPiece1 = CreateTestPiece(1, -1);
        var otherPiece2 = CreateTestPiece(2, -1);
        var otherPiece3 = CreateTestPiece(3, -1);
        pieces.AddRange(new List<Piece> { finishingPiece, otherPiece1, otherPiece2, otherPiece3 });

        stateMachine.StartGame();
        Assert.AreEqual(0, stateMachine.currentPlayerIndex);
        Assert.AreEqual(GamePhase.WaitingForRoll, stateMachine.gamePhase);

        // Roll and auto-finish piece
        stateMachine.RollDiceWithValues(1, 2);
        stateMachine.OnPieceClicked(finishingPiece);

        Assert.AreEqual(GamePhase.GameOver, stateMachine.gamePhase);
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
