using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float targetHeight = 1f;

    public float mouseSensitivityX = 2f;
    public float mouseSensitivityY = 1f;
    public float minPitch = -20f;
    public float maxPitch = 70f;

    public float minDistance = 2f;
    public float maxDistance = 12f;
    public float zoomSpeed = 4f;

    public float smoothSpeed = 0f;

    public LayerMask collisionMask = ~0;
    public float collisionRadius = 0.2f;

    float yaw;
    float pitch;
    float distance;
    Camera cam;

    Vector3 smoothFocus;

    void Start()
    {
        cam = GetComponent<Camera>();

        float startDistance = offset.magnitude;
        distance = Mathf.Clamp(startDistance, minDistance, maxDistance);
        
        Vector3 dir = -offset.normalized;
        yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        pitch = Mathf.Asin(Mathf.Clamp(-dir.y, -1f, 1f)) * Mathf.Rad2Deg;
    
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (target != null)
            smoothFocus = target.position + Vector3.up * targetHeight;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        if (cam != null && !cam.enabled)
            return;

        // mouse release + capture
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetMouseButtonDown(0) && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Edit cam via mouse if mouse is being captured
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            yaw += Input.GetAxis("Mouse X") * mouseSensitivityX;
            pitch -= Input.GetAxis("Mouse Y") * mouseSensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        distance -= Input.mouseScrollDelta.y * zoomSpeed * 0.1f * distance;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
/* smooth cam
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
*/

    //1:1 mouse cam motion#
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 focus = target.position + Vector3.up * targetHeight;
        smoothFocus = Vector3.Lerp(smoothFocus, focus, smoothSpeed * Time.deltaTime);

        Vector3 desiredPosition = smoothFocus - rotation * Vector3.forward * distance;

        // Check for camera colliding with enviroment
        Vector3 toCamera = desiredPosition - smoothFocus;
        bool hitSomething = Physics.SphereCast(
            smoothFocus,
            collisionRadius,
            toCamera.normalized,
            out RaycastHit hit,
            distance,
            collisionMask,
            QueryTriggerInteraction.Ignore
        );

        if (hitSomething && !hit.transform.IsChildOf(target))
        {
            desiredPosition = smoothFocus + toCamera.normalized * Mathf.Max(hit.distance - 0.1f, 0.5f);
        }

        transform.position = desiredPosition;
        transform.rotation = rotation;

    }
}