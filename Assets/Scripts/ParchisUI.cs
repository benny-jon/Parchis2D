using System;
using TMPro;
using UnityEngine;

public class ParchisUI : MonoBehaviour
{
    [Header("Orientation Roots")]
    [SerializeField] private GameObject portraitRoot;
    [SerializeField] private GameObject landscapeRoot;

    [Header("Per Player HUDs")]
    [SerializeField] private PlayerHud[] playerHuds;

    [Header("End Game")]
    [SerializeField] public TMP_Text gameOverText;
    [SerializeField] private WinCelebration winCelebration;

    private bool wasLastPortrait;

    private void Awake() {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    private void Start() {
        ApplyOrientation();
    }

    private void Update() {
        bool isPortrait = Screen.height > Screen.width;
        if (isPortrait != wasLastPortrait)
        {
            ApplyOrientation();
        }
    }

    private void ApplyOrientation()
    {
        bool isPortrait = Screen.height >= Screen.width;
        wasLastPortrait = isPortrait;

        if (portraitRoot != null) portraitRoot.SetActive(isPortrait);
        if (landscapeRoot != null) landscapeRoot.SetActive(!isPortrait);
    }

    public void ShowDiceForPlayer(int playerIndex, int d1, int d2)
    {
        if (playerHuds == null || playerIndex < 0 || playerIndex >= playerHuds.Length) return;

        String roll = $"{d1},{d2}";
        var hud = playerHuds[playerIndex];

        if (hud.landscapeDiceText != null) hud.landscapeDiceText.text = roll;
        if (hud.portraitDiceText != null) hud.portraitDiceText.text = roll;
    }

    public void ClearOtherPlayersDice(int currentPlayerIndex)
    {
        if (playerHuds == null || currentPlayerIndex < 0 || currentPlayerIndex >= playerHuds.Length) return;

        for (int i = 0; i < playerHuds.Length; i++)
        {
            if (i == currentPlayerIndex) continue;

            var hud = playerHuds[i];
            if (hud.landscapeDiceText != null) hud.landscapeDiceText.text = "";
            if (hud.portraitDiceText != null) hud.portraitDiceText.text = "";
        }
    }

    public void ShowTurnHintForPlayer(int playerIndex, GamePhase gamePhase)
    {
        if (playerHuds == null || playerIndex < 0 || playerIndex >= playerHuds.Length) return;

        String hint = gamePhase == GamePhase.WaitingForRoll ? "Roll" : gamePhase == GamePhase.WaitingForMove ? "Move" : "";

        var hud = playerHuds[playerIndex];
        if (hud.landscapeTurnHint != null) hud.landscapeTurnHint.text = hint;
        if (hud.portraitTurnHint != null) hud.portraitTurnHint.text = hint;
    }

    public void ClearTurnHints()
    {
        if (playerHuds == null) return;

        for (int i = 0; i < playerHuds.Length; i++)
        {
            var hud = playerHuds[i];
            if (hud.landscapeTurnHint != null) hud.landscapeTurnHint.text = "";
            if (hud.portraitTurnHint != null) hud.portraitTurnHint.text = "";
        }
    }

    public void ShowGameOver(int playerWinner)
    {
        if (gameOverText != null)
        {
            Debug.Log($"Player {playerWinner}\nhas Won!");
            gameOverText.text = $"Player {playerWinner}\nhas Won!";
            gameOverText.gameObject.SetActive(true);
        }
        if (winCelebration != null)
        {
            winCelebration.Play(gameOverText.transform.position);
        }
    }

    [System.Serializable]
    public class PlayerHud
    {
        [Header("Portrait")]
        public TextMeshProUGUI portraitDiceText;
        public TextMeshProUGUI portraitTurnHint;

        [Header("Landscape")]
        public TextMeshProUGUI landscapeDiceText;
        public TextMeshProUGUI landscapeTurnHint;
    }
}
