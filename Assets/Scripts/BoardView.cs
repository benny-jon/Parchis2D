using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class BoardView : MonoBehaviour
{
    public Transform[] tilePoints;
    public Transform[] pieceSpawnPoints;
    public Transform[] homeAnchorPoints;
    public BoardDefinition boardDefinition;

    [SerializeField] public float spacingBetweenPieces = 0.2f;
    [SerializeField] public Vector3 defaultPieceScale;
    [SerializeField] public Vector3 blockadePieceScale;

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
                if (piece.currentTileIndex == boardDefinition.GetHomeTilesIndex()[piece.ownerPlayerIndex])
                {
                    piece.transform.position = homeAnchorPoints[piece.ownerPlayerIndex * 4 + piece.playerPieceIndex].position;
                    piece.transform.localScale = blockadePieceScale;
                }
                else
                {
                    Vector3 offset = GetOffsetForGroup(count, i, IsTileVertical(tileIndex));
                    piece.transform.position = basePos + offset;
                    piece.transform.localScale = (count > 1) ? blockadePieceScale : defaultPieceScale;
                }
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

    public bool IsTileVertical(int tileIndex)
    {
        int HomeRowStart_Player1 = boardDefinition.GetFirstHomeRowTilesIndex()[1];
        int HomeRowStart_Player3 = boardDefinition.GetFirstHomeRowTilesIndex()[3];
        int HomRowsCount = BoardDefinition.HOME_ROW_COUNT;

        return (tileIndex >= 8 && tileIndex <= 24)
        || (tileIndex >= 42 && tileIndex <= 58)
        || (tileIndex >= HomeRowStart_Player1 && tileIndex < HomeRowStart_Player1 + HomRowsCount)
        || (tileIndex >= HomeRowStart_Player3 && tileIndex < HomeRowStart_Player3 + HomRowsCount);
    }

    public bool isCornerTile(int tileIndex)
    {
        return isLeftCornerTile(tileIndex) || isRightCornerTile(tileIndex);
    }

    public bool isRightCornerTile(int tileIndex)
    {
        return boardDefinition.GetRightCornerTilesIndex().Contains(tileIndex);
    }

    public bool isLeftCornerTile(int tileIndex)
    {
        return boardDefinition.GetLeftCornerTilesIndex().Contains(tileIndex);
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
        var boardImage = transform.Find("board_colored_v2").GetComponent<SpriteRenderer>();
        float minY = -1 * boardImage.bounds.size.y / 2; // bottom of the board since Pos is at 0,0
        float maxY = boardImage.bounds.size.y / 2; // top of the board since Pos is at 0,0
        const float minOut = 100f;
        const float maxOut = 200f;

        float t = Mathf.InverseLerp(maxY, minY, y);
        return Mathf.RoundToInt(Mathf.Lerp(minOut, maxOut, t));
    }

    [ContextMenu("Auto Assign Tiles From Children")]
    public void AutoAssignTilesContextMenu()
    {
        AutoAssignTilesPoints();
        AutoAssignHomeAnchorPoints();
        AutoAssignPieceSpawnPoints();
    }

    public void AutoAssignTilesPoints()
    {
        var trackTilesPointsParent = transform.Find("TrackTiles");
        var trackTilesPointsCount = trackTilesPointsParent.childCount;

        var homeTrackTilesPointsParent = transform.Find("HomeTrackTiles");
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

    public void AutoAssignHomeAnchorPoints()
    {
        var homeAnchorPointsParent = transform.Find("HomeAnchorPoints");
        var homeAnchorPointsCount = homeAnchorPointsParent.childCount;

        homeAnchorPoints = new Transform[homeAnchorPointsCount];

        for (int i = 0; i < homeAnchorPointsCount; i++)
        {
            homeAnchorPoints[i] = homeAnchorPointsParent.GetChild(i);
        }

        Debug.Log($"BoardView: Auto-assigned {homeAnchorPoints.Length} home anchor points");
    }

    public void AutoAssignPieceSpawnPoints()
    {
        var pieceSpawnPointsParent = transform.Find("PieceSpawnPoints");
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

        GUIStyle labelStyle = new GUIStyle();
        labelStyle.normal.textColor = Color.black;
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
                        Gizmos.DrawSphere(t.position, sphereRadius / 2);
                        break;
                    case TileType.Normal:
                    default:
                        Gizmos.DrawWireSphere(t.position, sphereRadius);
                        break;
                }
            }

            #if UNITY_EDITOR
            Handles.Label(t.position, $"{i}", labelStyle);
            #endif
        }

        // Start Box Pieces Anchor Points

        if (pieceSpawnPoints == null) return;

        Gizmos.color = Color.aquamarine;

        for (int i = 0; i < pieceSpawnPoints.Length; i++)
        {
            var p = pieceSpawnPoints[i];
            if (p == null) continue;

            Gizmos.DrawSphere(p.position, 0.10f);
        }

        // HOME Anchor points

        if (homeAnchorPoints == null) return;

        Gizmos.color = Color.white;

        for (int i = 0; i < homeAnchorPoints.Length; i++)
        {
            var p = homeAnchorPoints[i];
            if (p == null) continue;

            Gizmos.DrawSphere(p.position, 0.10f);
        }


        // Players HUD Anchor points

        Gizmos.color = Color.black;

        Transform anchorPointsParent = transform.Find("PlayersHudAnchors");
        if (anchorPointsParent != null)
        {
            for (int i = 0; i < anchorPointsParent.childCount; i++)
            {
                Gizmos.DrawSphere(anchorPointsParent.GetChild(i).position, 0.10f);
            }
        }
    }
}
