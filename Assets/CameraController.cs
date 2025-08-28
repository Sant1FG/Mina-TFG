using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;
    public float distance = 6.4f;
    public float height = 1.4f;
    public float rotationDamping = 3.0f;   // mayor = más suave
    public float heightDamping = 2.0f;
    public float zoomRatio = 0.5f;         // efecto “speed zoom”
    public float defaultFOV = 60f;
    public float lookHeight = 1.2f;        // punto al que la cámara mira en el vehículo

    Rigidbody rb;
    Camera cam;

    // estados para SmoothDamp
    float yawVel;          // ref para SmoothDampAngle
    float heightVel;       // ref para SmoothDamp
    float fovVel;          // ref para SmoothDamp del FOV
    float targetYaw;       // yaw deseado (suavizado)
    float wantedHeight;    // altura deseada (suavizado)

    public void SetPlayer(Transform playerTransform)
{
    if (playerTransform == null)
    {
        player = null;
        rb = null;
        return;
    }

    player = playerTransform;

    // Cachea la cámara si aún no lo hiciste (por si vienes de prefab recién instanciado)
    if (cam == null) cam = GetComponent<Camera>();

    // Rigidbody del player (si existe)
    rb = player.GetComponent<Rigidbody>();
    if (rb) rb.interpolation = RigidbodyInterpolation.Interpolate;

    // Snap inicial para evitar “tirón” en el primer frame
    targetYaw    = player.eulerAngles.y;
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

    void LateUpdate()
{
    if (!player || !rb) return;

    // 1) Dirección del vehículo + detección de marcha atrás (estable)
    Vector3 forward = player.forward;
    Vector3 vel = rb ? rb.linearVelocity : Vector3.zero;
    bool reversing = Vector3.Dot(forward, vel) < -0.1f;

    float desiredYaw = player.eulerAngles.y + (reversing ? 180f : 0f);
    targetYaw = Mathf.SmoothDampAngle(targetYaw, desiredYaw, ref yawVel, 1f / Mathf.Max(0.0001f, rotationDamping));
    if (float.IsNaN(targetYaw) || float.IsInfinity(targetYaw)) targetYaw = desiredYaw;

    float desiredHeight = player.position.y + height;
    float currentHeight = transform.position.y;

    // 👇 Guardas anti-NaN/∞
    float newHeight = Mathf.SmoothDamp(currentHeight, desiredHeight, ref heightVel, 1f / Mathf.Max(0.0001f, heightDamping));
    if (float.IsNaN(newHeight) || float.IsInfinity(newHeight)) newHeight = desiredHeight;

    // 2) Posición detrás del vehículo a distancia fija, con yaw suavizado
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

    // 3) Mirar a un punto estable del vehículo (evita vibración por ruedas/colisiones)
    Vector3 lookTarget = player.position + Vector3.up * lookHeight;
    Vector3 lookDir = lookTarget - transform.position;
    if (!float.IsNaN(lookDir.x) && !float.IsNaN(lookDir.y) && !float.IsNaN(lookDir.z) &&
        !float.IsInfinity(lookDir.x) && !float.IsInfinity(lookDir.y) && !float.IsInfinity(lookDir.z))
    {
        transform.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
    }

    // 4) FOV dinámico estable (sin deltaTime), con suavizado
    float speed = vel.magnitude;
    float desiredFOV = defaultFOV + speed * zoomRatio; // mapea velocidad a FOV
    float fov = Mathf.SmoothDamp(cam.fieldOfView, desiredFOV, ref fovVel, 0.15f);
    if (float.IsNaN(fov) || float.IsInfinity(fov)) fov = desiredFOV;
    cam.fieldOfView = Mathf.Clamp(fov, 1f, 179f);
}
    
    void OnDestroy()
    {
        player = null;
        rb = null;
    }
}
