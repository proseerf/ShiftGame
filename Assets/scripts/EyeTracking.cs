
using UnityEngine;

public class EyeTracking : MonoBehaviour
{
    //[Header("Target Settings")]
    private string targetTag = "copyable";

    [Header("Eye References")]
    public Transform leftEye;
    public Transform rightEye;

    [Header("Movement Settings")]
    public float eyeDistance = 0.75f;
    public float eyeSpacing = 0.3f;
    public float movementSpeed = 5f;

    private Transform closestTarget;

    void Update()
    {
        FindClosestTarget();

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
                transform.position,
                target.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target.transform;
            }
        }
    }

    void MoveEyes()
    {
        // Direction towards target
        Vector3 direction =
            (closestTarget.position - transform.position).normalized;

        // Find a sideways direction perpendicular to target direction
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized;

        // Avoid issues when looking directly up or down
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(direction, Vector3.forward).normalized;

        // Calculate eye positions
        Vector3 leftPosition = (
            direction * eyeDistance -
            right * (eyeSpacing / 2f)
        ).normalized * eyeDistance;

        Vector3 rightPosition = (
            direction * eyeDistance +
            right * (eyeSpacing / 2f)
        ).normalized * eyeDistance;

        // Convert to world positions
        leftPosition += transform.position;
        rightPosition += transform.position;

        // Smooth movement
        leftEye.position = Vector3.Lerp(
            leftEye.position,
            leftPosition,
            movementSpeed * Time.deltaTime
        );

        rightEye.position = Vector3.Lerp(
            rightEye.position,
            rightPosition,
            movementSpeed * Time.deltaTime
        );

        // Make eyes face outward
        leftEye.rotation = Quaternion.LookRotation(
            (leftEye.position - transform.position).normalized
        );

        rightEye.rotation = Quaternion.LookRotation(
            (rightEye.position - transform.position).normalized
        );
    }
}
