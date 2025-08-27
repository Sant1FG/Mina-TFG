using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private HUDController hud;
    [SerializeField] private TimerController timer;
    [SerializeField] private InteractionController interaction;
    [SerializeField] private ExcavatorController excavator;
    [SerializeField] private GameMenuController gameMenu;
    [SerializeField] private KeyCode respawnKey = KeyCode.T;
    [SerializeField] private Transform playerSpawn;

    public SessionState state;

    void OnEnable()
    {
        if (timer != null) timer.OnTimeUp += StopSession;
        if (interaction != null)
        {
            interaction.OnCollectCoal += HandleCollectCoal;
            interaction.OnDepositCoal += HandleDepositCoal;
        }

        // Si insistes en reenviar toasts desde el GM, deja esto (pero mejor pásalo al HUD o a un canal SO):
        // if (spawner) spawner.onObstacleSpawn += HandleNotificationToast;
        // if (oil)     oil.OnOilSlip         += HandleNotificationToast;
        // if (gas)     gas.OnGasEnter        += HandleNotificationToast;
    }

    void OnDisable()
    {
        if (timer != null) timer.OnTimeUp -= StopSession;
        if (interaction != null)
        {
            interaction.OnCollectCoal -= HandleCollectCoal;
            interaction.OnDepositCoal -= HandleDepositCoal;
        }
        // if (spawner) spawner.onObstacleSpawn -= HandleNotificationToast;
        // if (oil)     oil.OnOilSlip          -= HandleNotificationToast;
        // if (gas)     gas.OnGasEnter         -= HandleNotificationToast;
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
        state = new SessionState();

        // Inicializa según configuración
        state.score = 0;
        state.coalInDepot = 0;
        state.isRunning = true;
        //Colocar al jugador al inicio partida
        RespawnPlayer();

        // Distribuye referencias a otros controladores
        interaction.SetSessionState(state);
        interaction.setConfig(config);

        timer.StartTimer(config.initialTimeSeconds);

        gameMenu.hideTutorial();
        //Activate and update initial HUD
        gameMenu.ShowHUD();
        gameMenu.showOptions();
    }

    private void StopSession()
    {
        Debug.Log("Time is 0. GAME OVER");
        state.isRunning = false;
        gameMenu.hidePause();
        gameMenu.hideHUD();
        gameMenu.hideOptions();
        gameMenu.showGameOver();
    }

    private void RespawnPlayer()
    {
        Debug.Log("GameManager: ");
        excavator.Reposition(playerSpawn.position, playerSpawn.rotation);
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
