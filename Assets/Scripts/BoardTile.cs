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

// public class BoardTile : MonoBehaviour
// {
//     // Start is called once before the first execution of Update after the MonoBehaviour is created
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }
