using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float targetHeight = 1f;

    public float mouseSensitivity = 3f;
    public float minPitch = -20f;
    public float maxPitch = 70f;

    public float minDistance = 2f;
    public float maxDistance = 12f;
    public float zoomSpeed = 4f;

    public float smoothSpeed = 10f;

    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.2f;

    float yaw;
    float pitch;
    float distance;
    Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        float startDistance = offset.magnitude;
        distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        
        Vector3 dir = -offset.normalized;
        yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(Mathf.Clamp(-dir.y, -1f, 1f)) * Mathf.Rad2Deg;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        if (cam != null && !cam.enabled)
            return;

        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        distance -= Input.mouseScrollDelta.y * zoomSpeed * 0.1f * distance;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 focus = target.position + Vector3.up * targetHeight;
        Vector3 desiredPosition = focus - rotation * Vector3.forward * distance;

        // Check for camera colliding with enviroment
        Vector3 toCamera = desiredPosition - focus;
        bool hitSomething = Physics.SphereCast(
            focus,
            collisionRadius,
            toCamera.normalized,
            out RaycastHit hit,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (hitSomething && !hit.transform.IsChildOf(target))
        {
            desiredPosition = focus + toCamera.normalized * Mathf.Max(hit.distance - 0.1f, 0.5f);
        }

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );
        transform.rotation = Quaternion.LookRotation(focus - transform.position);
    }
}