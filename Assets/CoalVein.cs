using System;
using UnityEngine;

public class CoalVein : MonoBehaviour
{
    public int unit = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Forklift entered vein range");

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
        interaction.NotifyVeinEntered(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        Debug.Log("Forkling left vein range");

        var interaction = other.GetComponentInChildren<InteractionController>();
        if (interaction != null)
        interaction.NotifyVeinExited(this);
    }
}
