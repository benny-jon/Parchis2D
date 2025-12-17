using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParchisUI : MonoBehaviour
{
    [Header("Orientation Roots")]
    [SerializeField] private GameObject portraitRoot;
    [SerializeField] private GameObject landscapeRoot;

    [Header("Per Player HUDs")]
    [SerializeField] private PlayerHud[] playerHuds;

    [Header("Medals Sprite")]
    [SerializeField] private Sprite gold;
    [SerializeField] private Sprite silver;
    [SerializeField] private Sprite bronze;

    [Header("Notifications")]
    [SerializeField] private TimedMessageUI eventNotification;

    [Header("End Game")]
    [SerializeField] public TMP_Text gameOverText;
    [SerializeField] private WinCelebration winCelebration;

    private bool wasLastPortrait;

    private void Awake()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        ApplyOrientation();
    }

    private void Update()
    {
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
        if (eventNotification != null)
        {
            eventNotification.Hide();

        }
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

    public void ShowNotification(String message)
    {
        if (eventNotification != null)
        {
            eventNotification.Show(message, 2);

        }
    }

    public void ShowMedal(int player, Medal medal)
    {
        if (playerHuds == null || player < 0 || player >= playerHuds.Length) return;

        var hud = playerHuds[player];
        if (hud.portraitMedal != null)
        {
            hud.portraitMedal.sprite = GetMedalSprite(medal);
            hud.portraitMedal.gameObject.SetActive(wasLastPortrait);
        }
        if (hud.landscapeMedal != null)
        {
            hud.landscapeMedal.sprite = GetMedalSprite(medal);
            hud.landscapeMedal.gameObject.SetActive(!wasLastPortrait);
        }
    }

    private Sprite GetMedalSprite(Medal medal)
    {
        switch (medal)
        {
            case Medal.Gold: return gold;
            case Medal.Silver: return silver;
            case Medal.Bronze: return bronze;
            default: return null;
        }
    }

    [System.Serializable]
    public class PlayerHud
    {
        [Header("Portrait")]
        public TextMeshProUGUI portraitDiceText;
        public TextMeshProUGUI portraitTurnHint;
        public Image portraitMedal;

        [Header("Landscape")]
        public TextMeshProUGUI landscapeDiceText;
        public TextMeshProUGUI landscapeTurnHint;
        public Image landscapeMedal;
    }
}
