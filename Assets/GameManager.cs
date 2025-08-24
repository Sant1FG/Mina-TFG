using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameConfig config;
    [SerializeField] private HUDController hud;
    [SerializeField] private InteractionController interaction;

    public SessionState state;

    private void Start()
    {
        StartSession();
    }

    public void StartSession()
    {
        state = new SessionState();

        // Inicializa según configuración
        state.timeRemaining = config.initialTimeSeconds;
        state.score = 0;
        state.coalInDepot = 0;
        state.isRunning = true;

        // Distribuye referencias a otros controladores
        hud.SetSessionState(state);
        hud.setConfig(config);
        interaction.SetSessionState(state);
        interaction.setConfig(config);

        // Refresca HUD inicial
        //hud.SetTime(state.timeRemaining);
        //hud.SetScore(state.score);
        //hud.SetCoal(state.coalInDepot);
    }

    public SessionState GetSessionState() => state;
}
