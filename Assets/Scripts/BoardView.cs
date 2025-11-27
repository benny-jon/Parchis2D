using UnityEngine;

public class BoardView : MonoBehaviour
{
    public Transform[] tilePoints;

    public Vector3 GetTilePosition(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= tilePoints.Length)
        {
            Debug.LogError($"Invalid tile index {tileIndex}");
            return Vector3.zero;
        }

        return tilePoints[tileIndex].position;
    }

    public void AutoAssignTiles()
    {
        tilePoints = new Transform[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            tilePoints[i] = transform.GetChild(i);
        }

        Debug.Log($"BoardView: Auto-assigned {tilePoints.Length} tiles.");
    }

    [ContextMenu("Auto Assign Tiles From Childre")]
    public void AutoAssignTilesContextMenu()
    {
        AutoAssignTiles();
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
    }
}
