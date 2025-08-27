using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    public void onPlayClicked()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("MainMenuController: Name not configured.");
            return;
        }

        //Closes current scene when opening a new one
        SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
    }

    public void onQuitClicked()
    {
        Debug.Log("Closing Application");
        Application.Quit();
    }
}
