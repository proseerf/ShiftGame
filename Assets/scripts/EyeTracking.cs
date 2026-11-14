
using UnityEngine;

public class EyeTracking : MonoBehaviour
{
    private string targetTag = "copyable";

    [Header("Eye References")]
    public Transform leftEye;
    public Transform rightEye;

    [Header("Body")]
    public Transform eyeAnchor;
    public Collider bodyCollider;
    public float eyeSurfaceOffset = 0.1f;

    [Header("Eye Settings")]
    public float eyeSpacing = 0.3f;
    public float movementSpeed = 5f;

    private Transform closestTarget;
    private Vector3 previousAnchorPosition;

    void Start()
    {
        previousAnchorPosition = eyeAnchor.position;
    }

    void Update()
    {
        FindClosestTarget();

        // Always account for anchor movement
        Vector3 anchorMovement = eyeAnchor.position - previousAnchorPosition;

        leftEye.position += anchorMovement;
        rightEye.position += anchorMovement;

        previousAnchorPosition = eyeAnchor.position;

        if (closestTarget == null) return;

        MoveEyes();
    }

    void FindClosestTarget()
    {
        //get objects with the copyable tag
        GameObject[] targets = GameObject.FindGameObjectsWithTag(targetTag);

        float closestDistance = Mathf.Infinity;
        closestTarget = null;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(
                eyeAnchor.position,
                target.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.transform;
            }
        }
    }

    //works out how far from the center of the object the eyes should be
    //that being the edge of the collider minus the offset
    float CalculateEyeDistance(Vector3 direction)
    {
        Bounds bounds = bodyCollider.bounds;

        Vector3 extents = bounds.extents;

        float distance =
            Mathf.Abs(direction.x) * extents.x +
            Mathf.Abs(direction.y) * extents.y +
            Mathf.Abs(direction.z) * extents.z;

        return Mathf.Max(0f, distance - eyeSurfaceOffset);
    }

    //keeps the eyes inside the object
    Vector3 ConstrainEyePosition(Vector3 targetPosition)
    {
        Vector3 insidePoint = eyeAnchor.position;

        // If the target position is already inside, leave it alone
        if (bodyCollider.ClosestPoint(targetPosition) == targetPosition)
            return targetPosition;


        //if it's not inside, then take the midpoint between the target and the closest inside point
        //if that midpoint is inside, take the midpoint between that and the target,
        //if it's not, take the midpoint between the inside point and the midpoint
        //doing this 12 times is kinda arbitrary but needs to be looped through to find the furthest spot that's still inside
        for (int i = 0; i < 12; i++)
        {
            Vector3 midpoint = (insidePoint + targetPosition) * 0.5f;

            if (bodyCollider.ClosestPoint(midpoint) == midpoint)
            {
                insidePoint = midpoint;
            }
            else
            {
                targetPosition = midpoint;
            }
        }

        return insidePoint;
    }

    void MoveEyes()
    {
        //get the direction to the taget
        Vector3 direction =
            (closestTarget.position - eyeAnchor.position).normalized;

        //get the direction directly to the right of that
        Vector3 right =
            Vector3.Cross(direction, Vector3.up).normalized;

        //fixed weirdness when looking up or down
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(direction, Vector3.forward).normalized;

        float eyeDistance = CalculateEyeDistance(direction);

        //figures out the left and right eye postions based on the set spacing
        Vector3 leftPosition = (
            direction * eyeDistance -
            right * (eyeSpacing / 2f)
        ).normalized * eyeDistance;

        Vector3 rightPosition = (
            direction * eyeDistance +
            right * (eyeSpacing / 2f)
        ).normalized * eyeDistance;

        //makes sure the eyes are "stuck" to the anchor
        leftPosition += eyeAnchor.position;
        rightPosition += eyeAnchor.position;

        // Move each eye towards its desired position
        leftEye.position = Vector3.MoveTowards(
            leftEye.position,
            leftPosition,
            movementSpeed * Time.deltaTime
        );

        rightEye.position = Vector3.MoveTowards(
            rightEye.position,
            rightPosition,
            movementSpeed * Time.deltaTime
        );

        // Independently constrain both eyes
        leftEye.position = ConstrainEyePosition(leftEye.position);
        rightEye.position = ConstrainEyePosition(rightEye.position);

        //lock the rotation, only really matters if we put proper textures on them
        leftEye.rotation = Quaternion.LookRotation(
            (leftEye.position - eyeAnchor.position).normalized
        );

        rightEye.rotation = Quaternion.LookRotation(
            (rightEye.position - eyeAnchor.position).normalized
        );
    }
}
