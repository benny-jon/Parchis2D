using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Parchis/Game Settings")]
public class GameSettings : ScriptableObject
{
    [Range(2, 4)]
    [SerializeField] 
    public int playerCount = 4;

    [SerializeField]
    public bool soundEnabled = true;

    [SerializeField]
    public bool highlightMovesEnabled = false;

    [SerializeField]
    public bool flipRedBlueUI = false;
}
