using System;
using UnityEngine;

/// <summary>
/// Handles the in-game countdown timer.
/// Raises events on significant timer milestones like whole second change, reaching zero and low time.
/// </summary>
public class TimerController : MonoBehaviour
{
    private float timeRemaining;
    private bool isRunning;
    private bool timeUpInvoked;
    private int previousSecond;
    /// <summary>
    /// Invoked when remaining time crosses into a new second and 
    /// after methods that modify the countdown (start/add/remove) for synchronization.
    /// </summary>
    public event Action<float> OnTimeUpdate;
    /// <summary>
    /// Invoked when countdown reaches zero.
    /// </summary>
    public event Action OnTimeUp;
    /// <summary>
    /// Invoked when countdown reaches 30s.
    /// </summary>
    public event Action OnLowTime;

    /// <summary>
    /// Called by Unity once per frame. Reduces the countdown once per frame using scaled time.
    /// Invokes a time-up event just once when reaching zero and low time (30s) and notifies each time 
    /// the countdown changes a whole second.
    /// </summary>
    void Update()
    {
        if (!isRunning) return;
        //Time.delta time works independently from framerate 
        timeRemaining -= Time.deltaTime;
 
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            isRunning = false;
            NotifySecondChanged();

            if (!timeUpInvoked)
            {
                OnTimeUp?.Invoke();
                timeUpInvoked = true;
            }

        }
        else
        {
            NotifySecondChanged();
        }
    }

    /// <summary>
    /// Starts or restarts the countdown with an specified number of seconds.
    /// Emits an OnTimeUpdate for synchronization.
    /// </summary>
    /// <param name="initialTimeSeconds">Initial time for countdown in seconds.</param>
    public void StartTimer(int initialTimeSeconds)
    {
        timeRemaining = initialTimeSeconds;
        isRunning = true;
        timeUpInvoked = false;
        previousSecond = -1;
        OnTimeUpdate?.Invoke(timeRemaining);
    }

    /// <summary>
    /// Adds time to the countdown and resumes if it was at zero.
    /// Emits an OnTimeUpdate for synchronization.
    /// </summary>
    /// <param name="time">Seconds to add (> 0). </param>
    public void AddTime(float time)
    {
        if (time <= 0f) return;
        timeRemaining += time;
        if (timeRemaining > 0f) isRunning = true;
        OnTimeUpdate?.Invoke(timeRemaining);
    }

    /// <summary>
    /// Removes  time from the countdown.
    /// Emits an OnTimeUpdate for synchronization and OnTimeUp if it reaches zero.
    /// </summary>
    /// <param name="time">Seconds to remove (> 0).</param>
    public void RemoveTime(float time)
    {
        if (time <= 0f) return;
        timeRemaining = Mathf.Max(0f, timeRemaining - time);
        OnTimeUpdate?.Invoke(timeRemaining);

        if (timeRemaining == 0f)
        {
            isRunning = false;

            if (!timeUpInvoked)
            {
                OnTimeUp?.Invoke();
                timeUpInvoked = true;

            }
        }
    }

    /// <summary>
    /// Pauses the countdown.
    /// </summary>
    public void Pause()
    {
        isRunning = false;
    }

    /// <summary>
    /// Resumes the countdown if there is time remaining.
    /// </summary>
    public void Resume()
    {
        isRunning = timeRemaining > 0f;
    }

    /// <summary>
    /// Gets the remaining time in seconds.
    /// </summary>
    /// <returns>Remaining time.</returns>
    public float GetTimeRemaining()
    {
        return timeRemaining;
    }

    /// <summary>
    /// Emits an OnTimeUpdate only when the whole second changes.
    /// Reduces HUD updates compared to invoking the event once-per-frame in Update.
    /// </summary>
    private void NotifySecondChanged()
    {
        int currentSecond = Mathf.FloorToInt(timeRemaining);

        if (currentSecond != previousSecond)
        {
            previousSecond = currentSecond;
            if(currentSecond == 30) OnLowTime?.Invoke();
            OnTimeUpdate?.Invoke(timeRemaining);
        }
    }


}
