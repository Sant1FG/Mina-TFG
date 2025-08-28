using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMenuController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject hudButtonsCanvas;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private string menuSceneName = "MenuScene";


    void Start()
    {
        ShowTutorial();
    }

    public void ShowTutorial()
    {
        tutorialCanvas.SetActive(true);
    }

    public void ShowHUD()
    {
        hudCanvas.SetActive(true);
    }
    public void ShowPause()
    {
        pauseCanvas.SetActive(true);
    }

    public void ShowGameOver()
    {
        gameOverCanvas.SetActive(true);
    }

    public void ShowHUDButtons()
    {
        hudButtonsCanvas.SetActive(true);
    }

    public void HideTutorial()
    {
        tutorialCanvas.SetActive(false);
    }

    public void HidePause()
    {
        pauseCanvas.SetActive(false);
    }

    public void HideHUDButtons()
    {
        hudButtonsCanvas.SetActive(false);
    }
    public void HideHUD()
    {
        hudCanvas.SetActive(false);
    }

    public void HideGameOver()
    {
        gameOverCanvas.SetActive(false);
    }

    public void OnCloseTutorial()
    {
        gameManager.StartSession();
    }

    public void OnPauseClicked()
    {
        gameManager.PauseSession();
    }

    public void OnResumeClicked()
    {
        gameManager.ResumeSession();
    }

    public void OnRestartClicked()
    {
        gameManager.RestartSession();
    }

    public void OnMainMenuClicked()
    {
        SceneManager.LoadScene(menuSceneName, LoadSceneMode.Single);
    }


    


}
