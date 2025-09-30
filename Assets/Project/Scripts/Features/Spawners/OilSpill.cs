using UnityEngine;

/// <summary>
/// Triggers an oil slip when a player enters this oil spill trigger.
/// </summary>
public class OilSpill : MonoBehaviour
{
    [SerializeField] private float duration = 2.0f;
    private AudioManager audioManager;

    /// <summary>
    /// Called by Unity when another collider enters this trigger.
    /// If the collider belongs to a player, it starts an slip effect on the excavator and triggers
    /// the Oil Slip SFX.
    /// </summary>
    /// <param name="other">Data from the collider that entered the trigger</param>
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("OilSlip: Vehicle entered oil puddle obstacle");
        //Slip logic handled in excavatorController.
        var excavator = other.GetComponentInChildren<ExcavatorController>();
        excavator.TriggerOilSlip(duration);
        audioManager.PlayOilTriggerSFX();
    }

    /// <summary>
    /// Injects the AudioManager to operate on.
    /// Injected by the spawner during instatiation.
    /// </summary>
    /// <param name="t">Injected audio manager</param>
    public void AddAudioManager(AudioManager a)
    {
        audioManager = a;
    }
}
