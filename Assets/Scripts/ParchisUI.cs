using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ParchisUI : MonoBehaviour
{
    [SerializeField] private SpriteLibrary spriteLibrary;
    [SerializeField] private SoundManager soundManager;

    [Header("Orientation Roots")]
    [SerializeField] private GameObject portraitRoot;
    [SerializeField] private GameObject landscapeRoot;

    [Header("Settings")]
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private GameSettingsMenu gameSettingsMenu;

    [Header("Per Player HUDs")]
    [SerializeField] private PlayerHud[] playerHuds;

    [Header("Notifications")]
    [SerializeField] private TimedMessageUI eventNotification;

    [Header("End Game")]
    [SerializeField] public TMP_Text gameOverText;
    [SerializeField] private WinCelebration winCelebration;

    public Action<int> OnPlayerDiceClicked;

    private bool wasLastPortrait;

    private void Awake()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        CheckPlayersPlaying();
        SubscribeToPlayersDicePanelClicks();
    }

    private void Start()
    {
        ApplyOrientation();
    }

    private void CheckPlayersPlaying()
    {
        if (gameSettings.playerCount == 2)
        {
            playerHuds[1].portraitDicePanel.gameObject.SetActive(false);
            playerHuds[1].landscapeDicePanel.gameObject.SetActive(false);
            playerHuds[3].portraitDicePanel.gameObject.SetActive(false);
            playerHuds[3].landscapeDicePanel.gameObject.SetActive(false);
        }
        else if (gameSettings.playerCount == 3)
        {
            playerHuds[3].portraitDicePanel.gameObject.SetActive(false);
            playerHuds[3].landscapeDicePanel.gameObject.SetActive(false);
        }
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

        hud.landscapeDicePanel?.SetDice(d1, d2);
        hud.portraitDicePanel?.SetDice(d1, d2);
    }

    public void ClearOtherPlayersDice(int currentPlayerIndex)
    {
        if (playerHuds == null || currentPlayerIndex < 0 || currentPlayerIndex >= playerHuds.Length) return;

        for (int i = 0; i < playerHuds.Length; i++)
        {
            var hud = playerHuds[i];
            hud.landscapeDicePanel?.SetDim(i != currentPlayerIndex);
            hud.portraitDicePanel?.SetDim(i != currentPlayerIndex);

            if (i != currentPlayerIndex)
            {
                hud.landscapeDicePanel?.SetTimeToRoll(false);
                hud.portraitDicePanel?.SetTimeToRoll(false);
            }
        }
    }

    public void ShowTurnHintForPlayer(int playerIndex, GamePhase gamePhase)
    {
        if (playerHuds == null || playerIndex < 0 || playerIndex >= playerHuds.Length) return;

        var hud = playerHuds[playerIndex];
        hud.landscapeDicePanel?.SetDim(false);
        hud.portraitDicePanel?.SetDim(false);
        hud.landscapeDicePanel?.SetTimeToRoll(gamePhase == GamePhase.WaitingForRoll);
        hud.portraitDicePanel?.SetTimeToRoll(gamePhase == GamePhase.WaitingForRoll);
    }

    public void ClearTurnHints()
    {
        if (playerHuds == null) return;

        for (int i = 0; i < playerHuds.Length; i++)
        {
            var hud = playerHuds[i];
            hud.landscapeDicePanel?.SetDim(true);
            hud.portraitDicePanel?.SetDim(true);
        }
    }

    public void ShowGameOver(string message, int firstPlacePlayerIndex)
    {
        if (eventNotification != null)
        {
            eventNotification.Hide();

        }
        if (gameOverText != null)
        {
            gameOverText.text = message;
            gameOverText.gameObject.SetActive(true);

            gameOverText.transform.rotation = new Quaternion(0, 0, 0, 0);
            if (gameSettings.flipRedBlueUI && (firstPlacePlayerIndex == 1 || firstPlacePlayerIndex == 2))
            {
                gameOverText.transform.Rotate(0, 0, 180, Space.Self);
            }
        }
        if (winCelebration != null)
        {
            winCelebration.Play(new Vector3(0, 0, 1));
        }
    }

    public void ShowNotification(String message, int playerIndex)
    {
        if (eventNotification != null)
        {
            eventNotification.Show(message, 3);
            eventNotification.transform.rotation = new Quaternion(0, 0, 0, 0);
            if (gameSettings.flipRedBlueUI && (playerIndex == 1 || playerIndex == 2))
            {
                eventNotification.transform.Rotate(0, 0, 180, Space.Self);
            }
        }
    }

    public void ShowMedal(int player, Medal medal)
    {
        if (playerHuds == null || player < 0 || player >= playerHuds.Length) return;

        var hud = playerHuds[player];
        if (hud.portraitMedal != null)
        {
            hud.portraitMedal.sprite = spriteLibrary.GetMedalSprite(medal);
            hud.portraitMedal.gameObject.SetActive(true);
        }
        if (hud.landscapeMedal != null)
        {
            hud.landscapeMedal.sprite = spriteLibrary.GetMedalSprite(medal);
            hud.landscapeMedal.gameObject.SetActive(true);
        }
    }

    public void ShowSettingsMenu()
    {
        if (gameSettingsMenu != null)
        {
            gameSettingsMenu.Show();
            soundManager?.PlayUIClick();
        }
        else
        {
            Debug.LogWarning("ParchisUI: GameSettingMenu not assigned");
        }
    }

    private void SubscribeToPlayersDicePanelClicks()
    {
        Debug.Log("Parchis UI, SubscribeToPlayersDicePanelClicks");
        for (int i = 0; i < playerHuds.Length; i++)
        {
            Debug.Log("Subscribing for player " + i);
            int player = i;
            if (playerHuds[player].portraitDicePanel != null)
            {
                playerHuds[player].portraitDicePanel.OnDiceClicked += () =>
                {
                    Debug.Log($"OnDice clicked for player {player}");
                    OnPlayerDiceClicked?.Invoke(player);
                };
            }
            if (playerHuds[player].landscapeDicePanel != null)
            {
                playerHuds[player].landscapeDicePanel.OnDiceClicked += () =>
                {
                    Debug.Log($"OnDice clicked for player {player}");
                    OnPlayerDiceClicked?.Invoke(player);
                };
            }
        }
    }

    [System.Serializable]
    public class PlayerHud
    {
        [Header("Portrait")]
        public PlayerDicePanel portraitDicePanel;
        public Image portraitMedal;

        [Header("Landscape")]
        public PlayerDicePanel landscapeDicePanel;
        public Image landscapeMedal;
    }
}
