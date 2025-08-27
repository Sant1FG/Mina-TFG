using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coalText;
    [SerializeField] private GameObject interactionText;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
     //Observer
    [SerializeField] private TimerController timer;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private InteractionController interaction;


    private void Awake()
    {
        if (interactionText != null) interactionText.gameObject.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        if (timer != null) timer.OnTick += SetTimerText;
        if (interaction != null)
        {
            interaction.OnNotificationToast += ShowNotificationToast;
            interaction.OnShowInteraction += HandleShowInteraction;
        }
        if (spawner != null) spawner.onObstacleSpawn += ShowNotificationToast;
    }

    void OnDisable()
    {
        if (timer != null) timer.OnTick -= SetTimerText;
        if (interaction != null)
        {
            interaction.OnNotificationToast -= ShowNotificationToast;
            interaction.OnShowInteraction -= HandleShowInteraction;
        }
        if (spawner != null) spawner.onObstacleSpawn -= ShowNotificationToast;
    }

    private void SetTimerText(float time)
    {
        timerText.text = time.ToString("f0");
    }

    public void SetCoalText(int value)
    {
        coalText.text = value.ToString();
    }

    public void SetScoreText(int value)
    {
        scoreText.text = value.ToString();
    }

    public void ShowInteractionText()
    {
        if (interactionText == null) return;
        interactionText.gameObject.SetActive(true);
    }

     public void ShowNotificationToast(string msg, float duration)
    {
        if (notificationText == null) return;
        StartCoroutine(ToastRoutine(msg, duration));
    }

    private IEnumerator ToastRoutine(string msg, float duration)
    {
         notificationText.text = msg;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        notificationText.gameObject.SetActive(false);
    }

    private void HandleShowInteraction(bool show)
    {
        if (show)
        {
            ShowInteractionText();
        }
        else
        {
            HideInteractionText();
        }
    }

    public void HideInteractionText()
    {
        if (interactionText == null) return;
        interactionText.gameObject.SetActive(false);
    }
    

}
