using UnityEngine;

public class MoveOption
{
    public readonly Piece piece;
    public readonly int targetTileIndex;
    public readonly int steps;
    public readonly bool usesDice1;
    public readonly bool usesDice2;

    public MoveOption(Piece piece, int targetTileIndex, int steps, bool usesDice1, bool usesDice2)
    {
        this.piece = piece;
        this.targetTileIndex = targetTileIndex;
        this.steps = steps;
        this.usesDice1 = usesDice1;
        this.usesDice2 = usesDice2;
    }

    public override string ToString()
    {
        return $"piece={piece}, targetTile={targetTileIndex}, steps={steps}, dice1={usesDice1}, dice2={usesDice2}";
    }
}
