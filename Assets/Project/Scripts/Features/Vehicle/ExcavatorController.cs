using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the driving of the excavator using WheelColliders.
/// Reads player input, applies motor, break, steering and updates wheel meshes.
/// Supports temporary worse handling when triggering an "oil slip" obstacle.
/// Handles all SFX related to the excavator.
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
    [SerializeField] private AudioSource engineAudio;
    [SerializeField] private AudioSource brakeAudio;
    [SerializeField] private AudioClip brakeClip;
    [SerializeField] private float minPitch = 0.8f;
    [SerializeField] private float maxPitch = 2.0f;
    [SerializeField] private float brakeSoundCooldown = 1f; 
    private float lastBrakeSoundTime = -1f;
    private float originalAngularDrag;
    private float originalMaxSteerAngle;
    private bool valuesSaved;
    private bool controlsEnabled = false;
    private bool brakingSoundActive = false;
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
    /// Configures the engine audio of the excavator.
    /// </summary>
    private void Start()
    {
        if (playerRB != null)
        {
            var com = playerRB.centerOfMass;
            com.y += centerOfMassYOffset;
            playerRB.centerOfMass = com;
        }

         if (engineAudio != null)
        {
            engineAudio.loop = true;
            engineAudio.playOnAwake = false;
            
        }
    }

    /// <summary>
    /// Called by Unity once per Physics update.Handles the oil slip timers and values,reads player input, 
    /// applies motor, break, steering, updates wheel meshes to sync with Wheel Colliders and motor audio. 
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
        UpdateEngineAudio();


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
    /// Applies those values to the wheels. Handles break SFX.
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
        bool isBraking = brakingByKey || brakingByReverse;
        if (isBraking)
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
        PlayBrakeSound(speedKPH,isBraking);
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
    /// Updates motor engine sound pitch depending on the current speed.
    /// </summary>
    private void UpdateEngineAudio()
    {
        if (engineAudio == null) return;

        Vector3 v = playerRB.linearVelocity;
        float speed = new Vector3(v.x, 0f, v.z).magnitude;
        float speedKPH = speed * 3.6f;

        // Factor combining throttle and speed
        float intensity = Mathf.Abs(mainPedalInput) * 0.7f + speedKPH / maxSpeedKPH * 0.3f;
        float targetPitch = Mathf.Lerp(minPitch, maxPitch, intensity);

        engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, targetPitch, Time.deltaTime * 5f);
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
    public void HandleReposition(Vector3 position, Quaternion rotation)
    {
       StartCoroutine(Reposition(position, rotation));
    }

    /// <summary>
    /// Corroutine that removes all forces affecting the vehicle and its collider and safely repositions 
    /// it to provided position and rotation after waiting a physics step.
    /// </summary>
    /// <param name="position">Target world position.</param>
    /// <param name="rotation">Target world rotation.</param>
    public IEnumerator Reposition(Vector3 position, Quaternion rotation)
    {
        colliders.FLWheel.enabled = false;
        colliders.FRWheel.enabled = false;
        colliders.RLWheel.enabled = false;
        colliders.RRWheel.enabled = false;

        playerRB.linearVelocity = Vector3.zero;
        playerRB.angularVelocity = Vector3.zero;
        playerRB.Sleep();

        position += Vector3.up * 0.5f;

        playerRB.position = position;
        playerRB.rotation = rotation;

        // Wait for a physics step
        yield return new WaitForFixedUpdate();

        playerRB.WakeUp();

        colliders.FLWheel.enabled = true;
        colliders.FRWheel.enabled = true;
        colliders.RLWheel.enabled = true;
        colliders.RRWheel.enabled = true;
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

    /// <summary>
    /// Plays the brake SFX, sound pitch depending on speed, preventing sound looping and clipping.
    /// </summary>
    /// <param name="speedKPH">Vehicle speed.</param>
    /// <param name="isBraking">True if the vehicle is currently braking, false otherwise.</param>
    private void PlayBrakeSound(float speedKPH, bool isBraking)
    {
        if (brakeAudio == null || brakeClip == null) return;

        if (isBraking)
        {
            if (!brakingSoundActive && speedKPH > 5f && Time.time - lastBrakeSoundTime >= brakeSoundCooldown)
            {
                brakeAudio.pitch = Mathf.Lerp(0.9f, 1.2f, speedKPH / maxSpeedKPH);
                brakeAudio.volume = 0.07f;
                brakeAudio.clip = brakeClip;
                brakeAudio.Play();
                brakingSoundActive = true;
                lastBrakeSoundTime = Time.time;
            }
        }
        else
        {
            // Resets on releasing break button
            brakingSoundActive = false;
        }

    }

    /// <summary>
    /// Enables excavator engine sound.
    /// </summary>
    public void EnableEngineSound()
    {
        engineAudio.Play();
    }

    /// <summary>
    /// Disables excavator engine sound.
    /// </summary>
    public void DisableEngineSound()
    {
        engineAudio.Stop();
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

