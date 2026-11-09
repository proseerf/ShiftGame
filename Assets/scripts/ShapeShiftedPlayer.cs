using System.Reflection.Metadata;
using UnityEngine;
using System.Collections.Generic;

public class ShapeShiftedPlayer : MonoBehaviour
{
    public bool grounded;
    public bool copyable;

    private HashSet<Collider> groundContacts = new HashSet<Collider>();
    private HashSet<Collider> copyableContacts = new HashSet<Collider>();

    public GameObject selectedObject;

    [Header("controller")]
    [SerializeField] public PlayerController playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 acceleration =
            Vector3.down * playerController.gravity +
            playerController.MovementInput * playerController.moveForce;

        // Movement and gravity
        rb.AddForce(acceleration, ForceMode.Acceleration);

        // Jump
        if (playerController.JumpRequested)
        {
            if (grounded)
            {
                Vector3 jump = Vector3.up * playerController.jumpForce;

                rb.AddForce(jump, ForceMode.VelocityChange);
            }

            playerController.JumpRequested = false;
        }

        // Copy
        if (playerController.CopyRequested)
        {
            if(copyable)
            {
                playerController.ShapeShift(gameObject,selectedObject);
            }
            playerController.CopyRequested = false;
        }

        // Reform
        if (playerController.ReformRequested)
        {
            // Save the current Rigidbody velocity BEFORE destroying the object
            Vector3 currentVelocity = rb.linearVelocity;

            // Position the blob where the shapeshifted object is
            playerController.blobObject.transform.position = transform.position;
            //playerController.blobObject.transform.rotation = transform.rotation;
            playerController.blobObject.transform.localScale = transform.localScale;

            // Transfer velocity to the blob
            SoftBodyBlob blob =
                playerController.blobObject.GetComponent<SoftBodyBlob>();

            Debug.Log("setting velocity: " + rb.linearVelocity);
            blob.SetVelocity(rb.linearVelocity);

            // Switch camera target
            Camera.main.GetComponent<CameraFollow>().target =
                playerController.blobObject.transform;

            // Destroy shapeshifted object
            Destroy(gameObject);

            // Show blob
            blob.ShowBlob();

            playerController.ReformRequested = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        Collider other = collision.collider;

        if (other.CompareTag("Ground"))
        {
            groundContacts.Add(other);
            grounded = true;
        }

        if (other.CompareTag("copyable"))
        {
            copyableContacts.Add(other);
            UpdateSelectedObject();
        }
    }

    void OnCollisionExit(Collision collision)
    {
        Collider other = collision.collider;

        if (other.CompareTag("Ground"))
        {
            groundContacts.Remove(other);
            grounded = groundContacts.Count > 0;
        }

        if (other.CompareTag("copyable"))
        {
            copyableContacts.Remove(other);
            UpdateSelectedObject();
        }
    }

    void UpdateSelectedObject()
    {
        copyableContacts.RemoveWhere(c => c == null);

        copyable = copyableContacts.Count > 0;

        if (copyable)
        {
            selectedObject = null;

            foreach (Collider col in copyableContacts)
            {
                selectedObject = col.gameObject;
                break;
            }
        }
        else
        {
            selectedObject = null;
        }
    }

    void returnToBlob()
    {
        
    }
}
