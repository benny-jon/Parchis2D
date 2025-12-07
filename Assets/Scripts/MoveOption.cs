using UnityEngine;

public class MoveOption
{
    public readonly Piece piece;
    public readonly int targetTileIndex;
    public readonly int steps;
    public readonly bool usesDice1;
    public readonly bool usesDice2;
    public readonly int bonusIndex;

    public MoveOption(Piece piece, int targetTileIndex, int steps, bool usesDice1, bool usesDice2)
    {
        this.piece = piece;
        this.targetTileIndex = targetTileIndex;
        this.steps = steps;
        this.usesDice1 = usesDice1;
        this.usesDice2 = usesDice2;
        this.bonusIndex = -1;
    }

    public MoveOption(Piece piece, int targetTileIndex, int steps, int bonusIndex)
    {
        this.piece = piece;
        this.targetTileIndex = targetTileIndex;
        this.steps = steps;
        this.usesDice1 = false;
        this.usesDice2 = false;
        this.bonusIndex = bonusIndex;
    }

    public override string ToString()
    {
        if (bonusIndex >= 0)
        {
            return $"piece={piece}, targetTile={targetTileIndex}, steps={steps}, bonusIndex={bonusIndex}";
        }
        return $"piece={piece}, targetTile={targetTileIndex}, steps={steps}, dice1={usesDice1}, dice2={usesDice2}";
    }
}
