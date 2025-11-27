using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BoardDefinition", menuName = "Parchisi/BoardDefinition", order = 0)]
public class BoardDefinition : ScriptableObject {
    
    public List<BoardTile> tiles = new List<BoardTile>();    
}
