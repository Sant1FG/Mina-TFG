using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using System.Collections;

/// <summary>
/// Handles actions from the MainMenu: quits the applications starts the game scene,
///manages localization buttons, main menu music and sfx, and credits screen.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    //Name of the scene that contains the game in Unity
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject scoreButton;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject creditsButton;
    [SerializeField] private GameObject leaderboardCanvas;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private GameObject leaderboardButton;
    [SerializeField] private LeaderboardManager leaderboardManager;
    [SerializeField] private AudioManager audioManager;

    private bool localeGuard = false;

    /// <summary>
    /// On Script load changes localization to galician.
    /// </summary>
    private void Awake()
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[PlayerPrefs.GetInt("LocaleKey")];
    }

    /// <summary>
    /// On Starts closes all canvas except the main menu.
    /// </summary>
    private void Start()
    {
        leaderboardCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
    }

    /// <summary>
    /// UI callback for the play button.
    /// Loads the game scene in Single Mode.
    /// </summary>
    public void OnPlayClicked()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("MainMenuController: Name not configured.");
            return;
        }
        audioManager.PlayButtonSFX();
        //LoadSceneMode.Single destroys current scene when loading a new one
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    /// <summary>
    /// UI callback for the quit button.
    /// Closes the application.
    /// </summary>
    public void OnQuitClicked()
    {
        Debug.Log("MainMenuController: Closing Application");
        audioManager.PlayButtonSFX();
        PlayerPrefs.SetInt("LocaleKey", 0);
        Application.Quit();
    }

    /// <summary>
    /// UI callback for the leaderboard button.
    /// Opens the leaderboard, hiding all unrelated buttons.
    /// </summary>
    public void OnLeaderboardClicked()
    {
        audioManager.PlayButtonSFX();
        playButton.SetActive(false);
        quitButton.SetActive(false);
        scoreButton.SetActive(false);
        creditsButton.SetActive(false);
        leaderboardCanvas.SetActive(true);
        leaderboardButton.SetActive(true);
        leaderboardManager.PopulateLeaderboard();
    }

    /// <summary>
    /// UI callback for the close leaderboard button.
    /// Closes the leaderboard, restoring the previously disabled buttons.
    /// </summary>
    public void OnLeaderboardClosed()
    {
        audioManager.PlayButtonSFX();
        playButton.SetActive(true);
        scoreButton.SetActive(true);
        quitButton.SetActive(true);
        creditsButton.SetActive(true);
        leaderboardCanvas.SetActive(false);
        leaderboardButton.SetActive(false);
        leaderboardManager.ClearLeaderboardEntryTransform();
    }

    /// <summary>
    /// UI callback for the localization buttons, starts change locale corroutine.
    /// </summary>
    /// <param name="localeID">Selected locale id</param>
    public void ChangeLocale(int localeID)
    {
        if (localeGuard) return;
        StartCoroutine(SetLocale(localeID));
    }

    /// <summary>
    /// Switches the application locale.
    /// </summary>
    /// <param name="_localeID">Selected locale id</param>
    IEnumerator SetLocale(int _localeID)
    {
        yield return LocalizationSettings.InitializationOperation;
        audioManager.PlayButtonSFX();
        localeGuard = true;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
        PlayerPrefs.SetInt("LocaleKey", _localeID);
        localeGuard = false;

    }

    /// <summary>
    /// UI callback for the credits button.
    /// Opens the credits screen, closing the main menu canvas.
    /// </summary>
    public void OnCreditsClicked()
    {
        mainMenuCanvas.SetActive(false);
        leaderboardCanvas.SetActive(false);
        creditsCanvas.SetActive(true);
        audioManager.SwitchGameOverMusic();
    }

    /// <summary>
    /// UI callback for the close credits button.
    /// Closes the credits screen, reopening the main menu canvas.
    /// </summary>
    public void OnCreditsClosed()
    {
        creditsCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
        audioManager.SwitchMainMenuMusic();
    }

    /// <summary>
    /// Sent by Unity to all GameObject on application quit.
    /// Changes locale to default (galician)
    /// </summary>
     public void OnApplicationQuit()
    {
        PlayerPrefs.SetInt("LocaleKey", 0);
    }
}
