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

        Rigidbody rb = GetComponent<Rigidbody>();

        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 acceleration; 

        if(flatVelocity.magnitude >= playerController.maxSpeed)
        {
            acceleration =
                Vector3.down * playerController.gravity;
        }

        else
        {
            acceleration =
                Vector3.down * playerController.gravity +
                playerController.MovementInput * playerController.moveForce;
        }


        // Movement and gravity
        rb.AddForce(acceleration, ForceMode.Acceleration);

        //Debug.Log(flatVelocity.magnitude);

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
        
        if(playerController.CloneRequested)
        {
            playerController.CreateStaticCopy(gameObject, transform);
        }

        if(playerController.TeleportRequested != 0)
        {
            if(playerController.TeleportRequested == 1)
            {
                playerController.buttonLocation = transform.position;
                transform.position = playerController.theatreLocation;
            }
            if(playerController.TeleportRequested == 2)
            {
                transform.position = playerController.buttonLocation;
            }
            playerController.TeleportRequested = 0;
            playerController.ChangeViews();
        }

        // Copy
        if (playerController.CopyRequested)
        {
            if(copyable)
            {
                reform(rb);
                //playerController.CopyRequested = false;
                //playerController.ShapeShift(gameObject,selectedObject);
            }
            playerController.CopyRequested = false;
        }

        // Reform
        if (playerController.ReformRequested)
        {
            reform(rb);
        }
    }

    void reform(Rigidbody rb)
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

            blob.selectedObject = selectedObject;

            // Show blob
            blob.ShowBlob();

            playerController.ReformRequested = false;
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

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CameraSwap"))
        {
            playerController.isCameraSwappable = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CameraSwap"))
        {
            playerController.isCameraSwappable = false;
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
