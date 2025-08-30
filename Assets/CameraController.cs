using UnityEngine;

/// <summary>
/// Camara tasked with following the player through the scenario.
/// Configurable distance/height, damps and height changes, flips when reversing and 
/// increases FOV with speed.
/// </summary>
public class CameraController : MonoBehaviour
{
    public Transform player;
    public float distance = 6.4f;
    public float height = 1.4f;
    public float rotationDamping = 3.0f;
    public float heightDamping = 2.0f;
    public float zoomRatio = 0.5f;
    public float defaultFOV = 60f;
    public float lookHeight = 1.2f;

    Rigidbody rb;
    Camera cam;

    //SmoothDamp
    float yawVel;
    float heightVel;
    float fovVel;
    float targetYaw;
    float wantedHeight;

    /// <summary>
    /// Binds the camera to the players transform and snaps the camera to the initial follow pose.
    /// </summary>
    /// <param name="playerTransform">Transform of the player to follow.</param>
    public void SetPlayer(Transform playerTransform)
    {
        if (playerTransform == null)
        {
            player = null;
            rb = null;
            return;
        }

        player = playerTransform;

        if (cam == null) cam = GetComponent<Camera>();

        // Rigidbody from player
        rb = player.GetComponent<Rigidbody>();
        if (rb) rb.interpolation = RigidbodyInterpolation.Interpolate;

        targetYaw = player.eulerAngles.y;
        wantedHeight = player.position.y + height;

        Vector3 pos = player.position - Quaternion.Euler(0f, targetYaw, 0f) * Vector3.forward * distance;
        pos.y = wantedHeight;
        transform.position = pos;

        transform.rotation = Quaternion.LookRotation(
            (player.position + Vector3.up * lookHeight) - transform.position,
            Vector3.up
        );

        if (cam) cam.fieldOfView = defaultFOV;
    }

    /// <summary>
    /// Called by Unity after Update()
    /// Damps yaw and height, flips view when reversing, aims at the player, and applies speed-based FOV.
    /// </summary>
    void LateUpdate()
    {
        if (!player || !rb) return;

        Vector3 forward = player.forward;
        Vector3 vel = rb ? rb.linearVelocity : Vector3.zero;
        bool reversing = Vector3.Dot(forward, vel) < -0.1f;

        float desiredYaw = player.eulerAngles.y + (reversing ? 180f : 0f);
        targetYaw = Mathf.SmoothDampAngle(targetYaw, desiredYaw, ref yawVel, 1f / Mathf.Max(0.0001f, rotationDamping));
        if (float.IsNaN(targetYaw) || float.IsInfinity(targetYaw)) targetYaw = desiredYaw;

        float desiredHeight = player.position.y + height;
        float currentHeight = transform.position.y;

        float newHeight = Mathf.SmoothDamp(currentHeight, desiredHeight, ref heightVel, 1f / Mathf.Max(0.0001f, heightDamping));
        if (float.IsNaN(newHeight) || float.IsInfinity(newHeight)) newHeight = desiredHeight;

        Quaternion yawRot = Quaternion.Euler(0f, targetYaw, 0f);
        Vector3 desiredPos = player.position - (yawRot * Vector3.forward * distance);
        desiredPos.y = newHeight;
        if (float.IsNaN(desiredPos.x) || float.IsNaN(desiredPos.y) || float.IsNaN(desiredPos.z) ||
            float.IsInfinity(desiredPos.x) || float.IsInfinity(desiredPos.y) || float.IsInfinity(desiredPos.z))
        {
            desiredPos = transform.position; // fallback seguro
            desiredPos.y = newHeight;
        }

        transform.position = desiredPos;

        Vector3 lookTarget = player.position + Vector3.up * lookHeight;
        Vector3 lookDir = lookTarget - transform.position;
        if (!float.IsNaN(lookDir.x) && !float.IsNaN(lookDir.y) && !float.IsNaN(lookDir.z) &&
            !float.IsInfinity(lookDir.x) && !float.IsInfinity(lookDir.y) && !float.IsInfinity(lookDir.z))
        {
            transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
        }

        float speed = vel.magnitude;
        float desiredFOV = defaultFOV + speed * zoomRatio;
        float fov = Mathf.SmoothDamp(cam.fieldOfView, desiredFOV, ref fovVel, 0.15f);
        if (float.IsNaN(fov) || float.IsInfinity(fov)) fov = desiredFOV;
        cam.fieldOfView = Mathf.Clamp(fov, 1f, 179f);
    }

    /// <summary>
    /// Snaps camera to a fixed anchor position and looks at the target.
    /// </summary>
    /// <param name="anchor">World position the camera moves to.</param>
    /// <param name="target">Object the camera looks at.</param>
    public void SetFixedPose(Transform anchor, Transform target)
    {
        if (anchor == null || target == null) return;

        transform.position = anchor.position;

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

        targetYaw = target.eulerAngles.y;
        if (cam == null) cam = GetComponent<Camera>();
        if (cam) cam.fieldOfView = defaultFOV;
    }

    /// <summary>
    /// Called by Unity when controller is Destroyed. Clears all references.
    /// </summary>
    void OnDestroy()
    {
        player = null;
        rb = null;
    }
}
