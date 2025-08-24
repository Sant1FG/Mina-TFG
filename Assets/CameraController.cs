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

    void Awake()
    {
        cam = GetComponent<Camera>();
        if (player) rb = player.GetComponent<Rigidbody>();
        if (rb) rb.interpolation = RigidbodyInterpolation.Interpolate;

        // inicializa objetivos para evitar “salto” al primer frame
        if (player)
        {
            targetYaw = player.eulerAngles.y;
            wantedHeight = player.position.y + height;
            transform.position = player.position - Quaternion.Euler(0, targetYaw, 0) * Vector3.forward * distance;
            var p = transform.position; p.y = wantedHeight; transform.position = p;
            transform.rotation = Quaternion.LookRotation((player.position + Vector3.up * lookHeight) - transform.position, Vector3.up);
            cam.fieldOfView = defaultFOV;
        }
    }

    void LateUpdate()
    {
        if (!player) return;

        // 1) Dirección del vehículo + detección de marcha atrás (estable)
        Vector3 forward = player.forward;
        Vector3 vel = rb ? rb.linearVelocity : Vector3.zero;
        bool reversing = Vector3.Dot(forward, vel) < -0.1f;

        float desiredYaw = player.eulerAngles.y + (reversing ? 180f : 0f);
        targetYaw = Mathf.SmoothDampAngle(targetYaw, desiredYaw, ref yawVel, 1f / Mathf.Max(0.0001f, rotationDamping));

        float desiredHeight = player.position.y + height;
        float currentHeight = transform.position.y;
        float newHeight = Mathf.SmoothDamp(currentHeight, desiredHeight, ref heightVel, 1f / Mathf.Max(0.0001f, heightDamping));

        // 2) Posición detrás del vehículo a distancia fija, con yaw suavizado
        Quaternion yawRot = Quaternion.Euler(0f, targetYaw, 0f);
        Vector3 desiredPos = player.position - (yawRot * Vector3.forward * distance);
        desiredPos.y = newHeight;

        transform.position = desiredPos;

        // 3) Mirar a un punto estable del vehículo (evita vibración por ruedas/colisiones)
        Vector3 lookTarget = player.position + Vector3.up * lookHeight;
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);

        // 4) FOV dinámico estable (sin deltaTime), con suavizado
        float speed = vel.magnitude;
        float desiredFOV = defaultFOV + speed * zoomRatio; // mapea velocidad a FOV
        cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, desiredFOV, ref fovVel, 0.15f);
    }
}
