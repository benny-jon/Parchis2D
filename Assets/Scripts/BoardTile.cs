using UnityEngine;

public enum TileType {
    Normal,
    Safe,
    Start,
    HomeEntry,
    HomeRow,
    Home
}

[System.Serializable]
public class BoardTile {
    public int index;
    public TileType type;
    public int ownerPlayerIndex;
}
