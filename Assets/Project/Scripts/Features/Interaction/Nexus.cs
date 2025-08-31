using UnityEngine;

/// <summary>
/// Notifies when the player enters or exits the nexus area.
/// </summary>
public class Nexus : MonoBehaviour
{   
    /// <summary>
    /// Called by Unity when another collider enters the Nexus area.
    /// If the collider belongs to a player, forwards an enter notification to its controller.
    /// </summary>
    /// <param name="other">Data from the collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
            interaction.NotifyNexusEntered(this);
    }

    /// <summary>
    /// Called by Unity when another collider exits the Nexus area.
    /// If the collider belongs to a player, forwards an exit notification to its controller.
    /// </summary>
    /// <param name="other">Data from the collider that exited the trigger</param>
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
            interaction.NotifyNexusExited(this);
    }
}
