using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class BoardRules
{
    private static readonly int INVALID_TARGET = -1;
    private static readonly int START_ROLL_REQUIREMENT = 5;
    private BoardDefinition boardDefinition;

    private int[] startTilebyPlayer;
    private int[] homeEntryByPlayer;
    private int[] firstHomeRowByPlayer;
    private int[] homeTileByPlayer;

    public BoardRules(BoardDefinition boardDefinition)
    {
        this.boardDefinition = boardDefinition;

        startTilebyPlayer = boardDefinition.GetStartTilesIndex();
        homeEntryByPlayer = boardDefinition.GetHomeEntryTilesIndex();
        firstHomeRowByPlayer = boardDefinition.GetFirstHomeRowTilesIndex();
        homeTileByPlayer = boardDefinition.GetHomeTilesIndex();

        Debug.Log($"Initialized: Start Tiles {ArrayUtils.ToString(startTilebyPlayer)}");
        Debug.Log($"Initialized: Home Entry Tiles {ArrayUtils.ToString(homeEntryByPlayer)}");
        Debug.Log($"Initialized: First Home Row Tiles {ArrayUtils.ToString(firstHomeRowByPlayer)}");
    }

    public int GetStartTile(int playerIndex) => startTilebyPlayer[playerIndex];

    public int GetHomeTile(int playerIndex) => homeTileByPlayer[playerIndex];

    public bool IsTileSafe(int tileIndex)
    {
        TileType type = boardDefinition.tiles[tileIndex].type;
        return type == TileType.Safe || type == TileType.HomeEntry || type == TileType.Start;
    }

    public MoveResult TryResolveMove(Piece piece, int steps, List<Piece> allPieces)
    {
        int targetIndex = -1;

        // 1) First check pure geometry
        targetIndex = TryGetTargetTileIndex(piece, steps);
        if (targetIndex == -1)
        {
            return MoveResult.InvalidMove();
        }

        // 2) Check for blockade
        if (IsMoveBlockedByBlockade(piece, steps, allPieces))
        {
            return new MoveResult(MoveStatus.BlockedByBlockade, -1);
        }

        // 3) Check for Safe tiles
        if (IsTileSafe(targetIndex))
        {
            return new MoveResult(MoveStatus.Normal, targetIndex);
        }

        // 4) Look for enemies on that tile
        int currentPlayer = piece.ownerPlayerIndex;

        var enemiesOnTile = allPieces.Where(p => p.currentTileIndex == targetIndex && p.ownerPlayerIndex != currentPlayer).ToList();

        if (enemiesOnTile.Count == 1)
        {
            return new MoveResult(MoveStatus.Capture, targetIndex, enemiesOnTile[0]);
        }
        else if (enemiesOnTile.Count > 1)
        {
            return new MoveResult(MoveStatus.BlockedByBlockade, -1); // Enemy blockade
        }

        // nothing to capture
        return new MoveResult(MoveStatus.Normal, targetIndex);
    }

    public bool IsMoveBlockedByBlockade(Piece piece, int steps, List<Piece> allPieces)
    {
        int targetIndex = TryGetTargetTileIndex(piece, steps);
        if (targetIndex == -1)
        {
            return true;
        }

        int currentIndex = piece.currentTileIndex;
        
        // check blockate on the start tile
        if (currentIndex == -1 &&
            steps == START_ROLL_REQUIREMENT &&
            IsBlockadeAtTile(GetStartTile(piece.ownerPlayerIndex), allPieces, out int blackadeOwner))
        {
            return true;
        }

        // check blockate on the other tiles
        for (int step = 1; step <= steps; step++)
        {
            int nextIndex = GetNextIndexAlongPath(currentIndex, piece.ownerPlayerIndex);
            currentIndex = nextIndex;

            if (IsBlockadeAtTile(nextIndex, allPieces, out blackadeOwner))
            {
                return true;
            }
        }

        return false;
    }

    private int GetNextIndexAlongPath(int currentIndex, int player)
    {
        int pos = currentIndex;
        if (pos == homeEntryByPlayer[player])
        {
            pos = firstHomeRowByPlayer[player];
        }
        else if (pos < BoardDefinition.MAIN_TRACK_COUNT)
        {
            pos = (pos + 1) % BoardDefinition.MAIN_TRACK_COUNT;
        }
        else
        {
            pos = pos + 1;
        }
        return pos;
    }

    public bool IsBlockadeAtTile(int tileIndex, List<Piece> allPieces, out int ownerPlayerIndex)
    {
        ownerPlayerIndex = -1;

        // Count pieces on this tile grouped by owner
        var groups = allPieces
            .Where(p => p != null && p.currentTileIndex == tileIndex)
            .GroupBy(p => p.ownerPlayerIndex);

        foreach (var g in groups)
        {
            int count = g.Count();
            if (count >= 2)
            {
                ownerPlayerIndex = g.Key;
                return true;
            }
        }

        return false;
    }

    public int TryGetTargetTileIndex(Piece piece, int steps)
    {
        // TODO Add logs to find bug where Player 1 pieces in Base show not available moves when running a 5 (1, 4) or (3, 5), etc;
        
        int player = piece.ownerPlayerIndex;

        // In Base (-1)
        if (piece.currentTileIndex < 0)
        {
            if (steps <= 0) return INVALID_TARGET;
            if (steps == START_ROLL_REQUIREMENT)
            {
                return startTilebyPlayer[player] + (steps - 5);
            }

            return INVALID_TARGET;
        }

        int currentIndex = piece.currentTileIndex;
        BoardTile currentTile = boardDefinition.tiles[currentIndex];

        // Already Home
        if (currentTile.type == TileType.Home)
        {
            return INVALID_TARGET;
        }

        // In Home Rows
        if (currentTile.type == TileType.HomeRow)
        {
            int firstHomeRow = firstHomeRowByPlayer[player];
            int offset = currentIndex - firstHomeRow;
            int newOffset = offset + steps;

            if (newOffset > BoardDefinition.HOME_ROW_COUNT - 1)
            {
                return INVALID_TARGET; // overshoot
            }

            return firstHomeRow + newOffset;
        }

        // On Main Tracks
        int pos = currentIndex;
        for (int i = 0; i < steps; i++)
        {
            if (pos == homeEntryByPlayer[player])
            {
                pos = firstHomeRowByPlayer[player];
            }
            else if (pos < BoardDefinition.MAIN_TRACK_COUNT)
            {
                pos = (pos + 1) % BoardDefinition.MAIN_TRACK_COUNT;
            }
            else
            {
                pos = pos + 1;
            }
        }

        if (pos > firstHomeRowByPlayer[player] + BoardDefinition.HOME_ROW_COUNT - 1)
        {
            return INVALID_TARGET; // overshoot
        }

        return pos;
    }
}
