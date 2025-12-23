using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene";
    [SerializeField] private GameSettings gameSettings;

    public void OnStartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnTwoPlayersSelected(bool selected)
    {
        if (selected)
        {
            gameSettings.playerCount = 2;
        }
    }

    public void OnThreePlayersSelected(bool selected)
    {
        if (selected)
        {
            gameSettings.playerCount = 3;
        }
    }

    public void OnFourPlayersSelected(bool selected)
    {
        if (selected)
        {
            gameSettings.playerCount = 4;
        }
    }
}
