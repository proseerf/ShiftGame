using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [field: SerializeField]    
    public Camera camera1 { get; private set; }
    [field: SerializeField]
    public Camera camera2 { get; private set; }
    [field: SerializeField]
    public float speed { get; private set; }

    [field: SerializeField]
    Vector3 theatreLocation = new Vector3(0,0,0);

    Vector3 buttonLocation = new Vector3(0,0,0);

    bool isCameraSwappable = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera1.enabled = true;
        camera2.enabled = false;

    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "CameraSwap")
        {
            isCameraSwappable = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.tag == "CameraSwap")
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

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.X) && camera2.enabled == true)
        {
            CreateStaticCopy();
            swapCamera();
            returnToButton();
        }
        if(Input.GetKeyDown(KeyCode.Tab) && isCameraSwappable)
        {
            swapCamera();
            if(camera2.enabled == true)
            {
                goToTheatreDisplay();
            }
            else
            {
                returnToButton();
            }
        }

    }
    
    void FixedUpdate()
    {

        // Movement inputs
        if(Input.GetKey(KeyCode.D))
        {
            transform.Translate(speed, 0, 0);
        }
        if(Input.GetKey(KeyCode.A))
        {
            transform.Translate(-speed, 0, 0);
        }
        if(Input.GetKey(KeyCode.W))
        {
            transform.Translate(0, 0, speed);
        }
        if(Input.GetKey(KeyCode.S))
        {
            transform.Translate(0, 0, -speed);
        }
        if(Input.GetKey(KeyCode.Space))
        {
            transform.Translate(0, speed, 0);
        }
        if(Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(0, -speed, 0);
        }

        // Gravity
    }


    void swapCamera()
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

    void resolveGravity()
    {
    }
}
