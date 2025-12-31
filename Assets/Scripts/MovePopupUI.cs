using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovePopupUI : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;

    [Header("Wiring")]
    [SerializeField] public Canvas canvas;
    [SerializeField] public Camera worldCamera;
    [SerializeField] public SpriteLibrary spriteLibrary;
    [SerializeField] private RectTransform popupRect;
    [SerializeField] private RectTransform popupParentRect;
    [SerializeField] private MoveHighlightLayer moveHighlightLayer;
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TextMeshProUGUI[] optionLabels;
    [SerializeField] private Vector2 screenOffset = new Vector2(0, 130);

    private Button overlayButton;

    private Action<int> _onPickIndex;

    private void SetupOverlayButtonIfNeeded() {
        if (overlayButton == null)
        {
            overlayButton = transform.parent.GetComponent<Button>();
            overlayButton.onClick.RemoveAllListeners();
            overlayButton.onClick.AddListener(HideImmediate);
        }
    }

    private void Start() {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); // refresh layout
    }

    public void Show(Transform worldAnchor, MoveOption[] options, Action<int> onPickIndex)
    {
        if (worldAnchor == null || options == null || options.Length < 1) return;
        SetupOverlayButtonIfNeeded();

        _onPickIndex = onPickIndex;

        for (int i = 0; i < options.Length; i++)
        {
            int posIndex = i;
            optionButtons[i].gameObject.SetActive(true);
            optionButtons[i].onClick.AddListener(() => Pick(posIndex));
            optionLabels[i].text = options[i].steps.ToString();
        }

        PositionToWorldAnchor(worldAnchor.position);

        var playerIndex = options[0].piece.ownerPlayerIndex;
        SetPlayerColor(playerIndex);
        SetRotationByPlayer(playerIndex);

        gameObject.SetActive(true);
        if (overlayButton != null) overlayButton.gameObject.SetActive(true);

        // Set tile highlights
        if (moveHighlightLayer != null && gameSettings.highlightMovesEnabled)
        {
            moveHighlightLayer.ShowHighlights(options);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>()); // refresh layout
    }

    private void SetRotationByPlayer(int playerIndex)
    {
        transform.rotation = new Quaternion(0, 0, 0, 0);
        if (gameSettings.flipRedBlueUI && (playerIndex == 1 || playerIndex == 2))
        {
            transform.Rotate(0, 0, 180, Space.Self);
        }
    }

    private void SetPlayerColor(int playerIndex)
    {
       if (spriteLibrary != null)
        {
            GetComponent<Image>().color = spriteLibrary.playersColorSecondary[playerIndex];
            var buttonsContainer = transform.GetChild(0);
            for (int i = 0; i < buttonsContainer.childCount; i++)
            {
                buttonsContainer.GetChild(i).GetComponent<Image>().color = spriteLibrary.playersColorPrimary[playerIndex];
                buttonsContainer.GetChild(i).GetComponentInChildren<TMP_Text>().color = Color.white;
            }
        }
        else
        {
            Debug.LogWarning($"{name} missing SpriteLibrary assignation");
        }
    }

    private void PositionToWorldAnchor(Vector3 worldPos)
    {
        if (canvas == null || worldCamera == null || popupRect == null || popupParentRect == null) return;

        Vector3 screenPos = worldCamera.WorldToScreenPoint(worldPos);
        var camForUI = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(popupParentRect, screenPos, camForUI, out var localPoint))
        {
            popupRect.anchoredPosition = localPoint + screenOffset;
            float parentHeight = popupParentRect.GetComponent<RectTransform>().rect.height;
            float topOfScreen = parentHeight / 2; // assuming parent is at 0,0 pos and takes over the whole screen.
            bool rightSideOfTheBoard = popupRect.anchoredPosition.x > 0;

            if (popupRect.anchoredPosition.y > topOfScreen || rightSideOfTheBoard)
            {
                // Lets show the popup bellow the piece
                popupRect.anchoredPosition = localPoint + screenOffset * new Vector2(0, -1);
            }

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

        if (moveHighlightLayer != null)
        {
            moveHighlightLayer.Clear();
        }
    }
}
