using UnityEngine;

public class OilSpill : MonoBehaviour
{


    [SerializeField] private float duration = 2.0f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("Forklift entered oil puddle obstacle");
        //Slip logic handled in excavatorController.
        var excavator = other.GetComponentInChildren<ExcavatorController>();
        excavator.TriggerOilSlip(duration);

    }
}
