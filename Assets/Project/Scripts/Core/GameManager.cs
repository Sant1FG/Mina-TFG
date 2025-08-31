using UnityEngine;

/// <summary>
/// Coordinates the game session lifecycle: Start, pause/resume, end and restart.
/// Initializes and distributes configuration and state to other controllers and handles 
/// cross-cutting events (coal collect/deposit affect timer state and hud).
/// </summary>
public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private HUDController hud;
    [SerializeField] private TimerController timer;
    [SerializeField] private InteractionController interaction;
    [SerializeField] private ExcavatorController excavator;
    [SerializeField] private GameMenuController gameMenu;
    [SerializeField] private ObstacleSpawner obstacleSpawner;
    [SerializeField] private VeinSpawner veinSpawner;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private KeyCode respawnKey = KeyCode.T;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform cameraRespawnTransform;
    public SessionState state;

    /// <summary>
    /// On Script load validates if there is any missing reference.
    /// </summary>
    private void Awake()
    {
        if (!config || !hud || !timer || !interaction || !excavator || !gameMenu ||
            !obstacleSpawner || !veinSpawner || !cameraController || !playerSpawn || !cameraRespawnTransform)
        {
            Debug.LogError("GameManager: Missing references.", this);
        }
    }

    /// <summary>
    /// Subscribes to the following external events:
    /// - Time-up from TimerController.
    /// - Coal Collection and deposit events from InteractionController.
    /// </summary>
    private void OnEnable()
    {
        if (timer != null) timer.OnTimeUp += EndSession;
        if (interaction != null)
        {
            interaction.OnCollectCoal += HandleCollectCoal;
            interaction.OnDepositCoal += HandleDepositCoal;
        }
    }

    /// <summary>
    /// Unsubscribes from external events
    /// </summary>
    private void OnDisable()
    {
        if (timer != null) timer.OnTimeUp -= EndSession;
        if (interaction != null)
        {
            interaction.OnCollectCoal -= HandleCollectCoal;
            interaction.OnDepositCoal -= HandleDepositCoal;
        }
    }

    /// <summary>
    /// Called by Unity once per frame. Polls the respawn key while the game is running.
    /// Relocates the player to the spawn position.
    /// </summary>
    private void Update()
    {
        if (state.isRunning && Input.GetKeyDown(respawnKey))
        {
            if (excavator != null)
            {
                RespawnPlayer();
            }
        }
    }

    /// <summary>
    /// Starts a gameplay session: 
    /// - Cleans the game State and distributes it to the controllers with the GameConfig.
    /// - Resets the obstacle and vein spawners, positions the camera and players enabling the controls.
    /// - Prepares the HUD and UI and finally starts the countdown timer.
    /// </summary>
    public void StartSession()
    {
        Time.timeScale = 1f;
        state = new SessionState
        {
            // Initialize according to config
            score = 0,
            coalInDepot = 0,
            isRunning = true
        };

        //Camera follows player
        cameraController.SetPlayer(excavator.transform);

        // Provide state/config to controllers
        interaction.SetSessionState(state);
        interaction.SetConfig(config);

        obstacleSpawner.ResetObstacleSpawner();
        obstacleSpawner.ResumeSpawning();
        veinSpawner.ResetVeinSpawner();

        //Position player and enable controls
        RespawnPlayer();
        excavator.EnableControls();

        //Update inicial hud
        hud.SetScoreText(state.score);
        hud.SetCoalText(state.coalInDepot);
        hud.SetDepositMaxText(config.depositMax);
        hud.HideInteractionText();
        hud.ClearNotificationsToast();

        //Show Gameplay UI and start countdown
        gameMenu.ShowGameplayUI();
        timer.StartTimer(config.initialTimeSeconds);

    }

    /// <summary>
    /// Pauses the current gameplay session:
    /// - Stops the countdown timer.
    /// - Disables obstacle spawning and player controls.
    /// - Clears notifications, updates the HUD and shows the Paused UI.
    /// </summary>
    public void PauseSession()
    {
        //Scale = 0 freezes all physics updates and couroutines
        Time.timeScale = 0f;
        timer.Pause();
        obstacleSpawner.StopSpawning();
        excavator.DisableControls();
        hud.HideInteractionText();
        hud.ClearNotificationsToast();
        gameMenu.ApplyPausedUI(true);
    }

    /// <summary>
    /// Resumes the current paused gameplay session:
    /// -Resumes the countdown timer.
    /// -Reenables obstacle spawning and player controls.
    /// -Hides the Paused UI.
    /// </summary>
    public void ResumeSession()
    {
        Time.timeScale = 1f;
        timer.Resume();
        obstacleSpawner.ResumeSpawning();
        excavator.EnableControls();
        gameMenu.ApplyPausedUI(false);
    }

    /// <summary>
    /// Ends the current gameplay session when countdown timer reaches zero:
    /// -Stops the countdown timer and all physics updates.
    /// -Disables obstacle spawning and player controls.
    /// -Updates HUD and shows the Game Over UI.
    /// </summary>
    public void EndSession()
    {
        if (!state.isRunning) return;
        Debug.Log("GameManager: Time is 0. GAME OVER");
        Time.timeScale = 0f;
        state.isRunning = false;
        timer.Pause();
        excavator.DisableControls();
        obstacleSpawner.StopSpawning();
        hud.HideInteractionText();
        hud.ClearNotificationsToast();
        hud.SetFinalScoreText(state.score);
        gameMenu.ShowGameOverUI();
    }

    /// <summary>
    /// Restarts the current session by resetting the UI and delegating to StartSession.
    /// </summary>
    public void RestartSession()
    {
        gameMenu.HidePause();
        gameMenu.HideGameOver();
        StartSession();
    }

    /// <summary>
    /// Repositions the player and camera to the spawn points.
    /// Clears the vehicle deposit, updates the UI and shows the respawn notification.
    /// </summary>
    private void RespawnPlayer()
    {
        Debug.Log("GameManager: Respawning player");
        excavator.Reposition(playerSpawn.position, playerSpawn.rotation);
        cameraController.SetFixedPose(cameraRespawnTransform, excavator.transform);
        state.coalInDepot = 0;
        hud.SetCoalText(0);

        hud.ShowNotificationToast("Vehículo recolocado. Depósito vaciado", 2f);
    }

    /// <summary>
    /// Handles the coal collection event.
    /// Increments the count on the deposit and updates the HUD.
    /// </summary>
    private void HandleCollectCoal()
    {
        state.coalInDepot++;
        hud.SetCoalText(state.coalInDepot);
    }

    /// <summary>
    /// Handles the coal deposit event.
    /// Adds time and score based on the config, clears deposit and updates the HUD.
    /// </summary>
    private void HandleDepositCoal()
    {
        timer.AddTime(state.coalInDepot * config.timePerCoalUnit);
        state.score += state.coalInDepot * config.pointsPerCoalUnit;
        state.coalInDepot = 0;
        hud.SetCoalText(state.coalInDepot);
        hud.SetScoreText(state.score);
    }


}
