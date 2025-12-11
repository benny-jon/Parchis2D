using UnityEngine;

[ExecuteAlways]
public class MainCameraFitter : MonoBehaviour
{
    [SerializeField] public SpriteRenderer boardBounds;
    private Camera cam;
    private float lastAspect = -1f;

    private void Awake() {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        FitCamera();
        lastAspect = (float)Screen.width / Screen.height;
    }

    private void Update() {
        float currentAspect = (float)Screen.width / Screen.height;

        // Only refit when aspect actually changes (rotation / Game tab resolution)
        if (!Mathf.Approximately(currentAspect, lastAspect))
        {
            lastAspect = currentAspect;
            FitCamera();
        }
    }

    private void FitCamera()
    {
        Bounds b = boardBounds.bounds;
        float boardW = b.size.x;
        float boardH = b.size.y;

        float aspect = (float)Screen.width / Screen.height;

        float padding = 0.1f;
        float sizeForHeight = boardH / 2f + padding;
        float sizeForWidth  = (boardW / aspect) / 2f + padding;

        cam.orthographicSize = Mathf.Max(sizeForHeight, sizeForWidth);
    }
}
