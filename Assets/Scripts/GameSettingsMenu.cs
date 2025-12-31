using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettingsMenu : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle highlightMovesToggle;
    [SerializeField] private Toggle flipBlueAndRedUiToggle;
    [SerializeField] private string mainMenuScene = "MainMenu";

    private Button overlayButton;

    private void SetupOverlayButton() {
        overlayButton = GetComponent<Button>();
        overlayButton.onClick.RemoveAllListeners();
        overlayButton.onClick.AddListener(Hide);
    }

    public void Show()
    {
        Debug.Log("Show Menu");
        if (gameSettings != null)
        {
            soundToggle.isOn = gameSettings.soundEnabled;
            highlightMovesToggle.isOn = gameSettings.highlightMovesEnabled;
            flipBlueAndRedUiToggle.isOn = gameSettings.flipRedBlueUI;
        }

        gameObject.SetActive(true);

        if (overlayButton == null)
        {
            SetupOverlayButton();
        }
    }

    public void Hide()
    {
        Debug.Log("Hide Menu");
        gameObject.SetActive(false);
    }

    public void OnSoundToggled(bool enabled)
    {
        gameSettings.soundEnabled = enabled;
    }

    public void OnHighlightMovesToggled(bool enabled)
    {
        gameSettings.highlightMovesEnabled = enabled;
    }

    public void OnFlipBlueAndRedUiToggled(bool enabled)
    {
        gameSettings.flipRedBlueUI = enabled;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene(mainMenuScene);
    }
}
