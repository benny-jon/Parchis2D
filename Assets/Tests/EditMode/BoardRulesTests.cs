using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class BoardRulesTests
{
    private BoardDefinition boardDefinition;
    private BoardRules rules;

    [SetUp]
    public void SetUp()
    {
        boardDefinition = ScriptableObject.CreateInstance<BoardDefinition>();
        BoardDefinitionGenerator.GenerateFullBoard(boardDefinition);

        rules = new BoardRules(boardDefinition);
    }

    [Test]
    public void BasePiece_CannotLeaveWithoutRollingFive()
    {
        for (int owner = 0; owner < 4; owner++)
        {
            var piece = CreateTestPiece(owner, -1);

            Assert.IsTrue(rules.TryGetTargetTileIndex(piece, 1) == -1); // cannot move
            Assert.IsTrue(rules.TryGetTargetTileIndex(piece, 2) == -1); // cannot move
            Assert.IsTrue(rules.TryGetTargetTileIndex(piece, 3) == -1); // cannot move
            Assert.IsTrue(rules.TryGetTargetTileIndex(piece, 4) == -1); // cannot move
            Assert.IsTrue(rules.TryGetTargetTileIndex(piece, 6) == -1); // cannot move
            Assert.IsTrue(rules.TryGetTargetTileIndex(piece, 5) != -1); // can start!
        }
    }

    [Test]
    public void BasePiece_CanStart()
    {
        var piece1 = CreateTestPiece(1, -1);
        var piece2 = CreateTestPiece(1, -1);
        var piece3 = CreateTestPiece(1, 52);
        var piece4 = CreateTestPiece(1, -1);

        var allPieces = new List<Piece>() { piece1, piece2, piece3, piece4, };

        MoveResult resultFor1 = rules.TryResolveMove(piece1, 5, allPieces);
        Assert.AreEqual(MoveStatus.Normal, resultFor1.status);
        Assert.AreEqual(boardDefinition.GetStartTilesIndex()[1], resultFor1.targetTileIndex);

        MoveResult resultFor2 = rules.TryResolveMove(piece2, 5, allPieces);
        Assert.AreEqual(MoveStatus.Normal, resultFor2.status);
        Assert.AreEqual(boardDefinition.GetStartTilesIndex()[1], resultFor2.targetTileIndex);

        MoveResult resultFor4 = rules.TryResolveMove(piece4, 5, allPieces);
        Assert.AreEqual(MoveStatus.Normal, resultFor4.status);
        Assert.AreEqual(boardDefinition.GetStartTilesIndex()[1], resultFor4.targetTileIndex);
    }

    [Test]
    public void FirstPlayerPiece_ShouldEntryHomeRows()
    {
        for (int steps = 1; steps <= 10; steps++)
        {
            var expectedTargetIndex = 65 + steps;
            var piece = CreateTestPiece(0, 65);
            var targetIndex = rules.TryGetTargetTileIndex(piece, steps);
            Assert.AreEqual(expectedTargetIndex, targetIndex);
        }
    }

    [Test]
    public void SecondPlayerPiece_ShouldEntryHomeRows()
    {
        int[] expectedIndexes = new int[] { 14, 15, 16, 76, 77 };
        for (int steps = 0; steps < expectedIndexes.Length; steps++)
        {
            var piece = CreateTestPiece(1, 14);
            var targetIndex = rules.TryGetTargetTileIndex(piece, steps);
            Assert.AreEqual(expectedIndexes[steps], targetIndex);
        }
    }

    [Test]
    public void ThirdPlayerPiece_ShouldEntryHomeRows()
    {
        int[] expectedIndexes = new int[] { 32, 33, 84, 85, 86 };
        for (int steps = 0; steps < expectedIndexes.Length; steps++)
        {
            var piece = CreateTestPiece(2, 32);
            var targetIndex = rules.TryGetTargetTileIndex(piece, steps);
            Assert.AreEqual(expectedIndexes[steps], targetIndex);
        }
    }

    [Test]
    public void FourthPlayerPiece_ShouldEntryHomeRows()
    {
        int[] expectedIndexes = new int[] { 49, 50, 92, 93, 94 };
        for (int steps = 0; steps < expectedIndexes.Length; steps++)
        {
            var piece = CreateTestPiece(3, 49);
            var targetIndex = rules.TryGetTargetTileIndex(piece, steps);
            Assert.AreEqual(expectedIndexes[steps], targetIndex);
        }
    }

    [Test]
    public void FirstPlayerPiece_ShouldContinueInHomeRowsUntilHome()
    {
        for (int steps = 1; steps <= 7; steps++)
        {
            var firstHomeRowTileIndex = boardDefinition.GetFirstHomeRowTilesIndex()[0];
            var expectedTargetIndex = firstHomeRowTileIndex + steps;
            var piece = CreateTestPiece(0, firstHomeRowTileIndex);
            var targetIndex = rules.TryGetTargetTileIndex(piece, steps);
            Assert.AreEqual(expectedTargetIndex, targetIndex);
        }
    }

    [Test]
    public void LastPiece_CanArriveHome()
    {
        int homeTileIndex = boardDefinition.GetHomeTilesIndex()[0];
        int firstHomeRowTileIndex = boardDefinition.GetFirstHomeRowTilesIndex()[0];

        List<Piece> pieces = new List<Piece>()
        {
            CreateTestPiece(0, homeTileIndex),
            CreateTestPiece(0, homeTileIndex),
            CreateTestPiece(0, homeTileIndex),
            CreateTestPiece(0, firstHomeRowTileIndex),
        };

        var result = rules.TryResolveMove(pieces[3], BoardDefinition.HOME_ROW_COUNT - 1, pieces);

        Assert.AreEqual(MoveStatus.ReachedHome, result.status);
        Assert.AreEqual(homeTileIndex, result.targetTileIndex);
    }

    [Test]
    public void FirstPlayerPiece_ShouldNotOvershootHomeRows()
    {
        var piece = CreateTestPiece(0, 68);
        var targetIndex = rules.TryGetTargetTileIndex(piece, 9);
        Assert.AreEqual(-1, targetIndex);
    }

    [Test]
    public void PlayersPiece_AlreadyHome()
    {
        for (int player = 0; player < 4; player++)
        {
            var piece = CreateTestPiece(player, boardDefinition.GetFirstHomeRowTilesIndex()[player] + BoardDefinition.HOME_ROW_COUNT - 1);
            var targetIndex = rules.TryGetTargetTileIndex(piece, 1);
            Assert.AreEqual(-1, targetIndex);
        }
    }

    [Test]
    public void Test_TryingTooManyMoves()
    {
        var piece = CreateTestPiece(0, boardDefinition.GetStartTilesIndex()[0]);
        var targetIndex = rules.TryGetTargetTileIndex(piece, BoardDefinition.TOTAL_TILES * 2); // too many moves
        Assert.AreEqual(-1, targetIndex);
    }

    [Test]
    public void Test_HomeEntry_Tiles()
    {
        Assert.AreEqual(TileType.HomeEntry, boardDefinition.tiles[67].type);
        Assert.AreEqual(TileType.HomeEntry, boardDefinition.tiles[16].type);
        Assert.AreEqual(TileType.HomeEntry, boardDefinition.tiles[33].type);
        Assert.AreEqual(TileType.HomeEntry, boardDefinition.tiles[50].type);
    }

    [Test]
    public void Test_Home_Tiles()
    {
        Assert.AreEqual(TileType.Home, boardDefinition.tiles[67 + 8].type);
        Assert.AreEqual(TileType.Home, boardDefinition.tiles[67 + 8 * 2].type);
        Assert.AreEqual(TileType.Home, boardDefinition.tiles[67 + 8 * 3].type);
        Assert.AreEqual(TileType.Home, boardDefinition.tiles[67 + 8 * 4].type);
    }

    [Test]
    public void Test_Start_Tiles()
    {
        Assert.AreEqual(4, rules.GetStartTile(0));
        Assert.AreEqual(4 + 17, rules.GetStartTile(1));
        Assert.AreEqual(4 + 17 * 2, rules.GetStartTile(2));
        Assert.AreEqual(4 + 17 * 3, rules.GetStartTile(3));

        Assert.AreEqual(TileType.Start, boardDefinition.tiles[rules.GetStartTile(0)].type);
        Assert.AreEqual(TileType.Start, boardDefinition.tiles[rules.GetStartTile(1)].type);
        Assert.AreEqual(TileType.Start, boardDefinition.tiles[rules.GetStartTile(2)].type);
        Assert.AreEqual(TileType.Start, boardDefinition.tiles[rules.GetStartTile(3)].type);
    }

    [Test]
    public void FromStartToHome()
    {
        // Piece in the start tile
        var piece = CreateTestPiece(0, 4);
        // Move all the available tiles
        var targetIndex = rules.TryGetTargetTileIndex(piece, 68 - 5 + 8);
        var targetTile = boardDefinition.tiles[targetIndex];

        Assert.AreEqual(TileType.Home, targetTile.type);
        Assert.AreEqual(0, targetTile.ownerPlayerIndex);
    }

    [Test]
    public void VerifyPiece_CannotPassBlockade()
    {
        var pieceA1 = CreateTestPiece(0, 4);
        var pieceA2 = CreateTestPiece(0, 4);

        var trackPiece = CreateTestPiece(1, 2);

        var result = rules.TryResolveMove(trackPiece, 4, new Piece[] { pieceA1, pieceA2, trackPiece}.ToList());

        Assert.AreEqual(result.status, MoveStatus.BlockedByBlockade);
        Assert.AreEqual(result.targetTileIndex, -1);
    }

    [Test]
    public void VerifyPiece_CannotStartWithBlockade()
    {
        var pieceA1 = CreateTestPiece(0, boardDefinition.GetStartTilesIndex()[0]);
        var pieceA2 = CreateTestPiece(0, boardDefinition.GetStartTilesIndex()[0]);

        var trackPiece = CreateTestPiece(0, -1);

        var result = rules.TryResolveMove(trackPiece, 5, new Piece[] { pieceA1, pieceA2, trackPiece}.ToList());

        Assert.AreEqual(result.status, MoveStatus.BlockedByBlockade);
        Assert.AreEqual(result.targetTileIndex, -1);
    }

    [Test]
    public void VerifyPiece_CanIfNotBlockade()
    {
        var pieceA1 = CreateTestPiece(0, boardDefinition.GetStartTilesIndex()[0] - 3);
        var pieceA2 = CreateTestPiece(0, boardDefinition.GetStartTilesIndex()[0] - 3);

        var trackPiece = CreateTestPiece(3, -1);

        var result = rules.TryResolveMove(trackPiece, 5, new Piece[] { pieceA1, pieceA2, trackPiece}.ToList());

        Assert.AreEqual(result.status, MoveStatus.Normal);
        Assert.AreEqual(result.targetTileIndex, boardDefinition.GetStartTilesIndex()[3]);
    }

    [Test]
    public void Piece_CanGoOver_SafeTileWithEnemy()
    {
        var enemyPiece = CreateTestPiece(0, boardDefinition.GetHomeEntryTilesIndex()[1]);
        var mainPiece = CreateTestPiece(2, boardDefinition.GetHomeEntryTilesIndex()[1] - 3);

        var result = rules.TryResolveMove(mainPiece, 5, new List<Piece> { enemyPiece, mainPiece });

        Assert.AreEqual(MoveStatus.Normal, result.status);
        Assert.AreEqual(boardDefinition.GetHomeEntryTilesIndex()[1] + 2, result.targetTileIndex);
    }

    [Test]
    public void Piece_SharesSafeTileWithEnemy()
    {
        var homeEntryTile = boardDefinition.GetHomeEntryTilesIndex()[1];
        Assert.AreEqual(TileType.HomeEntry, boardDefinition.tiles[homeEntryTile].type);
        Assert.True(rules.IsTileSafe(homeEntryTile));
    
        var enemyPiece = CreateTestPiece(0, boardDefinition.GetHomeEntryTilesIndex()[1]);
        var mainPiece = CreateTestPiece(2, boardDefinition.GetHomeEntryTilesIndex()[1] - 3);

        var result = rules.TryResolveMove(mainPiece, 3, new List<Piece> { enemyPiece, mainPiece });

        Assert.AreEqual(MoveStatus.Normal, result.status);
        Assert.AreEqual(boardDefinition.GetHomeEntryTilesIndex()[1], result.targetTileIndex);
    }

    [Test]
    public void Piece_CannotLandOnSafeTile_WithTwoEnemiesThere()
    {
        var homeEntryTile = boardDefinition.GetHomeEntryTilesIndex()[1];
        Assert.AreEqual(TileType.HomeEntry, boardDefinition.tiles[homeEntryTile].type);
        Assert.True(rules.IsTileSafe(homeEntryTile));
    
        var enemyPiece = CreateTestPiece(0, boardDefinition.GetHomeEntryTilesIndex()[1]);
        var enemyPieceB = CreateTestPiece(3, boardDefinition.GetHomeEntryTilesIndex()[1]);
        var mainPiece = CreateTestPiece(2, boardDefinition.GetHomeEntryTilesIndex()[1] - 3);

        var result = rules.TryResolveMove(mainPiece, 3, new List<Piece> { enemyPiece, enemyPieceB, mainPiece });

        Assert.AreEqual(MoveStatus.Invalid, result.status);
        Assert.AreEqual(-1, result.targetTileIndex);
    }

    [Test]
    public void Piece_CannotLandOnSafeTile_WithTwoOtherPiecesThere()
    {
        var homeEntryTile = boardDefinition.GetHomeEntryTilesIndex()[1];
        Assert.AreEqual(TileType.HomeEntry, boardDefinition.tiles[homeEntryTile].type);
        Assert.True(rules.IsTileSafe(homeEntryTile));
    
        var enemyPiece = CreateTestPiece(0, boardDefinition.GetHomeEntryTilesIndex()[1]);
        var mainOtherPiece = CreateTestPiece(2, boardDefinition.GetHomeEntryTilesIndex()[1]);
        var mainPiece = CreateTestPiece(2, boardDefinition.GetHomeEntryTilesIndex()[1] - 3);

        var result = rules.TryResolveMove(mainPiece, 3, new List<Piece> { enemyPiece, mainOtherPiece, mainPiece });

        Assert.AreEqual(MoveStatus.Invalid, result.status);
        Assert.AreEqual(-1, result.targetTileIndex);
    }

    [Test]
    public void Piece_CanLandOnStartTile_WithTwoDifferentEnemiesThereAndCaptureLastInArrive()
    {
        var playerStartTile = boardDefinition.GetStartTilesIndex()[2];
        Assert.AreEqual(TileType.Start, boardDefinition.tiles[playerStartTile].type);
        Assert.True(rules.IsTileSafe(playerStartTile));
    
        var enemyPiece = CreateTestPiece(0, playerStartTile - 2);
        enemyPiece.MoveToTile(playerStartTile); // lands first
        var enemyPieceB = CreateTestPiece(3, playerStartTile - 1);
        enemyPieceB.MoveToTile(playerStartTile); // lands second
        var mainPiece = CreateTestPiece(2, -1);

        var result = rules.TryResolveMove(mainPiece, 5, new List<Piece> { enemyPiece, enemyPieceB, mainPiece });

        Assert.AreEqual(MoveStatus.Capture, result.status);
        Assert.AreEqual(playerStartTile, result.targetTileIndex);
        Assert.AreEqual(enemyPieceB, result.capturedPiece); // capturing the last one to arrive
    }

    [Test]
    public void Piece_CanStartAnd_ShareTileWithEnemy()
    {
        var playerOneStartTile = boardDefinition.GetStartTilesIndex()[1];
        Assert.AreEqual(TileType.Start, boardDefinition.tiles[playerOneStartTile].type);

        var enemyPiece = CreateTestPiece(0, playerOneStartTile);
        var mainPiece = CreateTestPiece(1, -1);

        var result = rules.TryResolveMove(mainPiece, 5, new List<Piece> { enemyPiece, mainPiece });

        Assert.AreEqual(MoveStatus.Normal, result.status);
        Assert.AreEqual(playerOneStartTile, result.targetTileIndex);
    }

    [Test]
    public void Piece_CannotStart_WithEnemyBlockade_OnStartTile()
    {
        var playerOneStartTile = boardDefinition.GetStartTilesIndex()[1];
        Assert.AreEqual(TileType.Start, boardDefinition.tiles[playerOneStartTile].type);

        var enemyPieceA = CreateTestPiece(0, playerOneStartTile);
        var enemyPieceB = CreateTestPiece(0, playerOneStartTile);
        var mainPiece = CreateTestPiece(1, -1);

        var result = rules.TryResolveMove(mainPiece, 5, new List<Piece> { enemyPieceA, enemyPieceB, mainPiece });

        Assert.AreEqual(MoveStatus.BlockedByBlockade, result.status);
        Assert.AreEqual(-1, result.targetTileIndex);
    }

    // Parametized test arguments
    static int[] ownerIndex = new int[] { 0, 1, 2, 3 };

    // Parametized test example
    [UnityTest]
    public IEnumerator FromStartToHomeForAllPlayers([ValueSource("ownerIndex")] int owner)
    {
        var startTile = boardDefinition.GetStartTilesIndex()[owner];
        // Piece in the start tile
        var piece = CreateTestPiece(owner, startTile);

        // Move all the available tiles steps
        var targetIndex = rules.TryGetTargetTileIndex(piece, 68 - 5 + 8);
        Assert.AreNotEqual(-1, targetIndex);
        var targetTile = boardDefinition.tiles[targetIndex];

        // Verifications
        Assert.AreEqual(TileType.Home, targetTile.type);
        Assert.AreEqual(owner, targetTile.ownerPlayerIndex);

        yield return null;
    }

    private Piece CreateTestPiece(int ownerPlayerIndex, int currentTileIndex)
    {
        var go = new GameObject("TestPiece");
        var piece = go.AddComponent<Piece>();
        piece.ownerPlayerIndex = ownerPlayerIndex;
        piece.currentTileIndex = currentTileIndex;
        return piece;
    }
}
