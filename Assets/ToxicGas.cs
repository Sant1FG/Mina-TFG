using UnityEngine;

/// <summary>
/// Removes time from the countdown timer when a player enters the toxic gas area.
/// </summary>
public class ToxicGas : MonoBehaviour
{
    private TimerController timerController;

    /// <summary>
    /// Injects the timer controller to operate on.
    /// Injected by the spawner during instatiation.
    /// </summary>
    /// <param name="t">Injected timer controller</param>
    public void AddTimerController(TimerController t)
    {
        timerController = t;
    }

    /// <summary>
    /// Called by Unity when another collider enters this trigger.
    /// If the collider belongs to a player, substracts a fixed amount of time from the countdown.
    /// </summary>
    /// <param name="other">Data from the collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("ToxicGas: Vehicle entered toxic gas obstacle");
        //Reduce time on timeController.
        timerController.RemoveTime(5f);
    }

}
