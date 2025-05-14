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
    private Dictionary<Vector3Int, List<int>> map = new Dictionary<Vector3Int, List<int>>();

    Vector3Int getKey(Vector3 pos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(pos.x / effectRadius),
            Mathf.FloorToInt(pos.y / effectRadius),
            Mathf.FloorToInt(pos.z / effectRadius)
        );
    }

    void addParticleToMap(int index) {
        Vector3Int key = getKey(positions[index]);
        if (!map.ContainsKey(key))
            map[key] = new List<int>();
        map[key].Add(index);
    }

    void removeParticleFromMap(int index) {
        Vector3Int key = getKey(positions[index]);
        if (map.ContainsKey(key))
        {
            map[key].Remove(index);
            if (map[key].Count == 0)
                map.Remove(key);
        }
    }

    void Awake()
    {
        positions = new Vector3[particleCount];
        velocities = new Vector3[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            positions[i] = Vector3.Scale(Random.insideUnitSphere, bounds);
            addParticleToMap(i);
            velocities[i] = Vector3.zero;
        }
    }

    IEnumerable<Vector3Int> GetNeighborKeys(Vector3Int key)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++)
                    yield return new Vector3Int(key.x + dx, key.y + dy, key.z + dz);
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

            Vector3Int cell = getKey(positions[i]);
            foreach (Vector3Int neighbor in GetNeighborKeys(cell))
            {
                if (!map.TryGetValue(neighbor, out var neighborList)) continue;

                foreach (int j in neighborList)
                {
                    if (i == j) continue;

                    Vector3 dir = positions[j] - positions[i];
                    float dist = dir.magnitude;

                    if (dist == 0f)
                    {
                        forces[i] += Random.insideUnitSphere * kConstant;
                        continue;
                    }

                    float q = dist / effectRadius;
                    if (q < 1f)
                    {
                        float strength = (1f - q) * kConstant;
                        forces[i] += -dir.normalized * strength;
                    }
                }
            }

            // cap velocity
            velocities[i] = Vector3.ClampMagnitude(velocities[i], particleVelocityCap);
            // add force away from bounds
            float wallForceDist = 0.3f;
            float wallForceStrength = 0.27f;
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
            removeParticleFromMap(i);
            positions[i]  += velocities[i];
            addParticleToMap(i);

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