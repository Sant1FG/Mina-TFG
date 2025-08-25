using UnityEngine;

public class ToxicGas : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Forklift entered toxic gas obstacle");
        //Reduce time on timeController.
        var interaction = other.GetComponentInChildren<InteractionController>();
        //if (interaction != null)
        //interaction.NotifyGasEntered(this);
    }

}
