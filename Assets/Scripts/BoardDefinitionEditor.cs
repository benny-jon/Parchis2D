using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoardDefinition))]
public class BoardDefinitionEditor : Editor
{
    private static readonly int MAIN_TRACK_COUNT = 68;
    private static readonly int HOME_ROW_COUNT = 8;
    private static readonly int PLAYERS = 4;

    private static readonly int TOTAL_TILES = MAIN_TRACK_COUNT + HOME_ROW_COUNT * PLAYERS;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BoardDefinition board = (BoardDefinition) target;

        GUILayout.Space(10);

        if (GUILayout.Button($"Generate Full Layout ({TOTAL_TILES} tiles)"))
        {
            GenerateFullBoard(board);
        }
    }

    private void GenerateFullBoard(BoardDefinition board)
    {
        board.tiles.Clear();

        for (int i = 0; i < TOTAL_TILES; i++)
        {
            board.tiles.Add(new BoardTile
            {
                index = i,
                type = TileType.Normal,
                ownerPlayerIndex = -1
            });
        }

        // Start Tiles
        SetTile(board, 5, TileType.Start, 0);
        SetTile(board, 5 + 17, TileType.Start, 1);
        SetTile(board, 5 + 17 * 2, TileType.Start, 2);
        SetTile(board, 5 + 17 * 3, TileType.Start, 3);

        // Safe Tiles
        SetTile(board, 12, TileType.Safe, -1);
        SetTile(board, 12 + 17, TileType.Safe, -1);
        SetTile(board, 12 + 17 * 2, TileType.Safe, -1);
        SetTile(board, 12 + 17 * 3, TileType.Safe, -1);

        // Home Entry Tiles
        SetTile(board, 0, TileType.HomeEntry, 0);
        SetTile(board, 16 + 1, TileType.HomeEntry, 1);
        SetTile(board, (16 + 1) * 2, TileType.HomeEntry, 2);
        SetTile(board, (16 + 1) * 3, TileType.HomeEntry, 3);

        // Home Tiles
        SetHomeTiles(board, 68, 0);
        SetHomeTiles(board, 68 + HOME_ROW_COUNT, 1);
        SetHomeTiles(board, 68 + HOME_ROW_COUNT * 2, 2);
        SetHomeTiles(board, 68 + HOME_ROW_COUNT * 3, 3);

        EditorUtility.SetDirty(board);
        Debug.Log($"Generated full board with {TOTAL_TILES} tiles.");
    }

    private void SetTile(BoardDefinition board, int index, TileType tileType, int owner)
    {
        if (index < 0 || index >= board.tiles.Count)
        {
            Debug.LogError($"Trying to set an invalid Tile index {index}, Type {tileType}, Owner {owner}");
        }

        board.tiles[index].type = tileType;
        board.tiles[index].ownerPlayerIndex = owner;
    }

    private void SetHomeTiles(BoardDefinition board, int startIndex, int owner)
    {
        for (int i = 0; i < HOME_ROW_COUNT - 1; i++)
        {
            SetTile(board, startIndex + i, TileType.HomeRow, owner);
        }

        SetTile(board, startIndex + HOME_ROW_COUNT - 1, TileType.Home, owner);
    }
}
