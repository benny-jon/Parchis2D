using System.Collections.Generic;
using UnityEngine;

public class MoveResult
{
    public readonly MoveStatus status;
    public readonly int targetTileIndex;
    public readonly Piece capturedPiece;

    public MoveResult(MoveStatus status, int targetTileIndex, Piece capturedPiece)
    {
        this.status = status;
        this.targetTileIndex = targetTileIndex;
        this.capturedPiece = capturedPiece;
    }

    public MoveResult(MoveStatus status, int targetTileIndex): this(status, targetTileIndex, null) { }

    public override string ToString() =>
    $"MoveResult(status={status}, targetTileIndex={targetTileIndex}, capturedPiece={capturedPiece})";

    public static MoveResult InvalidMove()
    {
        return new MoveResult(MoveStatus.Invalid, -1);
    }
}

public enum MoveStatus
{
    Invalid,
    Normal,
    Capture,
    BlockedByBlockade,
    ReachedHome,
}