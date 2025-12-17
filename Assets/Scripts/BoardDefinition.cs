using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;

[CreateAssetMenu(fileName = "BoardDefinition", menuName = "Parchis/BoardDefinition", order = 0)]
public class BoardDefinition : ScriptableObject {

    public static readonly int MAIN_TRACK_COUNT = 68;
    public static readonly int HOME_ROW_COUNT = 8;
    public static readonly int PLAYERS = 4;

    public static readonly int TOTAL_TILES = MAIN_TRACK_COUNT + HOME_ROW_COUNT * PLAYERS;
    
    public List<BoardTile> tiles = new List<BoardTile>();

    public int[] GetHomeEntryTilesIndex()
    {
        return new int[] { 67, 16, 33, 50 };
    }

    public int[] GetStartTilesIndex()
    {
        return new int[] { 4, 4 + 17, 4 + 17 * 2, 4 + 17 * 3 };
    }

    public int[] GetHomeTilesIndex()
    {
        return tiles.FindAll(p => p.type == TileType.Home).Select(p => p.index).ToArray();
    }

    /// <summary>
    /// With 4 playeres and 100 tiles, we know the exact indexes, but 
    /// keeping it generic in case we implement other variants of Parchise (maybe over-engineered for now)
    /// </summary>
    /// <returns></returns>
    public int[] GetFirstHomeRowTilesIndex()
    {
        List<int> result = new List<int>();

        int playerIndex = 0;
        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].type == TileType.HomeRow && result.Count == playerIndex)
            {
                result.Add(i);
            } else if (tiles[i].type == TileType.Home)
            {
                playerIndex++;
            }
        }

        return result.ToArray();
    }
}
