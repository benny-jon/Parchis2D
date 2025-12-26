using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BoardRules
{
    public static string RULES_TYPE = "Spanish";

    private static readonly int INVALID_TARGET = -1;
    private static readonly int START_ROLL_REQUIREMENT = 5;
    private BoardDefinition boardDefinition;

    private int[] startTilebyPlayer;
    private int[] homeEntryByPlayer;
    private int[] firstHomeRowByPlayer;
    private int[] homeTileByPlayer;

    private readonly MoveTraceBuffer debugBuffer = new MoveTraceBuffer();
    private bool isTestingEnvironment;

    private int moveCount = 0;

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

    public void SetTestEnvironment()
    {
        isTestingEnvironment = true;
    }

    private void AddTrace(MoveTraceEntry trace)
    {
        debugBuffer.Add(trace);
    }

    private void DumpDebugTraces(string reason)
    {
       if (!isTestingEnvironment)
        {
             TraceDumpWriter.Dump(debugBuffer.Snapshot(), reason);
        }
    }    

    public int GetStartTile(int playerIndex) => startTilebyPlayer[playerIndex];

    public int GetHomeTile(int playerIndex) => homeTileByPlayer[playerIndex];

    public bool IsTileSafe(int tileIndex)
    {
        TileType type = boardDefinition.tiles[tileIndex].type;
        return type == TileType.Safe || type == TileType.HomeEntry || type == TileType.Start;
    }

    public bool IsPlayerStartTile(int tileIndex, int player)
    {
        return boardDefinition.tiles[tileIndex].type == TileType.Start && boardDefinition.tiles[tileIndex].ownerPlayerIndex == player;
    }

    public MoveResult TryResolveMove(Piece piece, int steps, List<Piece> allPieces)
    {
        var result = TryResolveMoveHelper(piece, steps, allPieces);

        //Validate invariants
        var unSafetilesWithMultiplePieces = allPieces.Where(p => p.currentTileIndex > -1 && !IsTileSafe(p.currentTileIndex)).GroupBy(p => p.currentTileIndex);
        foreach (var unSafeTile in unSafetilesWithMultiplePieces)
        {
            var pieces = unSafeTile.ToList();
            if (pieces.Count > 1 && pieces[0].ownerPlayerIndex != pieces[1].ownerPlayerIndex)
            {
                DumpDebugTraces($"Invariant violation: mixed colors on non-safe tile: Player:{pieces[0].ownerPlayerIndex} and Player:{pieces[1].ownerPlayerIndex} at {pieces[0].currentTileIndex}");
            }
        }

        return result;
    }

    public MoveResult TryResolveMoveHelper(Piece piece, int steps, List<Piece> allPieces)
    {
        moveCount++;
        int targetIndex = -1;

        // First check pure geometry
        targetIndex = TryGetTargetTileIndex(piece, steps);
        AddTrace(new MoveTraceEntry
        {
            moveId = moveCount,
            phase = "TryResolveMove-TryGetTargetTileIndex",
            player = piece.ownerPlayerIndex,
            stepsToMove = steps,
            fromTile = piece.currentTileIndex,
            toTile = targetIndex,
            tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
            note = "After geometry check"
        });
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
            AddTrace(new MoveTraceEntry
            {
                moveId = moveCount,
                phase = "TryResolveMove-CheckHomeTile",
                player = piece.ownerPlayerIndex,
                stepsToMove = steps,
                fromTile = piece.currentTileIndex,
                toTile = targetIndex,
                tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                note = $"Tile ReachedHome"
            });
            return new MoveResult(MoveStatus.ReachedHome, targetIndex);
        }

        // Check for Safe tiles
        if (IsTileSafe(targetIndex))
        {
            var piecesCount = allPieces.Where(p => p.currentTileIndex == targetIndex).Count();

            if (IsPlayerStartTile(targetIndex, piece.ownerPlayerIndex))
            {
                var enemyPieces = allPieces.Where(p => p.currentTileIndex == targetIndex && p.ownerPlayerIndex != piece.ownerPlayerIndex).ToArray();
                if (enemyPieces.Length > 0)
                {
                    if (targetIndex == startTilebyPlayer[piece.ownerPlayerIndex])
                    {
                        Debug.Log($"Kicking enemy piece(s) from Start tile ({targetIndex}): {piece}");
                        if (enemyPieces.Length == 1 && piecesCount == 2)
                        {
                            AddTrace(new MoveTraceEntry
                            {
                                moveId = moveCount,
                                phase = "TryResolveMove-IsTileSafe-IsPlayerStartTile",
                                player = piece.ownerPlayerIndex,
                                stepsToMove = steps,
                                fromTile = piece.currentTileIndex,
                                toTile = targetIndex,
                                tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                                note = $"Capture enemy piece Player:{enemyPieces[0].ownerPlayerIndex} at startTile"
                            });
                            return new MoveResult(MoveStatus.Capture, targetIndex, enemyPieces[0]); // kick out enemy piece from my Start tile.
                        }
                        else if (enemyPieces.Length == 2) // at this point these are enemies from different players, otherwise it would have been a blockade
                        {
                            AddTrace(new MoveTraceEntry
                            {
                                moveId = moveCount,
                                phase = "TryResolveMove-IsTileSafe-IsPlayerStartTile",
                                player = piece.ownerPlayerIndex,
                                stepsToMove = steps,
                                fromTile = piece.currentTileIndex,
                                toTile = targetIndex,
                                tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                                note = $"Capture last enemy piece Player:{enemyPieces[1].currentTileIndex} to arrived at StartTile"
                            });
                            // capture the last one to arrive to the Start
                            Piece lastPieceToLand = enemyPieces[0].lastTimeItMoved > enemyPieces[1].lastTimeItMoved ? enemyPieces[0] : enemyPieces[1];
                            return new MoveResult(MoveStatus.Capture, targetIndex, lastPieceToLand);
                        }
                    }
                }
            }

            if (piecesCount >= 2)
            {
                AddTrace(new MoveTraceEntry
                {
                    moveId = moveCount,
                    phase = "TryResolveMove-IsTileSafe",
                    player = piece.ownerPlayerIndex,
                    stepsToMove = steps,
                    fromTile = piece.currentTileIndex,
                    toTile = targetIndex,
                    tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                    note = $"StartTile has already 2 pieces on it"
                });
                return MoveResult.InvalidMove(); // Dont allow more than 2 piece stacking
            }

            AddTrace(new MoveTraceEntry
            {
                moveId = moveCount,
                phase = "TryResolveMove-IsTileSafe",
                player = piece.ownerPlayerIndex,
                stepsToMove = steps,
                fromTile = piece.currentTileIndex,
                toTile = targetIndex,
                tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                note = $"Normal Move"
            });
            return new MoveResult(MoveStatus.Normal, targetIndex);
        }

        // Dont allow land on Tile with 2 pieces
        if (allPieces.Where(p => p.currentTileIndex == targetIndex).ToList().Count >= 2)
        {
            AddTrace(new MoveTraceEntry
            {
                moveId = moveCount,
                phase = "TryResolveMove",
                player = piece.ownerPlayerIndex,
                stepsToMove = steps,
                fromTile = piece.currentTileIndex,
                toTile = targetIndex,
                tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                note = $"Tile has already 2 pieces on it"
            });
            return MoveResult.InvalidMove();
        }

        // Look for enemies on target tile
        int currentPlayer = piece.ownerPlayerIndex;

        var enemiesOnTile = allPieces.Where(p => p.currentTileIndex == targetIndex && p.ownerPlayerIndex != currentPlayer).ToList();

        if (enemiesOnTile.Count == 1)
        {
            AddTrace(new MoveTraceEntry
            {
                moveId = moveCount,
                phase = "TryResolveMove-LookForEnemies",
                player = piece.ownerPlayerIndex,
                stepsToMove = steps,
                fromTile = piece.currentTileIndex,
                toTile = targetIndex,
                tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                note = $"Capturing enemy tile from Player:{enemiesOnTile[0].ownerPlayerIndex}"
            });
            return new MoveResult(MoveStatus.Capture, targetIndex, enemiesOnTile[0]);
        }
        else if (enemiesOnTile.Count > 1)
        {
            AddTrace(new MoveTraceEntry
            {
                moveId = moveCount,
                phase = "TryResolveMove-LookForEnemies",
                player = piece.ownerPlayerIndex,
                stepsToMove = steps,
                fromTile = piece.currentTileIndex,
                toTile = targetIndex,
                tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                note = $"Should not reached this point: Blockade by Pieces Player:{enemiesOnTile[0].ownerPlayerIndex} and Player{enemiesOnTile[1].ownerPlayerIndex}"
            });
            // should never reach here, but keeping
            return new MoveResult(MoveStatus.BlockedByBlockade, -1); // Enemy blockade
        }

        AddTrace(new MoveTraceEntry
        {
            moveId = moveCount,
            phase = "TryResolveMove-DefaultReturn",
            player = piece.ownerPlayerIndex,
            stepsToMove = steps,
            fromTile = piece.currentTileIndex,
            toTile = targetIndex,
            tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
            note = $"Normal Move"
        });
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
            if (IsBlockadeAtTile(GetStartTile(piece.ownerPlayerIndex), allPieces, out int blockerPlayer))
            {
                AddTrace(new MoveTraceEntry
                {
                    moveId = moveCount,
                    phase = "IsMoveBlockedByBlockade",
                    player = piece.ownerPlayerIndex,
                    stepsToMove = steps,
                    fromTile = piece.currentTileIndex,
                    toTile = targetIndex,
                    tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                    note = $"Blockade at StartTile by {blockerPlayer}"
                });
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

            if (IsBlockadeAtTile(nextIndex, allPieces, out int blockerPlayer))
            {
                AddTrace(new MoveTraceEntry
                {
                    moveId = moveCount,
                    phase = "IsMoveBlockedByBlockade",
                    player = piece.ownerPlayerIndex,
                    stepsToMove = steps,
                    fromTile = piece.currentTileIndex,
                    toTile = targetIndex,
                    tileType = targetIndex != -1 ? boardDefinition.tiles[targetIndex].type.ToString() : "Invalid",
                    note = $"Blockade by {blockerPlayer}"
                });
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
                //Debug.LogWarning($"[BR] Piece in base can Start: {piece} with steps {steps}");
                return startTilebyPlayer[player] + (steps - 5);
            }

            //Debug.LogWarning($"[BR] Invalid {steps} steps for piece in base {piece}");
            return INVALID_TARGET;
        }

        int currentIndex = piece.currentTileIndex;
        BoardTile currentTile = boardDefinition.tiles[currentIndex];

        // Already Home
        if (currentTile.type == TileType.Home)
        {
            //Debug.LogWarning($"[BR] Piece already home {piece}");
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
                //Debug.LogWarning($"[BR] {steps} steps will overshoot Home for {piece}");
                return INVALID_TARGET; // overshoot
            }

            //Debug.LogWarning($"[BR] Can move {steps} steps in Home Rows: {piece}");
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
            //Debug.LogWarning($"[BR] In Main track, Steps: {steps} overshoot piece: {piece}");
            return INVALID_TARGET; // overshoot
        }

        //Debug.LogWarning($"[BR] Available steps {steps} for {piece}");
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
