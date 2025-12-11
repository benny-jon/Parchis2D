using UnityEngine;

[ExecuteAlways]
public class BoardHudFollower : MonoBehaviour
{
    [SerializeField] public Transform worldAnchor;
    [SerializeField] public Canvas canvas;
    [SerializeField] public Camera worldCamera;
    [SerializeField] public Vector2 screenOffset;

    private RectTransform rectTransform;
    private RectTransform canvasRect;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }
    }

    private void LateUpdate() {
        if (worldAnchor == null || canvas == null || worldCamera == null || rectTransform == null) return;

        if (canvasRect == null) canvasRect = canvas.GetComponent<RectTransform>();

        // World -> Screen
        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldAnchor.position);

        // Screen -> Canvas
        Vector2 localPoint;
        Camera cameraToUse = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cameraToUse, out localPoint))
        {
            rectTransform.anchoredPosition = localPoint + screenOffset;
        }
    }
}
