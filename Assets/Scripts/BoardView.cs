using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardView : MonoBehaviour
{
    public Transform[] tilePoints;
    public Transform[] pieceSpawnPoints;
    public BoardDefinition boardDefinition;

    [SerializeField] public float spacingBetweenPieces = 0.235f;

    public Vector3 GetTilePosition(int tileIndex)
    {
        if (tileIndex < 0 || tileIndex >= tilePoints.Length)
        {
            Debug.LogError($"Invalid tile index {tileIndex}");
            return Vector3.zero;
        }

        return tilePoints[tileIndex].position;
    }

    public void LayoutPieces(IEnumerable<Piece> pieces)
    {
        var groups = pieces
        .Where(p => p != null && p.currentTileIndex >= 0)
        .GroupBy(p => p.currentTileIndex);

        foreach (var group in groups)
        {
            int tileIndex = group.Key;
            Vector3 basePos = GetTilePosition(tileIndex);

            var groupList = group.ToList();
            int count = groupList.Count;

            for (int i = 0; i < count; i++)
            {
                Piece piece = groupList[i];
                Vector3 offset = GetOffsetForGroup(count, i, IsTileVertical(tileIndex));
                piece.transform.position = basePos + offset;
                SetPieceSortingOrderByPos(piece);
            }
        }
    }

    private Vector3 GetOffsetForGroup(int count, int index, bool isVerticalTile)
    {
        float s = spacingBetweenPieces; // spacing

        switch (count)
        {
            case 1: return Vector3.zero;
            case 2:
                if (isVerticalTile)
                {
                    return (index == 0) ? new Vector3(0f, -s, 0f) : new Vector3(0f, s, 0f);
                }
                else
                {
                    return (index == 0) ? new Vector3(-s, 0f, 0f) : new Vector3(s, 0f, 0f);
                }
            case 3:
                if (isVerticalTile)
                {
                    if (index == 0) return new Vector3(-s, -s, 0f);
                    if (index == 1) return new Vector3(-s, s, 0f);
                    return new Vector3(s * 0.7f, 0f, 0f);
                }
                else
                {
                    if (index == 0) return new Vector3(-s, -s, 0f);
                    if (index == 1) return new Vector3(s, -s, 0f);
                    return new Vector3(0f, s * 0.7f, 0f);
                }
            case 4:
                switch (index)
                {
                    case 0: return new Vector3(-s, -s, 0f);
                    case 1: return new Vector3(s, -s, 0f);
                    case 2: return new Vector3(-s, s, 0f);
                    default: return new Vector3(s, s, 0f);
                }
            default:
                Debug.Log($"Stacking TOO many pieces {count}");
                float angle = (float)(Math.PI * 2f * index / count);
                return new Vector3(Mathf.Cos(angle) * s, Mathf.Sin(angle) * s, 0f);
        }
    }

    private bool IsTileVertical(int tileIndex)
    {
        int HomeRowStart_Player1 = boardDefinition.GetFirstHomeRowTilesIndex()[1];
        int HomeRowStart_Player3 = boardDefinition.GetFirstHomeRowTilesIndex()[3];
        int HomRowsCount = BoardDefinition.HOME_ROW_COUNT;

        return (tileIndex >= 8 && tileIndex <= 24)
        || (tileIndex >= 42 && tileIndex <= 58)
        || (tileIndex >= HomeRowStart_Player1 && tileIndex <= HomeRowStart_Player1 + HomRowsCount)
        || (tileIndex >= HomeRowStart_Player3 && tileIndex <= HomeRowStart_Player3 + HomRowsCount);
    }

    private void SetPieceSortingOrderByPos(Piece piece)
    {
        if (piece.transform.childCount < 3) return;

        var pieceImage = piece.transform.GetChild(2);    
        var pieceSprite = pieceImage.GetComponent<SpriteRenderer>();    
        var newOrder = MapYToSorting(piece.transform.position.y);
        pieceSprite.sortingOrder = newOrder;

        var pieceText = piece.transform.GetChild(0);
        var textRenderer = pieceText.GetComponent<MeshRenderer>();
        textRenderer.sortingOrder = newOrder + 10;
    }

    private int MapYToSorting(float y)
    {
        const float minY = -3.9f;
        const float maxY = 5.2f;
        const float minOut = 100f;
        const float maxOut = 200f;

        float t = Mathf.InverseLerp(maxY, minY, y);
        return Mathf.RoundToInt(Mathf.Lerp(minOut, maxOut, t));
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

    private void OnDrawGizmos()
    {
        if (tilePoints == null) return;

        float sphereRadius = 0.15f;

        Gizmos.color = Color.blueViolet;

        for (int i = 0; i < tilePoints.Length; i++)
        {
            var t = tilePoints[i];
            if (t == null) continue;

            if (boardDefinition != null)
            {
                var boardTile = boardDefinition.tiles[i];

                switch (boardTile.type)
                {
                    case TileType.Start:
                    case TileType.HomeEntry:
                    case TileType.Safe:
                        Gizmos.DrawSphere(t.position, sphereRadius);
                        break;
                    case TileType.Home:
                        Gizmos.DrawSphere(t.position, sphereRadius);
                        break;
                    case TileType.Normal:
                    default:
                        Gizmos.DrawWireSphere(t.position, sphereRadius);
                        break;
                }
            }

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

        Gizmos.color = Color.aquamarine;

        for (int i = 0; i < pieceSpawnPoints.Length; i++)
        {
            var p = pieceSpawnPoints[i];
            if (p == null) continue;

            Gizmos.DrawSphere(p.position, 0.10f);
        }

        Gizmos.color = Color.black;

        Transform anchorPointsParent = transform.GetChild(3);
        if (anchorPointsParent != null)
        {
            for (int i = 0; i < anchorPointsParent.childCount; i++)
            {
                Gizmos.DrawSphere(anchorPointsParent.GetChild(i).position, 0.10f);
            }
        }
    }
}
