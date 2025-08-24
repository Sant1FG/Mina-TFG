using UnityEngine;

public class Nexus : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Forkling entered nexus range");

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
        interaction.NotifyNexusEntered(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        Debug.Log("Forkling left nexus range");

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
        interaction.NotifyNexusExited(this);
    }
}
