using UnityEngine;

/// <summary>
/// Notifies when the player enters the puddle area.
/// </summary>
public class Puddle : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;

    /// <summary>
    /// Called by Unity when another collider enters this trigger.
    /// If the collider belongs to a player, it plays the puddle sfx.
    /// </summary>
    /// <param name="other">Data from the collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Puddle: Vehicle entered water puddle");
        audioManager.PlayPuddleSFX();
    }
}
