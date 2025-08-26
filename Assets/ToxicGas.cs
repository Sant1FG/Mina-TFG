using UnityEngine;

public class ToxicGas : MonoBehaviour
{
    private TimerController timerController;


    public void AddTimerController(TimerController t) {
        timerController = t;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Forklift entered toxic gas obstacle");
        //Reduce time on timeController.
        var interaction = other.GetComponentInChildren<InteractionController>();
        //if (interaction != null)
        //interaction.NotifyGasEntered(this);
        timerController.RemoveTime(5f);
    }

}
