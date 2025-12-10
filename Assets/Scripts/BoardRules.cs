using System.Collections.Generic;
using System.IO;
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

        // First check pure geometry
        targetIndex = TryGetTargetTileIndex(piece, steps);
        if (targetIndex == -1)
        {
            return MoveResult.InvalidMove();
        }

        // Check for blockade in the main tracks
        if (IsMoveBlockedByBlockade(piece, steps, allPieces))
        {
            return new MoveResult(MoveStatus.BlockedByBlockade, -1);
        }

        if (boardDefinition.GetHomeTilesIndex()[piece.ownerPlayerIndex] == targetIndex)
        {
            return new MoveResult(MoveStatus.ReachedHome, targetIndex);
        }

        // Check for Safe tiles
        if (IsTileSafe(targetIndex))
        {
            var enemyPiece = allPieces.Find(p => p.currentTileIndex == targetIndex && p.ownerPlayerIndex != piece.ownerPlayerIndex);
            var piecesCount = allPieces.Where(p => p.currentTileIndex == targetIndex).Count();

            if (piecesCount >= 2)
            {
                return MoveResult.InvalidMove(); // Dont allow more than 2 piece stacking
            }

            if (enemyPiece != null)
            {
                if (targetIndex == startTilebyPlayer[piece.ownerPlayerIndex])
                {
                    Debug.Log($"Kicking enemy piece from Start tile ({targetIndex}): {piece}");
                    return new MoveResult(MoveStatus.Capture, targetIndex, enemyPiece); // kick out enemy piece from my Start tile.
                }

                Debug.Log($"Enemy piece already in Safe tile {targetIndex}");
                // Update: allow sharing Safe tiles
                //return MoveResult.InvalidMove(); // Cannot land on Safe tile that has an enemy piece on it.
            }

            return new MoveResult(MoveStatus.Normal, targetIndex);
        }

        // Look for enemies on that tile
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
        if (currentIndex == -1)
        {
            if (IsBlockadeAtTile(GetStartTile(piece.ownerPlayerIndex), allPieces, out _))
            {
                return true;
            }

            // Starting tile is not blocked
            return false;
        }

        // check blockate on the other tiles
        for (int step = 1; step <= steps; step++)
        {
            int nextIndex = GetNextIndexAlongPath(currentIndex, piece.ownerPlayerIndex);
            currentIndex = nextIndex;

            if (IsBlockadeAtTile(nextIndex, allPieces, out _))
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

        // It's OK to have multiple pieces at Home
        if (boardDefinition.GetHomeTilesIndex().Contains(tileIndex))
        {
            return false;
        }

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
        int player = piece.ownerPlayerIndex;

        // In Base (-1)
        if (piece.currentTileIndex < 0)
        {
            if (steps <= 0) return INVALID_TARGET;
            if (steps == START_ROLL_REQUIREMENT)
            {
                Debug.LogWarning($"[BR] Piece in base can Start: {piece} with steps {steps}");
                return startTilebyPlayer[player] + (steps - 5);
            }

            Debug.LogWarning($"[BR] Invalid {steps} steps for piece in base {piece}");
            return INVALID_TARGET;
        }

        int currentIndex = piece.currentTileIndex;
        BoardTile currentTile = boardDefinition.tiles[currentIndex];

        // Already Home
        if (currentTile.type == TileType.Home)
        {
            Debug.LogWarning($"[BR] Piece already home {piece}");
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
                Debug.LogWarning($"[BR] {steps} steps will overshoot Home for {piece}");
                return INVALID_TARGET; // overshoot
            }

            Debug.LogWarning($"[BR] Can move {steps} steps in Home Rows: {piece}");
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
            Debug.LogWarning($"[BR] In Main track, Steps: {steps} overshoot piece: {piece}");
            return INVALID_TARGET; // overshoot
        }

        Debug.LogWarning($"[BR] Available steps {steps} for {piece}");
        return pos;
    }

    /// <summary>
    /// Returns a "progress" score along this player's path:
    /// -1 = base,
    /// 0..67 = distance from start along outer loop,
    /// 68+ = in home row (further is better).
    /// </summary>
    public int GetProgressScore(int tileIndex, int playerIndex)
    {
        if (tileIndex < 0)
            return -1; // base, not on board

        int homeStart = firstHomeRowByPlayer[playerIndex];
        int homeEndExclusive = homeStart + BoardDefinition.HOME_ROW_COUNT - 1; // one past final home
        int outerLoopLength = BoardDefinition.MAIN_TRACK_COUNT;

        // Home row segment for this player
        if (tileIndex >= homeStart && tileIndex < homeEndExclusive)
        {
            // steps in home row: 0..7 (0 = first home-row tile)
            int homeSteps = tileIndex - homeStart;
            // Outer loop fully traversed (=68) + home steps
            return outerLoopLength + homeSteps;
        }

        // Otherwise, treat as outer loop tile
        int startIndex = startTilebyPlayer[playerIndex];

        // Distance from this player's start around the loop (0..67)
        int diff = (tileIndex - startIndex + outerLoopLength) % outerLoopLength;
        return diff;
    }

    public List<int> GetPathIndices(Piece piece, int steps)
    {
        var resultPath = new List<int>();

        if (piece.currentTileIndex < 0)
        {
            resultPath.Add(startTilebyPlayer[piece.ownerPlayerIndex]);
            return resultPath;
        }

        int currentIndex = piece.currentTileIndex;

        for (int i = 0; i < steps; i++)
        {
            int nextIndex = GetNextIndexAlongPath(currentIndex, piece.ownerPlayerIndex);
            currentIndex = nextIndex;
            resultPath.Add(currentIndex);
        }

        return resultPath;
    }
}
