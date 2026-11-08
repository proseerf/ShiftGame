
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField]
    public Camera camera1 { get; private set; }

    [field: SerializeField]
    public Camera camera2 { get; private set; }

    [field: SerializeField]
    Vector3 theatreLocation = new Vector3(0,0,0);

    Vector3 buttonLocation = new Vector3(0,0,0);

    bool isCameraSwappable = false;
     [Header("Movement")]
    public float gravity = 9.81f;
     public float moveForce = 12f;
     public float jumpForce = 5f;

    // Movement input supplied to SoftBodyBlob
    public Vector3 MovementInput;

    public bool JumpRequested { get; private set; }

    public bool CopyRequested { get; private set; }

    // Called by SoftBodyBlob after a jump has been processed
    public void ConsumeJumpRequest()
    {
        JumpRequested = false;
    }

    // Called by SoftBodyBlob after a jump has been processed
    public void ConsumeCopyRequest()
    {
        CopyRequested = false;
    }

    void Start()
    {
        //user cam
        camera1.enabled = true;
        //theatre view cam
        camera2.enabled = false;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CameraSwap"))
        {
            isCameraSwappable = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CameraSwap"))
        {
            isCameraSwappable = false;
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
        // no update if paused
        if (PauseMenu.IsPaused)
        {
            MovementInput = Vector3.zero;
            return;
        }

        if(Input.GetKeyDown(KeyCode.X) && camera2.enabled == true)
        {
            CreateStaticCopy();
            SwapCamera();
            returnToButton();
        }
        if(Input.GetKeyDown(KeyCode.Tab) && isCameraSwappable)
        {
            SwapCamera();
            if(camera2.enabled == true)
            {
                goToTheatreDisplay();
            }
            else
            {
                returnToButton();
            }
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

        if (Input.GetKey(KeyCode.LeftControl))
            MovementInput.y -= 1f;

        MovementInput = Vector3.ClampMagnitude(MovementInput, 1f);
        
        // if using player cam (not in theatre view), move based on camera view not world view
        if (camera1 != null && camera1.enabled)
        {
            Vector3 camForward = camera1.transform.forward;
            Vector3 camRight = camera1.transform.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            MovementInput =
                camRight * MovementInput.x
                + camForward * MovementInput.z
                + Vector3.up * MovementInput.y;
        }

        // Request a jump once, rather than continuously
        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpRequested = true;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            CopyRequested = true;
        }
    }

    void SwapCamera()
    {
        camera1.enabled = !camera1.enabled;
        camera2.enabled = !camera2.enabled;
    }

    void goToTheatreDisplay()
    {
        buttonLocation = transform.position;
        transform.position = theatreLocation;
    }

    void returnToButton()
    {
        transform.position = buttonLocation;
    }

        public void ShapeShift(GameObject currentObject, GameObject targetObject)
    {
        if (targetObject == null)
            return;

        // Get the original mesh components
        MeshFilter copiedMeshFilter = targetObject.GetComponent<MeshFilter>();
        Collider copiedCollider = targetObject.GetComponent<Collider>();

        if (copiedMeshFilter == null)
            return;

        // Create a new object
        GameObject newObject = new GameObject(targetObject.name + "_Copy");

        // Copy transform
        newObject.transform.position = currentObject.transform.position;
        newObject.transform.rotation = targetObject.transform.rotation;
        newObject.transform.localScale = targetObject.transform.localScale;

        // Add mesh components
        MeshFilter newMeshFilter = newObject.AddComponent<MeshFilter>();
        MeshRenderer newMeshRenderer = newObject.AddComponent<MeshRenderer>();

        // Assign the same mesh and materials
        newMeshFilter.sharedMesh = copiedMeshFilter.sharedMesh;
        newMeshRenderer.sharedMaterials = currentObject.GetComponent<MeshRenderer>().sharedMaterials;

        ShapeShiftedPlayer newControls = newObject.AddComponent<ShapeShiftedPlayer>();
        newControls.grounded=true;
        newControls.playerController = this;

        Rigidbody rigidbody = newObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = false;


        Debug.Log("copied collider type:" + copiedCollider.GetType());
        // Add a collider of the same type
        if (copiedCollider != null)
        {
            Collider newCollider = newObject.AddComponent(copiedCollider.GetType()) as Collider;

            // Copy collider settings
            newCollider.isTrigger = copiedCollider.isTrigger;
            newCollider.sharedMaterial = copiedCollider.sharedMaterial;
            patternMatchCollider(copiedCollider,newCollider);
        }

        Camera.main.GetComponent<CameraFollow>().target = newObject.transform;
    }

    private void patternMatchCollider(Collider copiedCollider, Collider newCollider)
    {
        if (copiedCollider is BoxCollider originalBox &&
            newCollider is BoxCollider newBox)
        {
            newBox.center = originalBox.center;
            newBox.size = originalBox.size;
        }

        if (copiedCollider is SphereCollider originalSphere &&
            newCollider is SphereCollider newSphere)
        {
            newSphere.center = originalSphere.center;
            newSphere.radius = originalSphere.radius;
        }

        if (copiedCollider is CapsuleCollider originalCapsule &&
            newCollider is CapsuleCollider newCapsule)
        {
            newCapsule.center = originalCapsule.center;
            newCapsule.radius = originalCapsule.radius;
            newCapsule.height = originalCapsule.height;
            newCapsule.direction = originalCapsule.direction;
        }

        if (copiedCollider is MeshCollider originalMesh &&
            newCollider is MeshCollider newMesh)
        {
            newMesh.sharedMesh = originalMesh.sharedMesh;
            newMesh.convex = originalMesh.convex;
        }
    }
}
