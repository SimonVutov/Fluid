using UnityEngine;
using System.Collections.Generic;

public class Fluid : MonoBehaviour
{
    [Header("Simulation Settings")]
    public int particleCount = 300;
    public Vector3 bounds = new Vector3(10, 10, 10);
    public float gizmoSphereRadius = 0.05f;
    public Color gizmoColor = Color.cyan;

    private Vector3[] positions;
    private Vector3[] velocities;
    public float effectRadius = 1.0f;
    public float kConstant = 0.08f;
    public float dampAmount = 0.99f;
    public float particleVelocityCap = 0.1f;

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
                else if (dist < effectRadius) forces[i] += -dir.normalized / (dist * dist) * kConstant;
            }

            // cap velocity
            if (velocities[i].magnitude > particleVelocityCap)
            {
                velocities[i] = velocities[i].normalized * particleVelocityCap;
            }
            float wallForceDist = 0.5f;
            float wallForceStrength = 0.1f;
            // add force away from bounds
            forces[i] += new Vector3(
                Mathf.Clamp((Mathf.Abs(positions[i].x) - bounds.x/2) * ((positions[i].x) - bounds.x/2) > wallForceDist ? 0 : Mathf.Sign(positions[i].x), -wallForceDist, wallForceDist) * wallForceStrength,
                Mathf.Clamp((Mathf.Abs(positions[i].y) - bounds.y/2) * ((positions[i].y) - bounds.y/2) > wallForceDist ? 0 : Mathf.Sign(positions[i].y), -wallForceDist, wallForceDist) * wallForceStrength,
                Mathf.Clamp((Mathf.Abs(positions[i].z) - bounds.z/2) * ((positions[i].z) - bounds.z/2) > wallForceDist ? 0 : Mathf.Sign(positions[i].z), -wallForceDist, wallForceDist) * wallForceStrength
            );
        }
        for (int i = 0; i < particleCount; i++)
        {
            velocities[i] += forces[i];
            positions[i]  += velocities[i];

            // apply damping
            velocities[i] *= dampAmount;

            if (positions[i].x > bounds.x * 0.5f || positions[i].x < -bounds.x * 0.5f)
                velocities[i].x *= -0.5f;
            if (positions[i].y > bounds.y * 0.5f || positions[i].y < -bounds.y * 0.5f)
                velocities[i].y *= -0.5f;
            if (positions[i].z > bounds.z * 0.5f || positions[i].z < -bounds.z * 0.5f)
                velocities[i].z *= -0.5f;

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