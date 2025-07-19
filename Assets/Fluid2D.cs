using UnityEngine;
using System.Collections.Generic;

public class Fluid2D : MonoBehaviour
{
    public int particleCount = 300;
    public Vector2 bounds = new Vector2(2, 2);
    public float gizmoCircleRadius = 0.05f;
    public Color gizmoColor = Color.cyan;
    public float effectRadius = 0.7f;
    public float kConstant = 0.1f;
    public float dampAmount = 0.95f;
    public float particleVelocityCap = 0.3f;
    private Vector2[] positions;
    private Vector2[] velocities;
    private Dictionary<Vector2Int, List<int>> map = new Dictionary<Vector2Int, List<int>>();

    Vector2Int getKey(Vector2 pos)
    {
        return new Vector2Int(
            Mathf.FloorToInt(pos.x / effectRadius),
            Mathf.FloorToInt(pos.y / effectRadius)
        );
    }

    void addParticleToMap(int index) {
        Vector2Int key = getKey(positions[index]);
        if (!map.ContainsKey(key))
            map[key] = new List<int>();
        map[key].Add(index);
    }

    void removeParticleFromMap(int index) {
        Vector2Int key = getKey(positions[index]);
        if (map.ContainsKey(key))
        {
            map[key].Remove(index);
            if (map[key].Count == 0)
                map.Remove(key);
        }
    }

    void Awake()
    {
        positions = new Vector2[particleCount];
        velocities = new Vector2[particleCount];

        for (int i = 0; i < particleCount; i++)
        {
            positions[i] = new Vector2(
                Random.Range(-bounds.x, bounds.x),
                Random.Range(-bounds.y, bounds.y)
            );
            addParticleToMap(i);
            velocities[i] = Vector2.zero;
        }
    }

    IEnumerable<Vector2Int> GetNeighborKeys(Vector2Int key)
    {
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                yield return new Vector2Int(key.x + dx, key.y + dy);
    }

    void FixedUpdate()
    {
        SimulationStep();
    }

    void SimulationStep()
    {
        Vector2[] forces = new Vector2[particleCount];
        for (int i = 0; i < particleCount; i++)
        {
            forces[i] = new Vector2(0, -9.81f / 60f);

            Vector2Int cell = getKey(positions[i]);
            foreach (Vector2Int neighbor in GetNeighborKeys(cell))
            {
                if (!map.TryGetValue(neighbor, out var neighborList)) continue;

                foreach (int j in neighborList)
                {
                    if (i == j) continue;

                    Vector2 dir = positions[j] - positions[i];
                    float dist = dir.magnitude;

                    if (dist == 0f)
                    {
                        Vector2 randomDir = Random.insideUnitCircle;
                        forces[i] += randomDir * kConstant;
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
            velocities[i] = Vector2.ClampMagnitude(velocities[i], particleVelocityCap);
            // add force away from bounds
            float wallForceDist = 0.3f;
            float wallForceStrength = 0.27f;
            // Distance from each wall
            float distRight = bounds.x - positions[i].x;
            float distLeft  = positions[i].x + bounds.x;
            float distTop   = bounds.y - positions[i].y;
            float distBottom= positions[i].y + bounds.y;

            if (distRight < wallForceDist)
                forces[i].x -= (1f - distRight / wallForceDist) * wallForceStrength;
            if (distLeft < wallForceDist)
                forces[i].x += (1f - distLeft / wallForceDist) * wallForceStrength;

            if (distTop < wallForceDist)
                forces[i].y -= (1f - distTop / wallForceDist) * wallForceStrength;
            if (distBottom < wallForceDist)
                forces[i].y += (1f - distBottom / wallForceDist) * wallForceStrength;
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

            // clamp inside bounds
            positions[i] = new Vector2(
                Mathf.Clamp(positions[i].x, -bounds.x, bounds.x),
                Mathf.Clamp(positions[i].y, -bounds.y, bounds.y)
            );
        }
    }

    // Draw both the bounds box and each particle as a small circle
    void OnDrawGizmos()
    {
        if (positions == null || positions.Length == 0) return;
        
        // Draw bounds rect
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, new Vector3(bounds.x * 2, bounds.y * 2, 0));
        
        // Draw particles
        Gizmos.color = gizmoColor;
        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 pos3D = new Vector3(positions[i].x, positions[i].y, 0) + transform.position;
            DrawCircle(pos3D, gizmoCircleRadius, 12);
        }
    }
    
    // Helper method to draw a circle in gizmos since Unity doesn't have a direct method for this
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        Vector3 prevPos = center + new Vector3(radius, 0, 0);
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 2 * Mathf.PI;
            Vector3 pos = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPos, pos);
            prevPos = pos;
        }
    }
}
