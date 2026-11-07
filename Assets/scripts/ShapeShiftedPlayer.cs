using UnityEngine;

public class ShapeShiftedPlayer : MonoBehaviour
{
    public bool grounded;

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
            
            playerController.ConsumeJumpRequest();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        grounded = false;
        if (collision.collider.CompareTag("Ground")){
            grounded = true;
        }
    }
    void returnToBlob()
    {
        
    }
}
