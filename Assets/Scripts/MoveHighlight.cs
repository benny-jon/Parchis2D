using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MoveHighlight : MonoBehaviour
{
    [SerializeField] private SpriteLibrary spriteLibrary;
    [SerializeField] private Vector3 normalTileScale;
    [SerializeField] private Vector3 cornerTileScale;
    [SerializeField] private Vector3 homeTileScale;

    private SpriteRenderer spriteRenderer;

    public int TileIndex { get; private set; }

    private readonly Dictionary<(int, int), int> leftCornerRadiousByQuadrant = new Dictionary<(int, int), int>
    {
        [(1, -1)] = 0,
        [(1, 1)] = 90,
        [(-1, 1)] = 180,
        [(-1, -1)] = 270
    };
    private readonly Dictionary<(int, int), int> rightCornerRadiousByQuadrant = new Dictionary<(int, int), int>
    {
        [(1, -1)] = 90,
        [(1, 1)] = 180,
        [(-1, 1)] = 270,
        [(-1, -1)] = 0
    };
    private readonly Dictionary<int, int> degreeByHome = new Dictionary<int, int>
    {
        [75] = 0,
        [83] = 90,
        [91] = 180,
        [99] = 270
    };

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Show(int tileIndex, int player, BoardView boardView)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (spriteLibrary != null)
        {
            TileIndex = tileIndex;
            transform.position = boardView.tilePoints[tileIndex].position;
            spriteRenderer.color = spriteLibrary.playersColorSecondary[player];
            transform.rotation = new Quaternion(0, 0, 0, 1f);

            // set correct background
            if (boardView.isLeftCornerTile(tileIndex))
            {
                spriteRenderer.sprite = spriteLibrary.leftCornerHighlightBackground;
            }
            else if (boardView.isRightCornerTile(tileIndex))
            {
                spriteRenderer.sprite = spriteLibrary.rightCornerHighlightBackground;
            }
            else if (boardView.boardDefinition.GetHomeTilesIndex().Contains(tileIndex))
            {
                spriteRenderer.sprite = spriteLibrary.homeHighlightBackground;
            }
            else
            {
                spriteRenderer.sprite = spriteLibrary.normalHighlightBackground;
            }

            // set rotation for corner tiles
            if (boardView.isCornerTile(tileIndex))
            {
                bool isRightCorner = boardView.isRightCornerTile(tileIndex);
                var degreesByQuadrant = GetDegreesForQuadrant(tileIndex, isRightCorner);
                transform.Rotate(new Vector3(0, 0, degreesByQuadrant), Space.Self);
            }
            else if (boardView.boardDefinition.GetHomeTilesIndex().Contains(tileIndex))
            {
                var degreesByQuadrant = GetDegreeByHome(tileIndex);
                transform.Rotate(new Vector3(0, 0, degreesByQuadrant), Space.Self);
            }
            else
            {
                // set rotation if needed for normal tiles
                if (boardView.IsTileVertical(tileIndex))
                {
                    transform.Rotate(new Vector3(0, 0, 90), Space.Self);
                }
            }

            // set scale
            if (boardView.isCornerTile(tileIndex))
            {
                transform.localScale = cornerTileScale;
            }
            else if (boardView.boardDefinition.GetHomeTilesIndex().Contains(tileIndex))
            {
                transform.localScale = homeTileScale;
            }
            else
            {
                transform.localScale = normalTileScale;
            }

            gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"{name}, SpriteLibrary not assigned");
        }
    }

    private int GetDegreeByHome(int tileIndex)
    {
        return degreeByHome[tileIndex];
    }

    private int GetDegreesForQuadrant(int tileIndex, bool isRightCorner)
    {
        var x = transform.position.x;
        var y = transform.position.y;

        var key1 = x > 0 ? 1 : -1;
        var key2 = y > 0 ? 1 : -1;

        if (isRightCorner)
        {
            return rightCornerRadiousByQuadrant[(key1, key2)];
        }
        else
        {
            return leftCornerRadiousByQuadrant[(key1, key2)];
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
