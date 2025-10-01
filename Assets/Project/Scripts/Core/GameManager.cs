using UnityEngine;
using UnityEngine.Localization.Settings;

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
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private LeaderboardManager leaderboardManager;
    //[SerializeField] private KeyCode respawnKey = KeyCode.T;
    //[SerializeField] private KeyCode escapeKey = KeyCode.Escape;
   // [SerializeField] private KeyCode hornKey = KeyCode.H;
    [SerializeField] private Transform playerSpawn;
    [SerializeField] private Transform cameraRespawnTransform;

    private bool gamePaused = false;

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
    /// - Time-up and Low-time from TimerController.
    /// - Coal Collection and deposit events from InteractionController.
    /// - Obstacle spawning from ObstacleSpawner.
    /// </summary>
    private void OnEnable()
    {
        if (timer != null)
        {
            timer.OnTimeUp += EndSession;
            timer.OnLowTime += PlayLowTimeMusic;
        }
        if (interaction != null)
        {
            interaction.OnCollectCoal += HandleCollectCoal;
            interaction.OnDepositCoal += HandleDepositCoal;
        }
        if (obstacleSpawner != null)
        {
            obstacleSpawner.OnObstacleSFX += HandleObstacleSpawnSFX;
        }

    }

    /// <summary>
    /// Unsubscribes from external events
    /// </summary>
    private void OnDisable()
    {
        if (timer != null)
        {
            timer.OnTimeUp -= EndSession;
            timer.OnLowTime -= PlayLowTimeMusic;
        }

        if (interaction != null)
        {
            interaction.OnCollectCoal -= HandleCollectCoal;
            interaction.OnDepositCoal -= HandleDepositCoal;
        }
        if (obstacleSpawner != null)
        {
            obstacleSpawner.OnObstacleSFX -= HandleObstacleSpawnSFX;
        }
    }

    /// <summary>
    /// Called by Unity once per frame. 
    /// Polls the respawn key to reposition the player.  
    /// Poll the horn key to play the horn sound effect.
    /// Polls the ESC key to display the pause menu.
    /// </summary>
/*     private void Update()
    {
         if (state.isRunning && Input.GetKeyDown(respawnKey))
        {
            if (excavator != null)
            {
                audioManager.PlayRespawnSFX();
                RespawnPlayer();
            }
        } 

        if (state.isRunning && Input.GetKeyDown(hornKey))
        {
            if (excavator != null)
            {
                audioManager.PlayHornSFX();
            }
        }

        if (state.isRunning && Input.GetKeyDown(escapeKey))
        {
            if (!gamePaused)
            {
                PauseSession();
            }
            else
            {
                ResumeSession();
            }
            audioManager.PlayButtonSFX();
        }
    } */

    /// <summary>
    /// Starts a gameplay session: 
    /// - Cleans the game State and distributes it to the controllers with the GameConfig.
    /// - Resets the obstacle and vein spawners, positions the camera and players enabling the controls.
    /// - Prepares the HUD, UI, SFXs and finally starts the countdown timer.
    /// </summary>
    public void StartSession()
    {
        Time.timeScale = 1f;
        state = new SessionState
        {
            // Initialize according to config
            score = 0,
            coalInDepot = 0,
            isRunning = false
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
        excavator.EnableEngineSound();

        //Update inicial hud
        hud.SetScoreText(state.score);
        hud.SetCoalText(state.coalInDepot);
        hud.SetDepositMaxText(config.depositMax);
        hud.HideInteractionText();
        hud.ClearNotificationsToast();

        //Show Gameplay UI and start countdown
        gameMenu.ShowGameplayUI();
        audioManager.PlayVehicleStartSFX();
        state.isRunning = true;
        timer.StartTimer(config.initialTimeSeconds);

    }

    /// <summary>
    /// Pauses the current gameplay session:
    /// - Stops the countdown timer.
    /// - Disables obstacle spawning and player controls.
    /// - Clears notifications, updates the HUD, stops music, and shows the Paused UI.
    /// </summary>
    public void PauseSession()
    {
        //Scale = 0 freezes all physics updates and couroutines
        Time.timeScale = 0f;
        gamePaused = true;
        timer.Pause();
        obstacleSpawner.StopSpawning();
        excavator.DisableControls();
        excavator.DisableEngineSound();
        audioManager.PauseBackgroundMusic();
        hud.HideInteractionText();
        hud.ClearNotificationsToast();
        gameMenu.ApplyPausedUI(true);
    }

    /// <summary>
    /// Resumes the current paused gameplay session:
    /// -Resumes the countdown timer.
    /// -Reenables obstacle spawning, music and player controls.
    /// -Hides the Paused UI.
    /// </summary>
    public void ResumeSession()
    {
        Time.timeScale = 1f;
        gamePaused = false;
        timer.Resume();
        obstacleSpawner.ResumeSpawning();
        excavator.EnableControls();
        excavator.EnableEngineSound();
        audioManager.UnpauseBackgroundMusic();
        gameMenu.ApplyPausedUI(false);
    }

    /// <summary>
    /// Ends the current gameplay session when countdown timer reaches zero:
    /// -Stops the countdown timer and all physics updates.
    /// -Disables obstacle spawning and player controls.
    /// -Updates HUD switches to game over music and sfx and shows the Game Over UI.
    /// </summary>
    public void EndSession()
    {
        if (!state.isRunning) return;
        Debug.Log("GameManager: Time is 0. GAME OVER");
        Time.timeScale = 0f;
        state.isRunning = false;
        timer.Pause();
        excavator.DisableControls();
        excavator.DisableEngineSound();
        
        audioManager.SwitchGameOverMusic();
        obstacleSpawner.StopSpawning();
        hud.HideInteractionText();
        hud.ClearNotificationsToast();
        hud.SetFinalScoreText(state.score);
        gameMenu.ShowGameOverUI();

        if (state.score > leaderboardManager.LowestHighScore())
        {
            gameMenu.ShowLeaderboardInput();
            audioManager.PlayGameOverRecordSFX();
        }
        else
        {
            gameMenu.HideLeaderboardInput();
            audioManager.PlayGameOverSFX();
        }
    }

    /// <summary>
    /// Restarts the current session by resetting the UI, music and delegating to StartSession.
    /// </summary>
    public void RestartSession()
    {
        audioManager.StopBackgroundMusic();
        audioManager.SwitchRegularMusic();
        audioManager.PlayBackgroundMusic();
        gameMenu.HidePause();
        gameMenu.HideGameOver();
        
        StartSession();
    }

    /// <summary>
    /// Repositions the player and camera to the spawn points.
    /// Halves the vehicle deposit, updates the UI and shows the respawn notification.
    /// </summary>
    public void RespawnPlayer()
    {
        Debug.Log("GameManager: Respawning player");
        excavator.HandleReposition(playerSpawn.position, playerSpawn.rotation);
        cameraController.SetFixedPose(cameraRespawnTransform, excavator.transform);
        int coal = state.coalInDepot;
        state.coalInDepot = coal / 2;
        hud.SetCoalText(state.coalInDepot);
        hud.ShowNotificationToast(LocalizationSettings.StringDatabase.GetLocalizedString("respawnNotification"), 2f);
    }

    /// <summary>
    /// Add a new leaderboard entry to the leaderboard using the game score and
    /// input name.
    /// </summary>
    /// <param name="name">Player input name</param>
    public void AddLeaderboardEntry(string name)
    {
        leaderboardManager.AddLeaderboardEntry(state.score, name);
    }

    /// <summary>
    /// Handles the coal collection event.
    /// Increments the count on the deposit, plays collect SFX and updates the HUD.
    /// </summary>
    private void HandleCollectCoal(bool success)
    {
        if (success)
        {
            state.coalInDepot++;
            audioManager.PlayCollectSFX();
            hud.SetCoalText(state.coalInDepot);
        }
        else
        {
            audioManager.PlayDepositFailSFX();
        }
       
    }

    /// <summary>
    /// Handles the coal deposit event.
    /// Adds time and score based on the config, clears deposit, plays deposit sfx updates the HUD.
    /// </summary>
    private void HandleDepositCoal(bool success)
    {
        if (success)
        {
            timer.AddTime(state.coalInDepot * config.timePerCoalUnit);
            state.score += state.coalInDepot * config.pointsPerCoalUnit;
            state.coalInDepot = 0;
            audioManager.PlayDepositSFX();
            hud.SetCoalText(state.coalInDepot);
            hud.SetScoreText(state.score);
        }
        else if(state.isRunning)
        {
            audioManager.PlayDepositFailSFX();
        }
        
    }

    /// <summary>
    /// Handles the low time event from TimerController.
    /// Plays low time SFX and switches to low time music.
    /// </summary>
    public void PlayLowTimeMusic()
    {
        audioManager.PlayClockSFX();
        audioManager.SwitchLowTimeMusic();
    }

    /// <summary>
    /// Plays the SFX according to the spawned obstacle.
    /// </summary>
    /// <param name="id">Spawned obstacle id</param>
    private void HandleObstacleSpawnSFX(int id)
    {
        switch (id)
        {
            case 0: audioManager.PlayGasSpawnSFX(); break;
            case 1: audioManager.PlayOilSpawnSFX(); break;
            case 2: audioManager.PlayRockSpawnSFX(); break;
            default: break;
        }
    }

}
