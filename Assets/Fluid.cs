using UnityEngine;
using System.Collections.Generic;

public class Fluid : MonoBehaviour
{
    public int particleCount = 300;
    public Vector3 bounds = new Vector3(2, 2, 2);
    public float gizmoSphereRadius = 0.05f;
    public Color gizmoColor = Color.cyan;
    public float effectRadius = 0.7f;
    public float kConstant = 0.1f;
    public float dampAmount = 0.95f;
    public float particleVelocityCap = 0.3f;
    private Vector3[] positions;
    private Vector3[] velocities;

    void Awake()
    {
        positions = new Vector3[particleCount];
        velocities = new Vector3[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            positions[i] = Vector3.Scale(Random.insideUnitSphere, bounds);
            velocities[i] = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        SimulationStep();
    }

    void SimulationStep()
    {
        Vector3[] forces = new Vector3[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            forces[i] = new Vector3(0, -9.81f / 60f, 0);

            for (int j = 0; j < particleCount; j++)
            {
                if (i == j) continue;
                Vector3 dir = positions[j] - positions[i];
                float dist = dir.magnitude;
                if (dist == 0) forces[i] += new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * kConstant;
                float q = dist / effectRadius;
                if (q < 1f)
                {
                    float strength = (1 - q) * kConstant;
                    forces[i] += -dir.normalized * strength;
                }
            }

            // cap velocity
            velocities[i] = Vector3.ClampMagnitude(velocities[i], particleVelocityCap);
            // add force away from bounds
            float wallForceDist = 0.3f;
            float wallForceStrength = 0.2f;
            // Distance from each wall
            float distRight = bounds.x - positions[i].x;
            float distLeft  = positions[i].x + bounds.x;
            float distTop   = bounds.y - positions[i].y;
            float distBottom= positions[i].y + bounds.y;
            float distFront = bounds.z - positions[i].z;
            float distBack  = positions[i].z + bounds.z;

            if (distRight < wallForceDist)
                forces[i].x -= (1f - distRight / wallForceDist) * wallForceStrength;
            if (distLeft < wallForceDist)
                forces[i].x += (1f - distLeft / wallForceDist) * wallForceStrength;

            if (distTop < wallForceDist)
                forces[i].y -= (1f - distTop / wallForceDist) * wallForceStrength;
            if (distBottom < wallForceDist)
                forces[i].y += (1f - distBottom / wallForceDist) * wallForceStrength;

            if (distFront < wallForceDist)
                forces[i].z -= (1f - distFront / wallForceDist) * wallForceStrength;
            if (distBack < wallForceDist)
                forces[i].z += (1f - distBack / wallForceDist) * wallForceStrength;
        }
        for (int i = 0; i < particleCount; i++)
        {
            velocities[i] += forces[i];
            positions[i]  += velocities[i];

            // apply damping
            velocities[i] *= dampAmount;

            if (positions[i].x > bounds.x || positions[i].x < -bounds.x)
                velocities[i].x *= -0.5f;
            if (positions[i].y > bounds.y || positions[i].y < -bounds.y)
                velocities[i].y *= -0.5f;
            if (positions[i].z > bounds.z || positions[i].z < -bounds.z)
                velocities[i].z *= -0.5f;

            // clamp inside bounds
            positions[i] = new Vector3(
                Mathf.Clamp(positions[i].x, -bounds.x, bounds.x),
                Mathf.Clamp(positions[i].y, -bounds.y, bounds.y),
                Mathf.Clamp(positions[i].z, -bounds.z, bounds.z)
            );
        }
    }

    // Draw both the bounds box and each particle as a small sphere
    void OnDrawGizmos()
    {
        if (positions == null || positions.Length == 0) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, bounds * 2);
        Gizmos.color = gizmoColor;
        for (int i = 0; i < positions.Length; i++)
        {
            Gizmos.DrawSphere(transform.position + positions[i], gizmoSphereRadius);
        }
    }
}