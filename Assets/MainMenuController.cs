using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "GameScene";

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;          // desactiva VSync (si está activo, ignora targetFrameRate)
        Application.targetFrameRate = 60;        // cap duro a 60 fps
        Time.fixedDeltaTime = 1f / 60f;          // opcional: alinear física a 60 Hz (mejora WheelColliders)
    }

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
