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
    [SerializeField] private float lowSpeedThreshold = 6f;         // m/s (~21.6 km/h)
    [SerializeField] private float lowSpeedBoost = 1.6f;   
    private bool breakInput;
   
    private float steerAngle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();

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

