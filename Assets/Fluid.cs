using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Fluid : MonoBehaviour
{
    public int particleCount = 1000;
    public Vector3 bounds = new Vector3(10, 10, 10);

    private Vector3[] positions;
    private Vector3[] velocities;

    void Start()
    {
        positions = new Vector3[particleCount];
        velocities = new Vector3[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            positions[i] = new Vector3(
                Random.Range(-bounds.x / 2, bounds.x / 2),
                Random.Range(-bounds.y / 2, bounds.y / 2),
                Random.Range(-bounds.z / 2, bounds.z / 2)
            );
            velocities[i] = Vector3.zero;
        }
    }

  void FixedUpdate()
    {
        SimulationStep();
    }

    void SimulationStep() {
        for (int i = 0; i < particleCount; i++)
        {
            Vector3 Force = new Vector3(0, -9.81f / 60, 0); // Gravity
            for (int j = 0; j < particleCount; j++)
            {
                if (i == j) continue;
                Vector3 direction = positions[j] - positions[i];
                float distance = direction.magnitude;
                
                if (distance < 1.0f)
                {
                    Force += direction.normalized / (distance * distance);
                }
            }

            velocities[i] += Force;
            positions[i] += velocities[i];
            positions[i] = new Vector3(
                Mathf.Clamp(positions[i].x, -bounds.x / 2, bounds.x / 2),
                Mathf.Clamp(positions[i].y, -bounds.y / 2, bounds.y / 2),
                Mathf.Clamp(positions[i].z, -bounds.z / 2, bounds.z / 2)
            );
            // velocities[i] *= 0.99f; // Damping
        }
    }
}
