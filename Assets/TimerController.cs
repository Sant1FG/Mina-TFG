using System;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    private float timeRemaining;
    private bool running = false;
    private bool timeUpFired;

    public event Action<float> OnTick;
    public event Action OnTimeUp;

    // Update is called once per frame
    void Update()
    {
        if (!running) return;
        timeRemaining -= Time.deltaTime;

         if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            running = false;
            OnTick?.Invoke(timeRemaining);

            if (!timeUpFired)
            {
                OnTimeUp?.Invoke();
                timeUpFired = true;
            }
            
        }else
        {
            OnTick?.Invoke(timeRemaining);
        }       
    }

    public void StartTimer(int initialTimeSeconds)
    {
        timeRemaining = initialTimeSeconds;
        running = true;
        timeUpFired = false;
        OnTick?.Invoke(timeRemaining);
    }

    public void AddTime(float time)
    {
        if (time <= 0f) return;
        timeRemaining += time;
        if (timeRemaining > 0f) running = true;
        OnTick?.Invoke(timeRemaining);
    }

    public void RemoveTime(float time)
    {
        if (time <= 0f) return;
        timeRemaining = Mathf.Max(0f, timeRemaining - time);
        OnTick?.Invoke(timeRemaining);
    
        if (timeRemaining == 0f)
        {
            running = false;

            if (!timeUpFired)
            {
                OnTimeUp?.Invoke();
                timeUpFired = true;
            
            }
        }
    }

    public void Pause()
    {
        running = false;
    }

    public void Resume()
    {
        running = timeRemaining > 0f;
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }


}
