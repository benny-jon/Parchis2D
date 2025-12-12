using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovePopupUI : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] public Canvas canvas;
    [SerializeField] public Camera worldCamera;
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private RectTransform popupParentRect;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionLabels;
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 40);

    [Header("Overlay")]
    [SerializeField] private Button overlayButton; 

    private Action<int> _onPickIndex;

    private void Awake() {
        HideImmediate();

        if (overlayButton != null)
        {
            overlayButton.onClick.RemoveAllListeners();
            overlayButton.onClick.AddListener(HideImmediate);
        }
    }

    public void Show(Transform worldAnchor, MoveOption[] options, Action<int> onPickIndex)
    {
        if (worldAnchor == null || options == null || options.Length < 1) return;

        _onPickIndex = onPickIndex;

        for (int i = 0; i < options.Length; i++)
        {
            int posIndex = i;
            optionButtons[i].gameObject.SetActive(true);
            optionButtons[i].onClick.AddListener(() => Pick(posIndex));
            optionLabels[i].text = options[i].steps.ToString();
        }

        PositionToWorldAnchor(worldAnchor.position);

        gameObject.SetActive(true);
        if (overlayButton != null) overlayButton.gameObject.SetActive(true);
    }

    private void PositionToWorldAnchor(Vector3 worldPos)
    {
        if (canvas == null || worldCamera == null || popupRect == null || popupParentRect == null) return;

        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);
        var camForUI = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(popupParentRect, screenPos, camForUI, out var localPoint))
        {
            popupRect.anchoredPosition = localPoint + screenOffset;

            ClampToParent(popupRect, popupParentRect);
        }
    }

    static void ClampToParent(RectTransform child, RectTransform parent)
    {
        var parentRect = parent.rect;
        var size = child.rect.size;

        // Convert pivoted rect extents
        float left   = -size.x * child.pivot.x;
        float right  =  size.x * (1f - child.pivot.x);
        float bottom = -size.y * child.pivot.y;
        float top    =  size.y * (1f - child.pivot.y);

        Vector2 p = child.anchoredPosition;

        p.x = Mathf.Clamp(p.x, parentRect.xMin - left, parentRect.xMax - right);
        p.y = Mathf.Clamp(p.y, parentRect.yMin - bottom, parentRect.yMax - top);

        child.anchoredPosition = p;
    }

    private void Pick(int index)
    {
        Debug.Log($"Option button clicked {index}");
        var temp = _onPickIndex;
        HideImmediate();
        temp?.Invoke(index);
    }

    private void HideImmediate()
    {
        _onPickIndex = null;
        for (int i = 0; i < optionButtons.Length; i++)
        {
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
        if (overlayButton != null) overlayButton.gameObject.SetActive(false);
    }
}
