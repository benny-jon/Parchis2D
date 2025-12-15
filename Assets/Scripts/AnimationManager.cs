using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private BoardView boardView;
    [SerializeField] private float stepDuration = 0.15f;

    private int temporarySpriteSortingOrder;
    private int temporaryTextSortingOrder;

    public void PlayResetPiece(Piece piece, Vector3 spawnPoint, System.Action onComplete)
    {
        StartCoroutine(AnimatePieceResetCoroutine(piece, spawnPoint, onComplete));
    }

    private IEnumerator AnimatePieceResetCoroutine(Piece piece, Vector3 spawnPoint, System.Action onComplete)
    {
        piece.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        Vector3 startPos = piece.transform.position;
        Vector3 endPos = spawnPoint;

        float t = 0f;
        while (t < 1f)
        {
            // update piece hint with every step to show less available steps
            t += Time.deltaTime / stepDuration;
            piece.transform.position = Vector3.Lerp(startPos, endPos, Mathf.Clamp01(t));
            yield return null;
        }
        piece.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
    }

    public void PlayMove(Piece piece, List<int> pathIndices, System.Action onComplete)
    {
        StartCoroutine(AnimatePieceMoveCoroutine(piece, pathIndices, onComplete));
    }

    private IEnumerator AnimatePieceMoveCoroutine(Piece piece, List<int> pathIndices, System.Action onComplete)
    {
        HighlightPiece(piece);
        Vector3 startPos = piece.transform.position;

        foreach (var tileIndex in pathIndices)
        {
            Vector3 endPos = boardView.GetTilePosition(tileIndex);

            float t = 0f;
            while (t < 1f)
            {
                // update piece hint with every step to show less available steps
                t += Time.deltaTime / stepDuration;
                piece.transform.position = Vector3.Lerp(startPos, endPos, Mathf.Clamp01(t));
                yield return null;
            }

            startPos = endPos;
        }
        UnhighlightPiece(piece);

        onComplete?.Invoke();
    }

    private void HighlightPiece(Piece piece)
    {
        piece.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);

        var pieceImage = piece.transform.GetChild(2);    
        var pieceSprite = pieceImage.GetComponent<SpriteRenderer>();    
        pieceSprite.sortingLayerID = SortingLayer.NameToID("Animating");
        temporarySpriteSortingOrder = pieceSprite.sortingOrder;
        pieceSprite.sortingOrder = 1000;

        var pieceText = piece.transform.GetChild(0);
        var textRenderer = pieceText.GetComponent<MeshRenderer>();
        textRenderer.sortingLayerID = SortingLayer.NameToID("Animating");
        temporaryTextSortingOrder = textRenderer.sortingOrder;
        textRenderer.sortingOrder = 1100;
    }

    private void UnhighlightPiece(Piece piece)
    {
        piece.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);

        var pieceImage = piece.transform.GetChild(2);        
        var pieceSprite = pieceImage.GetComponent<SpriteRenderer>();  
        pieceSprite.sortingLayerID = SortingLayer.NameToID("Default");
        pieceSprite.sortingOrder = temporarySpriteSortingOrder;

        var pieceText = piece.transform.GetChild(0);
        var textRenderer = pieceText.GetComponent<MeshRenderer>();
        textRenderer.sortingLayerID = SortingLayer.NameToID("UI");
        textRenderer.sortingOrder = temporaryTextSortingOrder;
    }
}
