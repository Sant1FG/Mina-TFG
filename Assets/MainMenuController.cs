using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles actions from the MainMenu: quits the applications and starts the game scene.
/// Also configures global values on start (VSync off, 60 fps cap and interval at which physics updates)
/// </summary>
public class MainMenuController : MonoBehaviour
{
    //Name of the scene that contains the game in Unity
    [SerializeField] private string gameSceneName = "GameScene";

    /// <summary>
    /// On load configures the following global timings: Disables VSync, caps frame rate to 60 FPS and
    /// sets the physics fixed timestep to 1/60 (60 updates per second).
    /// </summary>
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
        Time.fixedDeltaTime = 1f / 60f;
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
        Application.Quit();
    }
}
