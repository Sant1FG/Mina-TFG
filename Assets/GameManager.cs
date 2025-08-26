using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private HUDController hud;
    [SerializeField] private TimerController timer;
    [SerializeField] private InteractionController interaction;

    public SessionState state;


    public void Awake()
    {
        timer.OnTimeUp += StopSession;
    }

    private void Start()
    {
        StartSession();
    }

    public void StartSession()
    {
        state = new SessionState();

        // Inicializa según configuración
        //state.timeRemaining = config.initialTimeSeconds;
        state.score = 0;
        state.coalInDepot = 0;
        state.isRunning = true;

        // Distribuye referencias a otros controladores
        hud.SetSessionState(state);
        hud.setConfig(config);
        interaction.SetSessionState(state);
        interaction.setConfig(config);

        timer.StartTimer(config.initialTimeSeconds);

        // Refresca HUD inicial
        //hud.SetTime(state.timeRemaining);
        //hud.SetScore(state.score);
        //hud.SetCoal(state.coalInDepot);
    }

    private void StopSession()
    {
        Debug.Log("Time is 0. GAME OVER");
        state.isRunning = false;
    }

    public SessionState GetSessionState() => state;
}
