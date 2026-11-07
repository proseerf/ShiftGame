
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField]
    public Camera camera1 { get; private set; }

    [field: SerializeField]
    public Camera camera2 { get; private set; }

     [Header("Movement")]
     public float moveForce = 12f;
     public float jumpForce = 5f;

    // Movement input supplied to SoftBodyBlob
    public Vector3 MovementInput;

    public bool JumpRequested { get; private set; }

    // Called by SoftBodyBlob after a jump has been processed
    public void ConsumeJumpRequest()
    {
        JumpRequested = false;
    }

    void Start()
    {
        camera1.enabled = true;
        camera2.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CameraSwap"))
        {
            SwapCamera();
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CameraSwap"))
        {
            SwapCamera();
        }
    }

    public void CreateStaticCopy()
    {
        // Create a copy of the current GameObject
        GameObject copy = Instantiate(
            gameObject,
            transform.position,
            transform.rotation
        );

        // Keep the same scale
        copy.transform.localScale = transform.lossyScale;

        // Remove everything except the mesh and renderer
        Component[] components = copy.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component is Transform ||
                component is MeshFilter ||
                component is MeshRenderer)
            {
                continue;
            }

            Destroy(component);
        }

        // Mark as static
        copy.isStatic = true;
    }

    void Update()
    {
        // Static copy
        if (Input.GetKeyDown(KeyCode.X))
        {
            CreateStaticCopy();
        }

        // Read movement input
        MovementInput = Vector3.zero;

        if (Input.GetKey(KeyCode.D))
            MovementInput.x += 1f;

        if (Input.GetKey(KeyCode.A))
            MovementInput.x -= 1f;

        if (Input.GetKey(KeyCode.W))
            MovementInput.z += 1f;

        if (Input.GetKey(KeyCode.S))
            MovementInput.z -= 1f;

        // if (Input.GetKey(KeyCode.Space))
        //     MovementInput.y += 1f;

        if (Input.GetKey(KeyCode.LeftControl))
            MovementInput.y -= 1f;

        MovementInput = Vector3.ClampMagnitude(MovementInput, 1f);

        // Request a jump once, rather than continuously
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpRequested = true;
        }
    }

    void SwapCamera()
    {
        camera1.enabled = !camera1.enabled;
        camera2.enabled = !camera2.enabled;
    }
}
