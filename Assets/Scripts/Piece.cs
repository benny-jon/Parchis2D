using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Piece : Clickable2D
{

    public int ownerPlayerIndex;
    public int currentTileIndex = -1; // -1 = in base
    public GameManager gameManager;

    // public void OnClick()
    // {
    //     if (isClickPressed)
    //     {
    //         Debug.Log($"{this.ToString()} clicked");
    //         gameManager.OnPieceClicked(this);
    //     }
    //     isClickPressed = !isClickPressed;
    // }

    public override void OnClickUp()
    {
       Debug.Log($"{this.ToString()} clicked");
       gameManager.OnPieceClicked(this);
    }

    public void MoveToTile(int tileIndex, BoardView boardView)
    {
        currentTileIndex = tileIndex;
        transform.position = boardView.GetTilePosition(tileIndex);
    }

    public void MoveToStart(Vector3 spawnPosition)
    {
        currentTileIndex = -1;
        transform.position = spawnPosition;
    }
 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override string ToString()
    {
        return $"Piece from Player {ownerPlayerIndex}, pos = {currentTileIndex}. - " + base.ToString();
    }
}
