using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGame : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private Toggle twoPlayersToggle;
    [SerializeField] private Toggle threePlayersToggle;
    [SerializeField] private Toggle fourPlayersToggle;

    private void Start() {
        twoPlayersToggle.isOn = gameSettings.playerCount == 2;
        threePlayersToggle.isOn = gameSettings.playerCount == 3;
        fourPlayersToggle.isOn = gameSettings.playerCount == 4;
    }

    public void OnStartGame()
    {
        soundManager?.PlayUIClick();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnTwoPlayersSelected(bool selected)
    {
        if (selected)
        {
            gameSettings.playerCount = 2;
        }
        soundManager?.PlayUIClick();
    }

    public void OnThreePlayersSelected(bool selected)
    {
        if (selected)
        {
            gameSettings.playerCount = 3;
        }
        soundManager?.PlayUIClick();
    }

    public void OnFourPlayersSelected(bool selected)
    {
        if (selected)
        {
            gameSettings.playerCount = 4;
        }
        soundManager?.PlayUIClick();
    }
}
