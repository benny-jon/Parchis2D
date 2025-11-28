using UnityEngine;

public class BoardView : MonoBehaviour
{
    public Transform[] tilePoints;
    public Transform[] pieceSpawnPoints;

    public Vector3 GetTilePosition(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= tilePoints.Length)
        {
            Debug.LogError($"Invalid tile index {tileIndex}");
            return Vector3.zero;
        }

        return tilePoints[tileIndex].position;
    }

    [ContextMenu("Auto Assign Tiles From Childre")]
    public void AutoAssignTilesContextMenu()
    {
        AutoAssignTilesPoints();
        AutoAssignPieceSpawnPoints();
    }

        public void AutoAssignTilesPoints()
    {
        var trackTilesPointsParent = transform.GetChild(0);
        var trackTilesPointsCount = trackTilesPointsParent.childCount;

        var homeTrackTilesPointsParent = transform.GetChild(1);
        var homeTrackTilesPointsCount = homeTrackTilesPointsParent.childCount;

        tilePoints = new Transform[trackTilesPointsCount + homeTrackTilesPointsCount];

        for (int i = 0; i < trackTilesPointsCount; i++)
        {
            tilePoints[i] = trackTilesPointsParent.GetChild(i);
        }

        for (int i = 0; i < homeTrackTilesPointsCount; i++)
        {
            tilePoints[trackTilesPointsCount + i] = homeTrackTilesPointsParent.GetChild(i);
        }

        Debug.Log($"BoardView: Auto-assigned {tilePoints.Length} tiles points.");
    }

    public void AutoAssignPieceSpawnPoints()
    {
        var pieceSpawnPointsParent = transform.GetChild(2);
        var pieceSpawnPointsCount = pieceSpawnPointsParent.childCount;

        pieceSpawnPoints = new Transform[pieceSpawnPointsCount];

        for (int i = 0; i < pieceSpawnPointsCount; i++)
        {
            pieceSpawnPoints[i] = pieceSpawnPointsParent.GetChild(i);
        }

        Debug.Log($"BoardView: Auto-assigned {pieceSpawnPoints.Length} piece spawn points");
    }

    private void OnDrawGizmos() {
        if (tilePoints == null) return;

        Gizmos.color = Color.blueViolet;

        for (int i = 0; i < tilePoints.Length; i++)
        {
            var t = tilePoints[i];
            if (t == null) continue;

            Gizmos.DrawWireSphere(t.position, 0.15f);

            // #if UNITY_EDITOR

            // var style = new GUIStyle();
            // style.normal.textColor = Color.black;     // <<< CHANGE LABEL COLOR HERE
            // style.fontSize = 15;
            // style.fontStyle = FontStyle.Bold;

            // UnityEditor.Handles.Label(
            //     t.position + Vector3.up * 0.1f,
            //     i.ToString(),
            //     style
            // );

            // #endif
        }

        if (pieceSpawnPoints == null) return;

        for (int i = 0; i < pieceSpawnPoints.Length; i++)
        {
            var p = pieceSpawnPoints[i];
            if (p == null) continue;

            Gizmos.DrawWireSphere(p.position, 0.15f);
        }    
    }
}
