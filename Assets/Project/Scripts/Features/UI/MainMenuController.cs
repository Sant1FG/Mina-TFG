using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using System.Collections;

/// <summary>
/// Handles actions from the MainMenu: quits the applications starts the game scene
/// and manages localization buttons.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    //Name of the scene that contains the game in Unity
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject scoreButton;
    [SerializeField] private GameObject quitButton;
    [SerializeField] private GameObject leaderboardCanvas;
    [SerializeField] private GameObject leaderboardButton;
    [SerializeField] private LeaderboardManager leaderboardManager;
    private bool localeGuard = false;

    private void Start()
    {
        int ID = PlayerPrefs.GetInt("LocaleKey", 0);
        leaderboardCanvas.SetActive(false);
        ChangeLocale(ID);
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
        PlayerPrefs.SetInt("LocaleKey", 0);
        Application.Quit();
    }

    public void OnLeaderboardClicked()
    {
        playButton.SetActive(false);
        quitButton.SetActive(false);
        scoreButton.SetActive(false);
        leaderboardCanvas.SetActive(true);
        leaderboardButton.SetActive(true);
        leaderboardManager.PopulateLeaderboard();
    }

    public void OnLeaderboardClosed()
    {
        playButton.SetActive(true);
        scoreButton.SetActive(true);
        quitButton.SetActive(true);
        leaderboardCanvas.SetActive(false);
        leaderboardButton.SetActive(false);
        leaderboardManager.ClearLeaderboardEntryTransform();
    }

    public void ChangeLocale(int localeID)
    {
        if (localeGuard) return;
        StartCoroutine(SetLocale(localeID));
    }

    IEnumerator SetLocale(int _localeID)
    { 
        yield return LocalizationSettings.InitializationOperation;
        localeGuard = true;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[_localeID];
        PlayerPrefs.SetInt("LocaleKey", _localeID);
        localeGuard = false;
        
    } 


}
