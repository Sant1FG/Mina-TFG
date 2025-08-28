using System.Security;
using UnityEngine;

public class ExcavatorController : MonoBehaviour
{

    private Rigidbody playerRB;
    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;
    public float mainPedalInput;
    public float steeringInput;
    [SerializeField] private float enginePower;
    private float breakPower = 0f;
    [SerializeField] private float maxSteerAngle;
    [SerializeField] private float centerOfMassYOffset = -0.55f;
    private bool breakInput;
    private bool oilSlipActive;
    private float oilSlipDuration;
    [SerializeField] private float oilSlipAngularDrag = 1.2f;
    [SerializeField] private float oilSlipSteerFactor = 0.5f;
    [SerializeField] private float oilSlipMotorFactor = 0.7f;
    private float originalAngularDrag;
    private float originalMaxSteerAngle;
    private bool valuesSaved;
    private bool controlsEnabled = false;


    private float steerAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //Game manager might call for Reposition before Start()
        playerRB = gameObject.GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (playerRB != null)
        {
            var com = playerRB.centerOfMass;
            com.y += centerOfMassYOffset;
            playerRB.centerOfMass = com;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (!controlsEnabled) return;

        if (oilSlipActive && Time.time >= oilSlipDuration)
        {
            oilSlipActive = false;
            // Restaurar valores originales
            playerRB.angularDamping = originalAngularDrag;
            maxSteerAngle = originalMaxSteerAngle;
        }

        GetInput();
        ApplyMotor();
        ApplySteering();
        UpdateWheelPosition();


    }

    void GetInput()
    {
        mainPedalInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        breakInput = Input.GetKey(KeyCode.Space);
    }

    void ApplyMotor()
    {

        // AÑADIDO: boost a baja velocidad
        float speed = new Vector3(playerRB.linearVelocity.x, 0f, playerRB.linearVelocity.z).magnitude;
        float torque = enginePower * mainPedalInput;
        if (oilSlipActive) torque *= oilSlipMotorFactor;
        if (speed < 2f && mainPedalInput > 0f) torque *= 2.5f;

        colliders.RRWheel.motorTorque = torque;
        colliders.RLWheel.motorTorque = torque;
        breakPower = breakInput ? 7000f : 0f;
        colliders.FLWheel.brakeTorque = breakPower * 0.65f;
        colliders.FRWheel.brakeTorque = breakPower * 0.65f;
        colliders.RLWheel.brakeTorque = breakPower * 0.35f;
        colliders.RRWheel.brakeTorque = breakPower * 0.35f;

    }

    void ApplySteering()
    {
        steerAngle = maxSteerAngle * steeringInput;
        colliders.FLWheel.steerAngle = steerAngle;
        colliders.FRWheel.steerAngle = steerAngle;
    }

    //Update the WheelMeshes to follow the WheelColliders
    void UpdateWheelPosition()
    {
        UpdateWheel(colliders.FRWheel, wheelMeshes.FRWheel);
        UpdateWheel(colliders.FLWheel, wheelMeshes.FLWheel);
        UpdateWheel(colliders.RRWheel, wheelMeshes.RRWheel);
        UpdateWheel(colliders.RLWheel, wheelMeshes.RLWheel);
    }

    void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }

    public void TriggerOilSlip(float duration)
    {
        oilSlipActive = true;
        oilSlipDuration = Time.time + duration;

        // Guardar valores originales (una sola vez)
        if (!valuesSaved)
        {
            originalAngularDrag = playerRB.angularDamping;
            originalMaxSteerAngle = maxSteerAngle;
            valuesSaved = true;
        }

        // Ajustar temporalmente
        playerRB.angularDamping = oilSlipAngularDrag;
        maxSteerAngle = originalMaxSteerAngle * oilSlipSteerFactor;
    }

    public void Reposition(Vector3 position, Quaternion rotation)
    {
        playerRB.position = position;
        playerRB.rotation = rotation;
        playerRB.linearVelocity = Vector3.zero;
        playerRB.angularVelocity = Vector3.zero;
    }

    public void EnableControls()
    {
        controlsEnabled = true;
    }

    public void DisableControls()
    {
        controlsEnabled = false;
    }
}

[System.Serializable]
public class WheelColliders {

    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;

}

[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer RRWheel;
    public MeshRenderer RLWheel;
}

