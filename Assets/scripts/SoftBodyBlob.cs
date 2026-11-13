
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.InputSystem.XR;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer),typeof(EyeTracking))]
public class SoftBodyBlob : MonoBehaviour
{
    [Header("Blob Shape")]
    public float radius = 0.75f;
    [Range(8, 32)] public int segments = 16;
    [Range(4, 16)] public int rings = 8;

    [Header("Physics")]
    public float stiffness = 0.85f;
    public float volumeStiffness = 0.8f;
    public float damping = 0.99f;
    public float particleMass = 1f;
    public int solverIterations = 6;

    [Header("Collision")]
    public float particleRadius = 0.06f;

    [Header("controller")]
    [SerializeField] private PlayerController playerController;

    private GameObject collisionProbeObject;
    private SphereCollider collisionProbe;

    class Constraint
    {
        public int a, b;
        public float restLength;
    }

    public Vector3[] positions;
    Vector3[] previous;
    Vector3[] original;
    Vector3[] normals;
    int[] triangles;

    List<Constraint> constraints = new();
    Mesh mesh;

    float originalVolume;
    public bool grounded;
    public bool copyable;

    public GameObject selectedObject;

    void Start()
    {
        collisionProbeObject = new GameObject("Blob Collision Probe");
        collisionProbeObject.hideFlags = HideFlags.HideAndDontSave;

        collisionProbe = collisionProbeObject.AddComponent<SphereCollider>();
        collisionProbe.radius = particleRadius;
        collisionProbe.isTrigger = true;
        CreateBlob();
    }

    void Update()
    {
    }

    void FixedUpdate()
    {
        if (positions == null) return;

        float dt = Time.fixedDeltaTime;

        // Integrate particle movement
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 velocity =
            (positions[i] - previous[i]) * damping;

            previous[i] = positions[i];

            Vector3 acceleration;

            //since velocity is calculated differently on the soft body then the Rigidbody(verlet vs linear)
            //the soft bodys velocity can be 0.2 while moving just as fast as the Rigidbody at something more like 5
            //not a huge issue as the softbody doesn't have to worry about weird bouncing but something to note
            if(velocity.magnitude <= playerController.maxSpeed)
            {
                acceleration =
                    Vector3.down * playerController.gravity +
                    playerController.MovementInput * playerController.moveForce;
            }
            else
            {
                acceleration =
                    Vector3.down * playerController.gravity;
            }

            //Debug.Log("Velocity: " + velocity + " acceleration " + acceleration);

            positions[i] += velocity + acceleration * dt * dt;
        }

        // Jump
        if (playerController.JumpRequested)
        {
            if (grounded)
            {
                for (int i = 0; i < previous.Length; i++)
                {
                    previous[i] -= Vector3.up * playerController.jumpForce * dt;
                }   
            }
            playerController.JumpRequested = false;
        }

        // Copy
        if (playerController.CopyRequested)
        {
            if(copyable)
            {
                //HideBlob();
                playerController.ShapeShift(gameObject, selectedObject, false);
                Debug.Log("touching copyable object");
            }
            playerController.CopyRequested = false;
        }

        // Reform, don't do anything if blob mode
        if (playerController.ReformRequested)
        {
            playerController.ReformRequested = false;
        }

        // Solve constraints
        for (int iteration = 0;
             iteration < solverIterations;
             iteration++)
        {
            SolveDistanceConstraints();
            SolveVolumeConstraint();
            SolveGroundCollision();
            SolveCopyableCollision();
            SolveObstacleCollisions();
        }

        RecenterBlob();
        UpdateMesh();
    }

    public void SetVelocity(Vector3 velocity)
    {
        Vector3 localVelocity = transform.InverseTransformVector(velocity);

        for (int i = 0; i < positions.Length; i++)
        {
            previous[i] = positions[i] -
                (localVelocity / damping) * Time.fixedDeltaTime;
        }
    }

    
    public void HideBlob()
    {
        //blobHidden = true;

        // Hide the rendered mesh
        GetComponent<MeshRenderer>().enabled = false;

        // Stop the physics simulation
        enabled = false;

        // Disable any colliders attached to the blob
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }

        GetComponent<EyeTracking>().enabled = false;
        // eyes.leftEye.GetComponent<MeshRenderer>().enabled = false;
        // eyes.rightEye.GetComponent<MeshRenderer>().enabled = false;


        // Disable the collision probe
        if (collisionProbe != null)
            collisionProbe.enabled = false;
    }

    public void ShowBlob()
    {
        GetComponent<MeshRenderer>().enabled = true;

        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = true;
        }

        GetComponent<EyeTracking>().enabled = true;

        if (collisionProbe != null)
            collisionProbe.enabled = true;

        enabled = true;
    }

    void CreateBlob()
    {
        List<Vector3> verts = new();
        List<int> tris = new();

        // Top pole
        verts.Add(Vector3.up * radius);

        // Latitude rings
        for (int r = 1; r < rings; r++)
        {
            float phi = Mathf.PI * r / rings;

            for (int s = 0; s < segments; s++)
            {
                float theta = 2f * Mathf.PI * s / segments;

                Vector3 p = new Vector3(
                    Mathf.Sin(phi) * Mathf.Cos(theta),
                    Mathf.Cos(phi),
                    Mathf.Sin(phi) * Mathf.Sin(theta)
                ) * radius;

                verts.Add(p);
            }
        }

        // Bottom pole
        int bottom = verts.Count;
        verts.Add(Vector3.down * radius);

        // Top triangles
        for (int s = 0; s < segments; s++)
        {
            tris.Add(0);
            tris.Add(1 + s);
            tris.Add(1 + (s + 1) % segments);
        }

        // Ring connections
        for (int r = 0; r < rings - 2; r++)
        {
            for (int s = 0; s < segments; s++)
            {
                int a = 1 + r * segments + s;
                int b = 1 + r * segments +
                    (s + 1) % segments;
                int c = a + segments;
                int d = b + segments;

                tris.Add(a);
                tris.Add(c);
                tris.Add(b);

                tris.Add(b);
                tris.Add(c);
                tris.Add(d);
            }
        }

        // Bottom triangles
        int lastRing = 1 + (rings - 2) * segments;

        for (int s = 0; s < segments; s++)
        {
            tris.Add(lastRing + s);
            tris.Add(bottom);
            tris.Add(lastRing + (s + 1) % segments);
        }

        // Ensure consistent outward-facing triangle winding
        for (int i = 0; i < tris.Count; i += 3)
        {
            int temp = tris[i + 1];
            tris[i + 1] = tris[i + 2];
            tris[i + 2] = temp;
        }

        positions = verts.ToArray();
        original = verts.ToArray();
        previous = verts.ToArray();
        triangles = tris.ToArray();

        mesh = new Mesh();
        mesh.name = "SoftBodyMesh";
        mesh.vertices = positions;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        // Generate unique edge constraints
        HashSet<string> edgeSet = new();

        for (int i = 0; i < triangles.Length; i += 3)
        {
            AddEdge(triangles[i], triangles[i + 1], edgeSet);
            AddEdge(triangles[i + 1], triangles[i + 2], edgeSet);
            AddEdge(triangles[i + 2], triangles[i], edgeSet);
        }

        originalVolume = CalculateVolume();

        Debug.Log("Blob created with " +
            positions.Length + " particles.");
    }

    void AddEdge(int a, int b, HashSet<string> edgeSet)
    {
        int min = Mathf.Min(a, b);
        int max = Mathf.Max(a, b);
        string key = min + "_" + max;

        if (!edgeSet.Add(key)) return;

        constraints.Add(new Constraint
        {
            a = min,
            b = max,
            restLength = Vector3.Distance(
                positions[min], positions[max])
        });
    }

    void SolveDistanceConstraints()
    {
        foreach (Constraint c in constraints)
        {
            Vector3 delta =
                positions[c.b] - positions[c.a];

            float length = delta.magnitude;
            if (length < 0.0001f) continue;

            Vector3 correction =
                delta.normalized *
                (length - c.restLength) *
                stiffness * 0.5f;

            positions[c.a] += correction;
            positions[c.b] -= correction;
        }
    }

    float CalculateVolume()
    {
        float volume = 0f;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3 a = positions[triangles[i]];
            Vector3 b = positions[triangles[i + 1]];
            Vector3 c = positions[triangles[i + 2]];

            volume += Vector3.Dot(
                a, Vector3.Cross(b, c)) / 6f;
        }

        return volume;
    }

    void SolveVolumeConstraint()
    {
        float volume = CalculateVolume();
        float error = volume - originalVolume;

        Vector3[] gradients = new Vector3[positions.Length];

        for (int i = 0; i < triangles.Length; i += 3)
        {
            int ia = triangles[i];
            int ib = triangles[i + 1];
            int ic = triangles[i + 2];

            Vector3 a = positions[ia];
            Vector3 b = positions[ib];
            Vector3 c = positions[ic];

            gradients[ia] += Vector3.Cross(b, c) / 6f;
            gradients[ib] += Vector3.Cross(c, a) / 6f;
            gradients[ic] += Vector3.Cross(a, b) / 6f;
        }

        float denominator = 0f;

        foreach (Vector3 g in gradients)
            denominator += g.sqrMagnitude;

        if (denominator < 0.000001f) return;

        float lambda =
            -error * volumeStiffness / denominator;

        for (int i = 0; i < positions.Length; i++)
            positions[i] += gradients[i] * lambda;
    }

    void SolveGroundCollision()
    {
        grounded = false;

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 worldPosition =
                transform.TransformPoint(positions[i]);

            Collider[] nearby = Physics.OverlapSphere(
                worldPosition,
                particleRadius
            );

            foreach (Collider other in nearby)
            {
                if (!other.CompareTag("Ground"))
                    continue;

                grounded = true;
                break;
            }
        }
    }

    void SolveCopyableCollision()
    {
        copyable = false;
        selectedObject = null;

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 worldPosition =
                transform.TransformPoint(positions[i]);

            Collider[] nearby = Physics.OverlapSphere(
                worldPosition,
                particleRadius
            );

            foreach (Collider other in nearby)
            {
                if (!other.CompareTag("copyable"))
                    continue;

                copyable = true;
                selectedObject = other.gameObject;

                // Material currentMat = gameObject.GetComponent<MeshRenderer>().materials[1];
                // selectedObject.GetComponent<MeshRenderer>().materials = 
                //     selectedObject.GetComponent<MeshRenderer>().materials.Append(currentMat).ToArray();

                break;
            }
        }
    }

    void UpdateMesh()
    {
        mesh.vertices = positions;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }


    void RecenterBlob()
    {
        Vector3 localCenter = Vector3.zero;

        for (int i = 0; i < positions.Length; i++)
            localCenter += positions[i];

        localCenter /= positions.Length;

        // Convert the local centre into a world-space offset
        Vector3 worldOffset = transform.TransformVector(localCenter);

        // Move the controller to the blob's centre
        transform.position += worldOffset;

        // Keep the rendered blob in exactly the same world position
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] -= localCenter;
            previous[i] -= localCenter;
        }
    }

    public Vector3 GetBlobCenter()
    {
        if (positions == null || positions.Length == 0)
            return transform.position;

        Vector3 center = Vector3.zero;

        for (int i = 0; i < positions.Length; i++)
            center += transform.TransformPoint(positions[i]);

        return center / positions.Length;
    }

    void SolveObstacleCollisions()
    {
        if (collisionProbe == null)
            return;

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 worldPosition = transform.TransformPoint(positions[i]);

            collisionProbeObject.transform.position = worldPosition;

            Collider[] nearby = Physics.OverlapSphere(
                worldPosition,
                particleRadius
            );

            foreach (Collider other in nearby)
            {
                if (other == collisionProbe || other.isTrigger == true)
                    continue;

                if (other.transform == transform ||
                    other.transform.IsChildOf(transform))
                    continue;

                bool penetrating = Physics.ComputePenetration(
                    collisionProbe,
                    collisionProbeObject.transform.position,
                    collisionProbeObject.transform.rotation,
                    other,
                    other.transform.position,
                    other.transform.rotation,
                    out Vector3 direction,
                    out float distance
                );

                if (penetrating)
                {
                    worldPosition += direction * distance;
                    collisionProbeObject.transform.position = worldPosition;
                }
            }

            positions[i] = transform.InverseTransformPoint(worldPosition);
        }
    }

    void OnDestroy()
    {
        if (collisionProbeObject != null)
            Destroy(collisionProbeObject);
    }
}
