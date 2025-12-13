using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Piece : Clickable2D
{

    public int ownerPlayerIndex;
    public int currentTileIndex = -1; // -1 = in base
    public GameManager gameManager;

    [Header("UI")]
    public TMP_Text moveHintLabel;

    public double lastTimeItMoved { get; private set;}

    public override void OnClickUp()
    {
       gameManager.OnPieceClicked(this, transform);
    }

    public void MoveToTile(int tileIndex)
    {
        currentTileIndex = tileIndex;
        lastTimeItMoved = Time.timeSinceLevelLoadAsDouble;
    }

    public void MoveToStart(Vector3 spawnPosition)
    {
        currentTileIndex = -1;
        transform.position = spawnPosition;
        lastTimeItMoved = Time.timeSinceLevelLoadAsDouble;
    }

    public void SetMoveHints(List<MoveOption> moveOptions)
    {
        Debug.Log($"{ToString()}: Set hint: {moveOptions.Count} move(s)");

        if (moveOptions == null || moveOptions.Count == 0)
        {
            ClearMoveHints();
            return;
        }

        var possibleSteps = moveOptions.Select(o => o.steps).OrderBy(s => s);

        if (moveHintLabel != null)
        {
            moveHintLabel.text = string.Join(",", possibleSteps);
        }
        else
        {
            Debug.LogError($"Hint Label Text is NULL for {ToString()}");
        }
    }

    public void ClearMoveHints()
    {
        if (moveHintLabel != null)
        {
            moveHintLabel.text = string.Empty;
        } 
        else
        {
            Debug.LogError($"Hint Label Text is NULL for {ToString()}");
        }
    }

    public override string ToString()
    {
        return $"Piece from Player {ownerPlayerIndex}, pos = {currentTileIndex}. - " + base.ToString();
    }
}
