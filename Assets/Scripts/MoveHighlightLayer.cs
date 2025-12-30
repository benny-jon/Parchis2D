using System.Collections.Generic;
using UnityEngine;

public class MoveHighlightLayer : MonoBehaviour
{
    [SerializeField] private MoveHighlight normalTilePrefab;
    [SerializeField] private BoardView boardView;
    [SerializeField] private int prewarm = 3;

    private readonly List<MoveHighlight> active = new();
    private readonly Stack<MoveHighlight> pool = new();


    private void Awake()
    {
        for (int i = 0; i < prewarm; i++)
        {
            pool.Push(CreateOne());
        }
    }

    private MoveHighlight CreateOne()
    {
        var h = Instantiate(normalTilePrefab, transform);
        h.gameObject.SetActive(false);
        return h;
    }

    private MoveHighlight Get(int tileIndex)
    {
        return pool.Count > 0 ? pool.Pop() : CreateOne();
    }

    public void ShowHighlights(MoveOption[] moveOptions)
    {
        Clear();

        for (int i = 0; i < moveOptions.Length; i++)
        {
            var option = moveOptions[i];
            var tileIndex = option.targetTileIndex;
            var h = Get(tileIndex);
            
            h.Show(
                option.targetTileIndex,
                option.piece.ownerPlayerIndex,
                boardView
            );

            active.Add(h);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < active.Count; i++)
        {
            var h = active[i];
            h.Hide();
            pool.Push(h);
        }
        active.Clear();
    }
}
