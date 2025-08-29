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
    [SerializeField] private float fullBrakeTorque = 20000f;
    [SerializeField] private float coastBrakeTorque = 1200f;
    [SerializeField] private float limiterCoastBrake = 1200f;
    [SerializeField] private float maxSpeedKPH = 40f;     
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
    // --- Velocidad horizontal y dirección de marcha
    Vector3 v = playerRB.linearVelocity;
    float speed = new Vector3(v.x, 0f, v.z).magnitude;
    float speedKPH = speed * 3.6f;
    float moveDir = Vector3.Dot(transform.forward, v) >= 0f ? 1f : -1f;

    // --- Entrada de usuario
    float throttle = mainPedalInput;           // [-1..1]
    bool brakingByKey = breakInput;
    bool coastingBrake = Mathf.Approximately(throttle, 0f) && speed > 0.5f;
    bool brakingByReverse = (throttle != 0f) && (Mathf.Sign(throttle) != Mathf.Sign(moveDir)) && speed > 0.5f;

    // --- Par motor base
    float torque = enginePower * throttle;
    if (oilSlipActive) torque *= oilSlipMotorFactor;

    // --- Frenos
    float brakeTorque = 0f;

    if (brakingByKey || brakingByReverse)
    {
        // Frenada fuerte y cortar motor
        torque = 0f;
        brakeTorque = fullBrakeTorque;
    }
    else if (coastingBrake)
    {
        // Freno motor al soltar gas
        brakeTorque = coastBrakeTorque;
    }

    // --- Limitador de velocidad suave
    if (speedKPH >= maxSpeedKPH)
    {
        torque = 0f; // siempre cortar motor cuando supera el tope
        brakeTorque = Mathf.Max(brakeTorque, limiterCoastBrake);
    }

    // --- Aplicación: 4x4 (si quieres solo tracción trasera, aplica a RL/RR)
    colliders.FLWheel.motorTorque = torque;
    colliders.FRWheel.motorTorque = torque;
    colliders.RLWheel.motorTorque = torque;
    colliders.RRWheel.motorTorque = torque;

    // Distribución de freno 65/35 (delante/detrás)
    float fFront = 0.65f, fRear = 0.35f;
    colliders.FLWheel.brakeTorque = brakeTorque * fFront;
    colliders.FRWheel.brakeTorque = brakeTorque * fFront;
    colliders.RLWheel.brakeTorque = brakeTorque * fRear;
    colliders.RRWheel.brakeTorque = brakeTorque * fRear;
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
        const float freezeBrake = 30000f; 
        colliders.FLWheel.motorTorque = 0f;
        colliders.FRWheel.motorTorque = 0f;
        colliders.RLWheel.motorTorque = 0f;
        colliders.RRWheel.motorTorque = 0f;

        colliders.FLWheel.brakeTorque = freezeBrake;
        colliders.FRWheel.brakeTorque = freezeBrake;
        colliders.RLWheel.brakeTorque = freezeBrake;
        colliders.RRWheel.brakeTorque = freezeBrake;

        colliders.FLWheel.steerAngle = 0f;
        colliders.FRWheel.steerAngle = 0f;

        playerRB.linearVelocity = Vector3.zero;
        playerRB.angularVelocity = Vector3.zero;

        playerRB.position = position;
        playerRB.rotation = rotation;
      
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

