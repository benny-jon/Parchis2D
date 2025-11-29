using UnityEngine;

public class BoardDefinitionGenerator
{
    public static void GenerateFullBoard(BoardDefinition board)
    {
        board.tiles.Clear();

        for (int i = 0; i < BoardDefinition.TOTAL_TILES; i++)
        {
            board.tiles.Add(new BoardTile
            {
                index = i,
                type = TileType.Normal,
                ownerPlayerIndex = -1
            });
        }

        // Start Tiles
        SetTile(board, 5 - 1, TileType.Start, 0);
        SetTile(board, 5 + 17 - 1, TileType.Start, 1);
        SetTile(board, 5 + 17 * 2 - 1, TileType.Start, 2);
        SetTile(board, 5 + 17 * 3 - 1, TileType.Start, 3);

        // Safe Tiles
        SetTile(board, 12 - 1, TileType.Safe, -1);
        SetTile(board, 12 + 17 - 1, TileType.Safe, -1);
        SetTile(board, 12 + 17 * 2 - 1, TileType.Safe, -1);
        SetTile(board, 12 + 17 * 3 - 1, TileType.Safe, -1);

        // Home Entry Tiles
        SetTile(board, 67, TileType.HomeEntry, 0);
        SetTile(board, 67 + 9, TileType.HomeEntry, 1);
        SetTile(board, 67 + 9 * 2, TileType.HomeEntry, 2);
        SetTile(board, 67 + 9 * 3, TileType.HomeEntry, 3);

        // Home Tiles
        SetHomeColumTilesFrom(board, 68, 0);
        SetHomeColumTilesFrom(board, 68 + 8, 1);
        SetHomeColumTilesFrom(board, 68 + 8 * 2, 2);
        SetHomeColumTilesFrom(board, 68 + 8 * 3, 3);

        Debug.Log($"Generated full board with {BoardDefinition.TOTAL_TILES} tiles.");
    }

    private static void SetTile(BoardDefinition board, int index, TileType tileType, int owner)
    {
        if (index < 0 || index >= board.tiles.Count)
        {
            Debug.LogError($"Trying to set an invalid Tile index {index}, Type {tileType}, Owner {owner}");
        }

        board.tiles[index].type = tileType;
        board.tiles[index].ownerPlayerIndex = owner;
    }

    private static void SetHomeColumTilesFrom(BoardDefinition board, int startIndex, int owner)
    {
        for (int i = 0; i < BoardDefinition.HOME_ROW_COUNT - 1; i++)
        {
            SetTile(board, startIndex + i, TileType.HomeRow, owner);
        }

        SetTile(board, startIndex + BoardDefinition.HOME_ROW_COUNT - 1, TileType.Home, owner);
    }
}
