using UnityEngine;

public class GameMenuController : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject hudCanvas;
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject gameOverCanvas;
    [SerializeField] private GameObject optionsCanvas;
    [SerializeField] private GameManager gameManager;

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

    public void showGameOver()
    {
        gameOverCanvas.SetActive(true);
    }

    public void showOptions()
    {
        optionsCanvas.SetActive(true);
    }

    public void hideTutorial()
    {
        tutorialCanvas.SetActive(false);
    }

    public void hidePause()
    {
        pauseCanvas.SetActive(false);
    }

    public void hideOptions()
    {
        pauseCanvas.SetActive(false);
    }
    public void hideHUD()
    {
        hudCanvas.SetActive(false);
    }

    public void onCloseTutorial()
    {
        gameManager.StartSession();
    }


    


}
