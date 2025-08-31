using UnityEngine;

/// <summary>
/// Handles the driving of the excavator using WheelColliders.
/// Reads player input, applies motor, break, steering and updates wheel meshes.
/// Supports temporary worse handling when triggering an "oil slip" obstacle.
/// </summary>
public class ExcavatorController : MonoBehaviour
{

    private Rigidbody playerRB;
    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;
    public float mainPedalInput;
    public float steeringInput;
    [SerializeField] private float enginePower;
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

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// Stores the reference to the rigidBody.
    /// </summary>
    private void Awake()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Called by Unity before the first execution of Update.
    /// Applies the center of mass vertical offset to improve stability.
    /// </summary>
    private void Start()
    {
        if (playerRB != null)
        {
            var com = playerRB.centerOfMass;
            com.y += centerOfMassYOffset;
            playerRB.centerOfMass = com;
        }
    }

    /// <summary>
    /// Called by Unity once per Physics update.Handles the oil slip timers and values,reads player input, 
    /// applies motor, break, steering and updates wheel meshes to sync with Wheel Colliders. 
    /// Does not work if controls are not enabled.
    /// </summary>
    private void FixedUpdate()
    {

        if (!controlsEnabled) return;

        if (oilSlipActive && Time.time >= oilSlipDuration)
        {
            oilSlipActive = false;
            //Restore original values
            playerRB.angularDamping = originalAngularDrag;
            maxSteerAngle = originalMaxSteerAngle;
        }

        GetInput();
        ApplyMotor();
        ApplySteering();
        UpdateWheelPosition();


    }

    /// <summary>
    /// Reads player input from space key and the horizontal and vertical axis (WASD). 
    /// </summary>
    private void GetInput()
    {
        mainPedalInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");
        breakInput = Input.GetKey(KeyCode.Space);
    }

    /// <summary>
    /// Calculates the motor and break torque based on player input, direction of travel, coasting
    /// speed limit, reverse braking and the handling modifiers applied by an "oil slip" obstacle.
    /// Applies those values to the wheels. 
    /// </summary>
    private void ApplyMotor()
    {
        Vector3 v = playerRB.linearVelocity;
        float speed = new Vector3(v.x, 0f, v.z).magnitude;
        float speedKPH = speed * 3.6f;
        float moveDir = Vector3.Dot(transform.forward, v) >= 0f ? 1f : -1f;


        float throttle = mainPedalInput;
        bool brakingByKey = breakInput;
        bool coastingBrake = Mathf.Approximately(throttle, 0f) && speed > 0.5f;
        bool brakingByReverse = (throttle != 0f) && (Mathf.Sign(throttle) != Mathf.Sign(moveDir)) && speed > 0.5f;

        float torque = enginePower * throttle;
        if (oilSlipActive) torque *= oilSlipMotorFactor;

        float brakeTorque = 0f;

        if (brakingByKey || brakingByReverse)
        {
            torque = 0f;
            brakeTorque = fullBrakeTorque;
        }
        else if (coastingBrake)
        {
            //Breaks when gas is released
            brakeTorque = coastBrakeTorque;
        }

        if (speedKPH >= maxSpeedKPH)
        {
            torque = 0f;
            brakeTorque = Mathf.Max(brakeTorque, limiterCoastBrake);
        }

        colliders.FLWheel.motorTorque = torque;
        colliders.FRWheel.motorTorque = torque;
        colliders.RLWheel.motorTorque = torque;
        colliders.RRWheel.motorTorque = torque;

        // Break distribution 65 Front, 35 Rear.
        float fFront = 0.65f, fRear = 0.35f;
        colliders.FLWheel.brakeTorque = brakeTorque * fFront;
        colliders.FRWheel.brakeTorque = brakeTorque * fFront;
        colliders.RLWheel.brakeTorque = brakeTorque * fRear;
        colliders.RRWheel.brakeTorque = brakeTorque * fRear;
    }

    /// <summary>
    /// Converts steering input into a wheel steer angle and applies it to the front wheels.
    /// </summary>
    private void ApplySteering()
    {
        steerAngle = maxSteerAngle * steeringInput;
        colliders.FLWheel.steerAngle = steerAngle;
        colliders.FRWheel.steerAngle = steerAngle;
    }

    /// <summary>
    /// Synchronizes the visual wheel meshes with the corresponding wheel colliders.
    /// </summary>
    private void UpdateWheelPosition()
    {
        UpdateWheel(colliders.FRWheel, wheelMeshes.FRWheel);
        UpdateWheel(colliders.FLWheel, wheelMeshes.FLWheel);
        UpdateWheel(colliders.RRWheel, wheelMeshes.RRWheel);
        UpdateWheel(colliders.RLWheel, wheelMeshes.RLWheel);
    }

    /// <summary>
    /// Copies the position and rotation of the wheel collider to the visual mesh.
    /// </summary>
    /// <param name="coll">Source wheel collider.</param>
    /// <param name="wheelMesh">Target mesh renderer.</param>
    private void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }

    /// <summary>
    /// Triggers a temporary "oil slip" effect for a given duration.
    /// Increases angular damping and reduces maximum steering angle and motor effectiveness.
    /// Original values are restored when the effect ends.
    /// </summary>
    /// <param name="duration">Duration of the slip effect in seconds.</param>
    public void TriggerOilSlip(float duration)
    {
        oilSlipActive = true;
        oilSlipDuration = Time.time + duration;

        //Save original values
        if (!valuesSaved)
        {
            originalAngularDrag = playerRB.angularDamping;
            originalMaxSteerAngle = maxSteerAngle;
            valuesSaved = true;
        }

        playerRB.angularDamping = oilSlipAngularDrag;
        maxSteerAngle = originalMaxSteerAngle * oilSlipSteerFactor;
    }

    /// <summary>
    /// Removes all forces affecting the vehicle and safely repositions it to provided position and rotation.
    /// </summary>
    /// <param name="position">Target world position.</param>
    /// <param name="rotation">Target world rotation.</param>
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

    /// <summary>
    /// Enables player controls for the vehicle.
    /// </summary>
    public void EnableControls()
    {
        controlsEnabled = true;
    }

    /// <summary>
    /// Disables player controls for the vehicle.
    /// </summary>
    public void DisableControls()
    {
        controlsEnabled = false;
    }
}

/// <summary>
/// Group of wheel collider references
/// </summary>
[System.Serializable]
public class WheelColliders {

    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;

}

/// <summary>
/// Group of wheel meshes references.
/// </summary>
[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer RRWheel;
    public MeshRenderer RLWheel;
}

