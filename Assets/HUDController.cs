using UnityEngine;
using TMPro;
using System.Collections;
using Unity.VisualScripting;
using System;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coalText;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TimerController timer;


    private SessionState state;
    private GameConfig config;
    private Coroutine notificationCoroutine;

    public void SetSessionState(SessionState sessionState)
    {
        state = sessionState;
    }

    public void setConfig(GameConfig gameConfig)
    {
        config = gameConfig;
    }

    private void Awake()
    {
        if (interactionText != null) interactionText.gameObject.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
        timer.OnTick += setTimerText;
    }

    private void setTimerText(float time)
    {
        timerText.text = time.ToString("f0");
    }

    public void setCoalText(int value)
    {
        coalText.text = value.ToString();
    }

    public void setScoreText(int value)
    {
        scoreText.text = value.ToString();
    }

    public void ShowInteractionText(string msg)
    {
        if (interactionText == null) return;
        interactionText.gameObject.SetActive(true);
        interactionText.text = msg;
    }

     public void ShowNotificationText(string msg, float duration)
    {
        if (notificationText == null) return;
        notificationCoroutine = StartCoroutine(ToastRoutine(msg, duration));
    }

    private IEnumerator ToastRoutine(string msg, float duration)
    {
         notificationText.text = msg;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }

    public void HideInteractionText()
    {
        if (interactionText == null) return;
        interactionText.gameObject.SetActive(false);
    }
    

}
