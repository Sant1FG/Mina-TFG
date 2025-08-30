using UnityEngine;

/// <summary>
/// Notifies when the player enters or exits the coal vein area.
/// </summary>
public class CoalVein : MonoBehaviour
{

    /// <summary>
    /// Called by Unity when another collider enters the coal vein area.
    /// If the collider belongs to a player, forwards an enter notification to its controller.
    /// </summary>
    /// <param name="other">Data from the collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Vehicle entered vein range");

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
            interaction.NotifyVeinEntered(this);
    }

    /// <summary>
    /// Called by Unity when another collider exits the coal vein area.
    /// If the collider belongs to a player, forwards an exit notification to its controller.
    /// </summary>
    /// <param name="other">Data from the collider that exited the trigger</param>
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Forkling left vein range");

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
            interaction.NotifyVeinExited(this);
    }
}
