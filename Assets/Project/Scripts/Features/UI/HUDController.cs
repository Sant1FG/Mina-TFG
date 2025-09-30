using UnityEngine;
using System.Collections;
using TMPro;

/// <summary>
/// Manages HUD elements (score, timer, coal, notifications, interaction prompts).
/// Subscribes to ingame events in order to update the canvas.
/// </summary>
public class HUDController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coalText;
    [SerializeField] private GameObject interactionText;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI depositMaxText;
    [SerializeField] private TextMeshProUGUI finalScoreText;

    //Observers
    [SerializeField] private TimerController timer;
    [SerializeField] private ObstacleSpawner spawner;
    [SerializeField] private InteractionController interaction;

    private Coroutine notificationCoroutine;

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// Initializes HUD visibility, hiding optionals panels at the start.
    /// </summary>
    private void Awake()
    {
        if (interactionText != null) interactionText.gameObject.SetActive(false);
        if (notificationText != null) notificationText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Subscribes to the following external events:
    /// - Internal timer updated from TimerController.
    /// - In-game notification from InteractionController.
    /// - Obstacle spawn notification from ObstacleSpawner.
    /// </summary>
    void OnEnable()
    {
        if (timer != null) timer.OnTimeUpdate += SetTimerText;
        if (interaction != null)
        {
            interaction.OnNotificationToast += ShowNotificationToast;
            interaction.OnShowInteraction += HandleShowInteraction;
        }
        if (spawner != null) spawner.OnObstacleSpawn += ShowNotificationToast;
    }

    /// <summary>
    /// Unsubscribes from external events
    /// </summary>
    void OnDisable()
    {
        if (timer != null) timer.OnTimeUpdate -= SetTimerText;
        if (interaction != null)
        {
            interaction.OnNotificationToast -= ShowNotificationToast;
            interaction.OnShowInteraction -= HandleShowInteraction;
        }
        if (spawner != null) spawner.OnObstacleSpawn -= ShowNotificationToast;
    }

    /// <summary>
    /// Updates the time on the canvas.
    /// </summary>
    /// <param name="time">Remaining time.</param>
    private void SetTimerText(float time)
    {
        timerText.SetText("{0:0}", time);
    }

    /// <summary>
    /// Sets the coal counter on the canvas.
    /// </summary>
    /// <param name="value">Current coal amount on deposit.</param>
    public void SetCoalText(int value)
    {
        coalText.SetText("{0}", value);
    }

    /// <summary>
    /// Sets the score on the canvas.
    /// </summary>
    /// <param name="value">Current player score.</param>
    public void SetScoreText(int value)
    {
        scoreText.SetText("{0}", value);
    }

    /// <summary>
    /// Sets the maximum deposit capacity on the canvas.
    /// </summary>
    /// <param name="value">Maximum deposit capacity </param>
    public void SetDepositMaxText(int value)
    {
        depositMaxText.SetText("{0}", value);
    }

    /// <summary>
    /// Sets the final score on the GameOver screen.
    /// </summary>
    /// <param name="value">Final score.</param>
    public void SetFinalScoreText(int value)
    {
        finalScoreText.SetText("{0}", value);
    }

    /// <summary>
    /// Shows the interaction panel on the canvas.
    /// </summary>
    public void ShowInteractionText()
    {
        if (interactionText == null) return;
        interactionText.gameObject.SetActive(true);
    }

    /// <summary>
    /// Displays a toast notification for a set duration on the canvas.
    /// Cancels any previous toast still running.
    /// </summary>
    /// <param name="msg">Message to display.</param>
    /// <param name="duration">Seconds the toast is visible.</param>
    public void ShowNotificationToast(string msg, float duration)
    {
        if (notificationText == null) return;

        //Stops previous toast if a new one comes
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }

        notificationCoroutine = StartCoroutine(ToastRoutine(msg, duration));
    }

    /// <summary>
    /// Coroutine that shows a toast message for a set duration
    /// </summary>
    /// <param name="msg">Toast message.</param>
    /// <param name="duration">Seconds the toast is visible.</param>
    /// <returns></returns>
    private IEnumerator ToastRoutine(string msg, float duration)
    {
        notificationText.SetText(msg);
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        notificationText.gameObject.SetActive(false);
        notificationCoroutine = null;
    }

    /// <summary>
    /// Cancels and hides any Toast running currently.
    /// </summary>
    public void ClearNotificationsToast()
    {
        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
            notificationCoroutine = null;
        }
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the Interaction panel depending on the provided flag.
    /// </summary>
    /// <param name="show">True to show, false to hide</param>
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

    /// <summary>
    /// Hides the Interaction panel on the canvas.
    /// </summary>
    public void HideInteractionText()
    {
        if (interactionText == null) return;
        interactionText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Event handler for Toxic gas.
    /// </summary>
    public void FlashTimerUI()
    {
        StartCoroutine(FlashTimer());
    }

    /// <summary>
    /// Turns the timer red for 3 seconds.
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlashTimer()
    {
        timerText.color = Color.red;
        yield return new WaitForSeconds(3);
        timerText.color = Color.white;
    }
}
