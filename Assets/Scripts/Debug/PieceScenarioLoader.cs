using System.Collections.Generic;
using UnityEngine;

public class PieceScenarioLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardView boardView;
    [SerializeField] private GameManager gameManager;

    [Header("Scenario")]
    [SerializeField] private PieceScenario scenario;

    private Dictionary<string, Piece> pieceLookup = new Dictionary<string, Piece>();

    private void Awake() {
        BuildLookup();
    }

    private void BuildLookup()
    {
        foreach (var piece in gameManager.allPieces)
        {
            pieceLookup[piece.ID] = piece;
        }
    }

    private void OnValidate() {
        if (gameManager != null)
        {
            BuildLookup();
        }
    }

    public string GetScenarioTitle()
    {
        return scenario != null ? scenario.title : "<Scenario not defined>";
    }

    public void LoadScenario()
    {
        if (scenario == null)
        {
            Debug.LogWarning("PieceScenarioLoader: No scenario assigned");
            return;
        }

        if (gameManager == null)
        {
            Debug.LogWarning("PieceScenarioLoader: No GameManager assigned");
            return;
        }

        foreach (var entry in scenario.configuration)
        {
            var piece = pieceLookup[entry.GetPieceID()];
            if (piece != null)
            {
                piece.MoveToTile(entry.tileIndex);
            }
        }

        if (boardView != null)
        {
            boardView.LayoutPieces(gameManager.allPieces);
        }
        else
        {
             Debug.LogWarning("PieceScenarioLoader: No BoardView assigned");
        }
    }
}
