
using UnityEngine;

public class MorphObject : MonoBehaviour
{
    [Header("Morph Settings")]
    [Min(0.001f)]
    public float morphDuration = 0.8f;

    public AnimationCurve morphCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Min(0f)]
    public float morphOversize = 0.05f;

    [Header("Surface Mapping")]
    [Min(0f)]
    public float rayPadding = 0.1f;

    public bool tryOppositeRay = true;

    public bool IsMorphing { get; private set; }
    public bool IsComplete { get; private set; }

    // Current positions in source-local space.
    public Vector3[] CurrentPositions => currentPositions;

    private Transform sourceTransform;

    private Vector3[] startPositions;
    private Vector3[] targetPositions;
    private Vector3[] currentPositions;

    private float elapsedTime;

    private GameObject mappingObject;
    private MeshCollider mappingCollider;

    public bool Begin(
        Transform source,
        Vector3[] sourceLocalPositions,
        Mesh targetMesh,
        Transform targetTransform,
        float duration = -1f)
    {
        if (source == null ||
            sourceLocalPositions == null ||
            sourceLocalPositions.Length == 0 ||
            targetMesh == null ||
            targetTransform == null)
        {
            Debug.LogError("MorphObject: Invalid input.", this);
            return false;
        }

        if (targetMesh.triangles.Length < 3)
        {
            Debug.LogError("MorphObject: Target mesh has no triangles.", this);
            return false;
        }

        Cancel();

        sourceTransform = source;

        if (duration > 0f)
            morphDuration = duration;

        int count = sourceLocalPositions.Length;

        startPositions = new Vector3[count];
        targetPositions = new Vector3[count];
        currentPositions = (Vector3[])sourceLocalPositions.Clone();

        // Store starting positions in world space.
        for (int i = 0; i < count; i++)
        {
            startPositions[i] =
                source.TransformPoint(sourceLocalPositions[i]);
        }

        // Create a temporary collider matching the target.
        if (!CreateMappingCollider(targetMesh, targetTransform))
        {
            CleanupMappingCollider();
            return false;
        }

        // Calculate where every particle should end up.
        CalculateTargetPositions();

        elapsedTime = 0f;
        IsMorphing = true;
        IsComplete = false;

        return true;
    }

    public Vector3[] Advance(float deltaTime)
    {
        if (!IsMorphing)
            return currentPositions;

        elapsedTime += Mathf.Max(0f, deltaTime);

        float progress = Mathf.Clamp01(
            elapsedTime / Mathf.Max(morphDuration, 0.001f)
        );

        float curveValue = morphCurve != null
            ? Mathf.Clamp01(morphCurve.Evaluate(progress))
            : progress;

        for (int i = 0; i < currentPositions.Length; i++)
        {
            Vector3 worldPosition = Vector3.LerpUnclamped(
                startPositions[i],
                targetPositions[i],
                curveValue
            );

            currentPositions[i] =
                sourceTransform.InverseTransformPoint(worldPosition);
        }

        if (progress >= 1f)
        {
            IsMorphing = false;
            IsComplete = true;

            CleanupMappingCollider();
        }

        return currentPositions;
    }

    public void Cancel()
    {
        IsMorphing = false;
        IsComplete = false;

        CleanupMappingCollider();
    }

    private bool CreateMappingCollider(
        Mesh targetMesh,
        Transform targetTransform)
    {
        mappingObject = new GameObject("MorphMapping");

        mappingObject.hideFlags = HideFlags.HideAndDontSave;

        // Match the target's complete transform.
        mappingObject.transform.SetParent(
            targetTransform.parent,
            false
        );

        mappingObject.transform.localPosition =
            targetTransform.localPosition;

        mappingObject.transform.localRotation =
            targetTransform.localRotation;

        mappingObject.transform.localScale =
            targetTransform.localScale;

        mappingCollider =
            mappingObject.AddComponent<MeshCollider>();

        mappingCollider.sharedMesh = targetMesh;
        mappingCollider.convex = false;

        return mappingCollider.sharedMesh != null;
    }

    private void CalculateTargetPositions()
    {
        Bounds bounds = mappingCollider.bounds;

        Vector3 targetCenter = bounds.center;

        float rayDistance =
            bounds.extents.magnitude * 2f +
            Mathf.Max(rayPadding, 0.01f) * 2f;

        // Find the centre of the source particles.
        Vector3 sourceCenter = Vector3.zero;

        for (int i = 0; i < startPositions.Length; i++)
            sourceCenter += startPositions[i];

        sourceCenter /= startPositions.Length;

        for (int i = 0; i < startPositions.Length; i++)
        {
            // Determine the direction from the source centre.
            Vector3 direction =
                startPositions[i] - sourceCenter;

            if (direction.sqrMagnitude < 0.000001f)
                direction = Vector3.up;
            else
                direction.Normalize();

            Vector3 surfacePoint;

            // Find the corresponding point on the target surface.
            if (!TryGetSurfacePoint(
                targetCenter,
                direction,
                rayDistance,
                out surfacePoint))
            {
                Vector3 samplePoint =
                    targetCenter +
                    direction *
                    (bounds.extents.magnitude + rayPadding);

                surfacePoint =
                    mappingCollider.ClosestPoint(samplePoint);
            }

            // Push the destination slightly outside the target.
            Vector3 outwardDirection =
                (surfacePoint - targetCenter).normalized;

            targetPositions[i] =
                surfacePoint +
                outwardDirection * morphOversize;
        }
    }

    private bool TryGetSurfacePoint(
        Vector3 center,
        Vector3 direction,
        float rayDistance,
        out Vector3 surfacePoint)
    {
        float startDistance =
            rayDistance * 0.5f + rayPadding;

        // Cast inward from outside the target.
        Vector3 origin =
            center + direction * startDistance;

        Ray ray = new Ray(origin, -direction);

        if (mappingCollider.Raycast(
            ray,
            out RaycastHit hit,
            rayDistance))
        {
            surfacePoint = hit.point;
            return true;
        }

        // Try the opposite direction if necessary.
        if (tryOppositeRay)
        {
            origin = center - direction * startDistance;

            ray = new Ray(origin, direction);

            if (mappingCollider.Raycast(
                ray,
                out hit,
                rayDistance))
            {
                surfacePoint = hit.point;
                return true;
            }
        }

        surfacePoint = center;
        return false;
    }

    private void CleanupMappingCollider()
    {
        if (mappingObject != null)
        {
            if (Application.isPlaying)
                Destroy(mappingObject);
            else
                DestroyImmediate(mappingObject);
        }

        mappingObject = null;
        mappingCollider = null;
    }

    private void OnDestroy()
    {
        CleanupMappingCollider();
    }
}
