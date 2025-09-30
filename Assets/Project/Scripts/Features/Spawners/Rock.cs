using UnityEngine;

/// <summary>
/// Blocks the player path
/// </summary>
public class Rock : MonoBehaviour
{
    private AudioManager audioManager;
    private float rockSoundCooldown = 2f;
    private float lastRockSoundTime = -1f;

    /// <summary>
    /// Injects the audio manager to operate on.
    /// Injected by the spawner during instatiation.
    /// </summary>
    /// <param name="t">Injected audio manager</param>
    public void AddAudioManager(AudioManager a)
    {
        audioManager = a;
    }

    /// <summary>
    /// Called by Unity when another collider enters this trigger.
    /// If the collider belongs to a player, it plays the according SFX with an internal cooldown.
    /// </summary>
    /// <param name="other">Data from the collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Rock: Vehicle hit the rock obstacle");
        //Prevents spamming
        if (Time.time - lastRockSoundTime >= rockSoundCooldown)
        {
            lastRockSoundTime = Time.time;
            audioManager.PlayRockTriggerSFX();
        }

    }
}
