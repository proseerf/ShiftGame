using UnityEngine;

public class SoftBodyParticle
{
    public Vector3 position;
    public Vector3 previousPosition;
    public Vector3 velocity;

    public float inverseMass = 1f;

    public SoftBodyParticle(Vector3 position)
    {
        this.position = position;
        previousPosition = position;
        velocity = Vector3.zero;
    }
}
