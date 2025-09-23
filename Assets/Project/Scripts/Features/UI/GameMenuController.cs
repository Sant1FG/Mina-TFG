using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles all in-game UI menus and panels: shows/hides tutorial, HUD, Pause, Game Over and HUD buttons.
/// Exposes UI callbacks to handle navigation in the menus.
/// </summary>
public class GameMenuController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject hudButtonsCanvas;
    [SerializeField] private GameObject leaderboardInputCanvas;
    [SerializeField] private TextMeshProUGUI inputName;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private LeaderboardManager leaderboardManager;
    //Name of the scene that contains the Main menu
    [SerializeField] private string menuSceneName = "MenuScene";

    /// <summary>
    /// Initializes the UI. Shows Tutorial screen by default.
    /// </summary>
    void Start()
    {
        ShowTutorial();
    }

    /// <summary>
    /// Shows the Tutorial screen.
    /// </summary>
    public void ShowTutorial()
    {
        tutorialCanvas.SetActive(true);
    }

    /// <summary>
    /// Shows the HUD panel.
    /// </summary>
    public void ShowHUD()
    {
        hudCanvas.SetActive(true);
    }

    /// <summary>
    /// Shows the Pause screen.
    /// </summary>
    public void ShowPause()
    {
        pauseCanvas.SetActive(true);
    }

    /// <summary>
    /// Shows the Game Over screen.
    /// </summary>
    public void ShowGameOver()
    {
        gameOverCanvas.SetActive(true);
    }

    /// <summary>
    /// Shows the HUDButtons panel, for now only Pause.
    /// </summary>
    public void ShowHUDButtons()
    {
        hudButtonsCanvas.SetActive(true);
    }

    /// <summary>
    /// Hides the Tutorial screen.
    /// </summary>
    public void HideTutorial()
    {
        tutorialCanvas.SetActive(false);
    }

    /// <summary>
    /// Hides the Pause screen.
    /// </summary>
    public void HidePause()
    {
        pauseCanvas.SetActive(false);
    }

    /// <summary>
    /// Hides the HUDButtons panel.
    /// </summary>
    public void HideHUDButtons()
    {
        hudButtonsCanvas.SetActive(false);
    }

    /// <summary>
    /// Hides the HUD panel.
    /// </summary>
    public void HideHUD()
    {
        hudCanvas.SetActive(false);
    }

    /// <summary>
    /// Hides the Game Over screen.
    /// </summary>
    public void HideGameOver()
    {
        gameOverCanvas.SetActive(false);
    }

    public void ShowLeaderboardInput()
    {
        leaderboardInputCanvas.SetActive(true);
    }

    public void HideLeaderboardInput()
    {
        leaderboardInputCanvas.SetActive(false);
    }

    /// <summary>
    /// Changes the UI state between pause and gameplay.
    /// </summary>
    /// <param name="paused">True to show pause screen. False to hide it.</param>
    public void ApplyPausedUI(bool paused)
    {
        if (paused)
        {
            HideHUDButtons();
            ShowPause();
        }
        else
        {
            ShowHUDButtons();
            HidePause();
        }
    }

    /// <summary>
    /// Show the gameplay UI.
    /// Hides the Tutorial, Pause and GameOver screens and activates the HUD and HUDButtons.
    /// </summary>
    public void ShowGameplayUI()
    {
        //Hide other screens
        HideTutorial();
        HidePause();
        HideGameOver();
        //Activate and update initial HUD
        ShowHUD();
        ShowHUDButtons();
    }

    /// <summary>
    /// Shows the GameOver UI.
    /// Hides the Pause, HUD Buttons, HUD and shows the Game Over screen.
    /// </summary>
    public void ShowGameOverUI()
    {
        HidePause();
        HideHUDButtons();
        HideHUD();
        leaderboardManager.PopulateLeaderboard();
        ShowGameOver();
    }

    

    /// <summary>
    /// UI callback for the Continue button on the Tutorial screen.
    /// Starts a new game session.
    /// </summary>
    public void OnCloseTutorial()
    {
        gameManager.StartSession();
    }
    
    /// <summary>
    /// UI callback for the Pause button on the HUDButtons panel.
    /// Pauses the current session.
    /// </summary>
    public void OnPauseClicked()
    {
        gameManager.PauseSession();
    }

    /// <summary>
    /// UI callback for the Resume button on the Pause screen.
    /// Unpauses and resumes the current session.
    /// </summary>
    public void OnResumeClicked()
    {
        gameManager.ResumeSession();
    }

    /// <summary>
    /// UI callback for the Restart button on the Pause and Game Over screens.
    /// Restarts the current session.
    /// </summary>
    public void OnRestartClicked()
    {
        gameManager.RestartSession();
    }

    public void OnLeaderboardInputClicked()
    {
        string playerName = Regex.Replace(inputName.text, @"\p{C}+", "");
        playerName = playerName.Trim();
        if (string.IsNullOrWhiteSpace(playerName)) playerName = "ANON";
        gameManager.AddLeaderboardEntry(playerName);
        leaderboardManager.ClearLeaderboardEntryTransform();
        leaderboardManager.PopulateLeaderboard();
        HideLeaderboardInput();

    }

    /// <summary>
    /// UI callback for the Main menu and Quit button on the Pause and Game Over screens.
    /// Loads the main menu scene, destroying the current one.
    /// </summary>
    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }
}
