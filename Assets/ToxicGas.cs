using System;
using UnityEngine;

public class ToxicGas : MonoBehaviour
{
    private TimerController timerController;

    public void AddTimerController(TimerController t)
    {
        timerController = t;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Forklift entered toxic gas obstacle");
        //Reduce time on timeController.
        var interaction = other.GetComponentInChildren<InteractionController>();
        timerController.RemoveTime(5f);
    }

}
