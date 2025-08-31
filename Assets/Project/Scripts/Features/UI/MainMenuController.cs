using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles actions from the MainMenu: quits the applications and starts the game scene.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    //Name of the scene that contains the game in Unity
    [SerializeField] private string gameSceneName = "GameScene";

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
        Application.Quit();
    }
}
