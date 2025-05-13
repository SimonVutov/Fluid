using UnityEngine;
using System.Collections.Generic;

public class Fluid : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int particleCount = 1000;
    public Vector3 bounds = new Vector3(10, 10, 10);
    public float gizmoSphereRadius = 0.05f;
    public Color gizmoColor = Color.cyan;

    private Vector3[] positions;
    private Vector3[] velocities;

    void Awake()
    {
        positions  = new Vector3[particleCount];
        velocities = new Vector3[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            positions[i] = new Vector3(
                Random.Range(-bounds.x * 0.5f, bounds.x * 0.5f),
                Random.Range(-bounds.y * 0.5f, bounds.y * 0.5f),
                Random.Range(-bounds.z * 0.5f, bounds.z * 0.5f)
            );
            velocities[i] = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        SimulationStep();
    }

    void SimulationStep()
    {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 force = new Vector3(0, -9.81f / 60f, 0);

            for (int j = 0; j < particleCount; j++)
            {
                if (i == j) continue;
                Vector3 dir = positions[j] - positions[i];
                float dist = dir.magnitude;
                
                if (dist > 0f && dist < 2.0f)
                    force += -dir.normalized / (dist * dist) * 0.1f;
            }

            velocities[i] += force;
            positions[i]  += velocities[i];

            // apply damping
            velocities[i] *= 0.3f;

            if (positions[i].x > bounds.x * 0.5f || positions[i].x < -bounds.x * 0.5f)
                velocities[i].x *= -1f;
            if (positions[i].y > bounds.y * 0.5f || positions[i].y < -bounds.y * 0.5f)
                velocities[i].y *= -1f;
            if (positions[i].z > bounds.z * 0.5f || positions[i].z < -bounds.z * 0.5f)
                velocities[i].z *= -1f;

            // clamp inside bounds
            positions[i] = new Vector3(
                Mathf.Clamp(positions[i].x, -bounds.x * 0.5f, bounds.x * 0.5f),
                Mathf.Clamp(positions[i].y, -bounds.y * 0.5f, bounds.y * 0.5f),
                Mathf.Clamp(positions[i].z, -bounds.z * 0.5f, bounds.z * 0.5f)
            );
        }
    }

    // Draw both the bounds box and each particle as a small sphere
    void OnDrawGizmos()
    {
        if (positions == null || positions.Length == 0) return;
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, bounds);
        Gizmos.color = gizmoColor;
        for (int i = 0; i < positions.Length; i++)
        {
            Gizmos.DrawSphere(transform.position + positions[i], gizmoSphereRadius);
        }
    }
}