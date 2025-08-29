using System;
using UnityEditor;
using UnityEngine;

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

    void OnEnable()
    {
        if (timer != null) timer.OnTimeUp += EndSession;
        if (interaction != null)
        {
            interaction.OnCollectCoal += HandleCollectCoal;
            interaction.OnDepositCoal += HandleDepositCoal;
        }
    }

    void OnDisable()
    {
        if (timer != null) timer.OnTimeUp -= EndSession;
        if (interaction != null)
        {
            interaction.OnCollectCoal -= HandleCollectCoal;
            interaction.OnDepositCoal -= HandleDepositCoal;
        }
    }

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

    public void StartSession()
    {
        Time.timeScale = 1f;
        state = new SessionState();

        // Inicializa según configuración
        state.score = 0;
        state.coalInDepot = 0;
        state.isRunning = true;

        //Instancia la camara
        cameraController.SetPlayer(excavator.transform);

        // Distribuye referencias a otros controladores
        interaction.SetSessionState(state);
        interaction.SetConfig(config);

        obstacleSpawner.ResetObstacleSpawner();
        obstacleSpawner.ResumeSpawning();
        veinSpawner.ResetVeinSpawner();

        //Activa controles y coloca al jugador al inicio partida
        RespawnPlayer();
        excavator.EnableControls();

        //Update inicial hud
        hud.SetScoreText(state.score);
        hud.SetCoalText(state.coalInDepot);
        hud.SetDepositMaxText(config.depositMax);
        hud.HideInteractionText();
        hud.ClearNotificationsToast();

        //Hide other screens
        gameMenu.HideTutorial();
        gameMenu.HidePause();
        gameMenu.HideGameOver();
        //Activate and update initial HUD
        gameMenu.ShowHUD();
        gameMenu.ShowHUDButtons();

        timer.StartTimer(config.initialTimeSeconds);

    }

    public void PauseSession()
    {
        Time.timeScale = 0f;
        timer.Pause();
        obstacleSpawner.StopSpawning();
        excavator.DisableControls();
        hud.HideInteractionText();
        hud.ClearNotificationsToast();
        gameMenu.HideHUDButtons(); 
        gameMenu.ShowPause();
    }

    public void ResumeSession()
    {
        Time.timeScale = 1f;
        timer.Resume();
        obstacleSpawner.ResumeSpawning();
        excavator.EnableControls();
        gameMenu.ShowHUDButtons();   
        gameMenu.HidePause();
    }

    public void EndSession()
    {
        if (!state.isRunning) return;
        Debug.Log("Time is 0. GAME OVER");
        Time.timeScale = 0f;
        state.isRunning = false;
        timer.Pause();
        excavator.DisableControls();
        obstacleSpawner.StopSpawning();
        hud.HideInteractionText();
        hud.ClearNotificationsToast();
        gameMenu.HidePause();
        gameMenu.HideHUDButtons();
        gameMenu.HideHUD();
        hud.SetFinalScoreText(state.score);
        gameMenu.ShowGameOver();
    }

    public void RestartSession()
    {
        gameMenu.HidePause();
        gameMenu.HideGameOver();
        StartSession();
    }

    private void RespawnPlayer()
    {
        Debug.Log("GameManager: ");
        excavator.Reposition(playerSpawn.position, playerSpawn.rotation);
        cameraController.SetFixedPose(cameraRespawnTransform, excavator.transform);
        state.coalInDepot = 0;
        hud.SetCoalText(0);

        hud.ShowNotificationToast("Vehiculo recolocado. Deposito vaciado", 2f);
    }

    private void HandleCollectCoal()
    {
        state.coalInDepot++;
        hud.SetCoalText(state.coalInDepot);
    }

    private void HandleDepositCoal()
    {
        timer.AddTime(state.coalInDepot * config.timePerCoalUnit);
        state.score += state.coalInDepot * config.pointsPerCoalUnit;
        state.coalInDepot = 0;
        hud.SetCoalText(state.coalInDepot);
        hud.SetScoreText(state.score);
    }
}
