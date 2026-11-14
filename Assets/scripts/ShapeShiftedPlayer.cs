using System.Reflection.Metadata;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class ShapeShiftedPlayer : MonoBehaviour
{
    public bool grounded;
    public bool copyable;

    private HashSet<Collider> groundContacts = new HashSet<Collider>();
    private HashSet<Collider> copyableContacts = new HashSet<Collider>();

    public GameObject selectedObject;
    private bool inTheatre = false;

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

        if(!inTheatre)
        {
            //Backstage Movement

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
        }
        else
        {
            //Theatre movement
            Vector3 inputRotation = new Vector3(-90f, 180f, 0f);
            Vector3 spaceMovement = Quaternion.Euler(inputRotation) * playerController.MovementInput;

            Vector3 planarVelocity = new Vector3(rb.linearVelocity.x,  rb.linearVelocity.y, 0f);
            Vector3 acceleration = new Vector3(0f,0f,0f); 

            if(planarVelocity.magnitude <= playerController.theatreMaxSpeed)
            {
                acceleration = spaceMovement * playerController.theatreMoveForce;
            }

            // Try to slow movement
            Vector3 restitutionForce = planarVelocity *  playerController.theatreRestitutionForce;
            acceleration -= restitutionForce;

            rb.AddForce(acceleration, ForceMode.Acceleration);

            //Theatre Rotation

            float yaw = 0f;
            float pitch = 0f;

            float rotationSensitivity = 5f;

            if (Input.GetMouseButton(1))
            {
                yaw += Input.GetAxis("Mouse X") * rotationSensitivity;
                pitch -= Input.GetAxis("Mouse Y") * rotationSensitivity;
            }

            // Parent
            transform.Rotate(new Vector3(pitch, -yaw, 0f), Space.World);
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
            playerController.CopyRequested = false;
            if(copyable)
            {
                reform(rb);
                playerController.blobObject.GetComponent<SoftBodyBlob>().Copy(selectedObject);
                //playerController.CopyRequested = false;
                //playerController.ShapeShift(gameObject,selectedObject);
            }
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
            //Vector3 currentVelocity = rb.linearVelocity;

            // Position the blob where the shapeshifted object is
            playerController.blobObject.transform.position = transform.position;

            // Transfer velocity to the blob
            SoftBodyBlob blob =
                playerController.blobObject.GetComponent<SoftBodyBlob>();

            // Debug.Log("setting velocity: " + rb.linearVelocity);
            // blob.SetVelocity(rb.linearVelocity);

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
            if(other.name == "CameraSwapTriggerRoom")
            {
                setTheatreMode(true);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("CameraSwap"))
        {
            playerController.isCameraSwappable = false;
            if(other.name == "CameraSwapTriggerRoom")
            {
                setTheatreMode(false);
            }
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

    private void setTheatreMode(bool onOff)
    {
        inTheatre = onOff;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.freezeRotation = onOff;
    }

    void returnToBlob()
    {
        
    }
}
