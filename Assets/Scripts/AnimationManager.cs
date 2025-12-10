using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private BoardView boardView;
    [SerializeField] private float stepDuration = 0.15f;

    public void PlayMove(Piece piece, List<int> pathIndices, System.Action onComplete)
    {
        StartCoroutine(AnimatePieceMoveCoroutine(piece, pathIndices, onComplete));
    }

    private IEnumerator AnimatePieceMoveCoroutine(Piece piece, List<int> pathIndices, System.Action onComplete)
    {
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

        onComplete?.Invoke();
    }
}
