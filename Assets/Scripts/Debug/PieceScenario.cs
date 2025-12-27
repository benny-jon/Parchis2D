using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Parchis/Test/PieceScenario", menuName = "Parchis/Test/PieceScenario")]
public class PieceScenario : ScriptableObject
{
    [Header("Name of the Scenario")]
    [Tooltip("Name for the scenario. Ex: All pieces at home")]
    public string title;

    [Header("Pieces positions")]
    public List<Entry> configuration = new List<Entry>();

    [Serializable]
    public class Entry
    {
        public int player;
        public int pieceIndex;
        public int tileIndex;

        public string GetPieceID()
        {
            return $"{player}_{pieceIndex}";
        }
    }
}
