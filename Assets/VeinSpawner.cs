using System;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class VeinSpawner : MonoBehaviour
{
    [SerializeField] private Terrain terrain;
    [SerializeField] private MeshCollider spawnArea;
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialVeins;
    [SerializeField] private int maxTriesPerSpawn = 40;
    [SerializeField] private float minDistanceXZ = 5f;
    [SerializeField] private float minDistanceObstacles = 10f;
    [SerializeField] Transform obstaclePositions;
    [SerializeField] private float maxSlopeDeg = 25f;
    private Vector3 terrainPos;
    private Vector3 terrainSize;
    private TerrainData terrainData;

    private List<Vector3> generatedVeins;


    private void Awake()
    {
        generatedVeins = new List<Vector3>();
        terrainData = terrain.terrainData;
        terrainPos = terrain.transform.position;
        terrainSize = terrainData.size;
    }

    private void Start()
    {
        for (int i = 0; i < initialVeins; i++)
        {
            SpawnOneVein();
        }
    }

    private bool SpawnOneVein()
    {
        if (!FindValidPoint(out Vector3 position))
        {
            return false;
        }

        Instantiate(prefab, position, Quaternion.identity, transform);
        generatedVeins.Add(position);
        return true;
    }

    public void ReplaceVein(GameObject veinCollected)
    {
        int index = generatedVeins.FindIndex(p => Vector3.SqrMagnitude(p - veinCollected.transform.position) < 0.01f);
        if (index >= 0) generatedVeins.RemoveAt(index);
        Destroy(veinCollected);
        SpawnOneVein();
    }

    private bool FindValidPoint(out Vector3 position)
    {
        Bounds bounds = spawnArea.bounds;

        for (int i = 0; i < maxTriesPerSpawn; i++)
        {
            float x = UnityEngine.Random.Range(bounds.min.x, bounds.max.x);
            float z = UnityEngine.Random.Range(bounds.min.z, bounds.max.z);

            if (!InsideVolumeXZ(x, z)) continue;
            if (InsideObstacleRangeXZ(x, z)) continue;

            Vector3 probe = new Vector3(x, bounds.center.y, z);
            if (spawnArea.ClosestPoint(probe) != probe) continue;

            float nx = (x - terrainPos.x) / terrainSize.x;
            float nz = (z - terrainPos.z) / terrainSize.z;

            if (nx < 0f || nx > 1f || nz < 0f || nz > 1f) continue;
            float y = terrainData.GetInterpolatedHeight(nx, nz) + terrainPos.y;

            Vector3 possible = new Vector3(x, y, z);
            bool tooClose = false;
            float sqrDistXZ = minDistanceXZ * minDistanceXZ;
            for (int j = 0; j < generatedVeins.Count; j++)
            {
                float dx = possible.x - generatedVeins[j].x;
                float dz = possible.z - generatedVeins[j].z;
                float d2 = dx * dx + dz * dz;
                if (d2 < sqrDistXZ ) { tooClose = true; break; }
            }

            if (tooClose) continue;

            position = possible;
            return true;
        }

        position = default;
        return false;
    }

    private bool InsideObstacleRangeXZ(float x, float z)
    {
        if (obstaclePositions == null) return false;
        float minDistObstacleSqr = minDistanceObstacles * minDistanceObstacles;

        foreach (Transform obstacle in obstaclePositions)
        {
            float dx = obstacle.position.x - x;
            float dz = obstacle.position.z - z;
            float d = dx * dx + dz * dz;
            if (d < minDistObstacleSqr) return true;
        }

        return false;
    }

    // 2) Validar que (x,z) cae dentro del volumen con RaycastAll (paridad impar)
    bool InsideVolumeXZ(float x, float z)
{
    Bounds b = spawnArea.bounds;
    Vector3 origin = new Vector3(x, b.max.y + 5f, z);
    Vector3 dir = Vector3.down;
    float dist = b.size.y + 10f;

    // Filtra por la capa del volumen si puedes (crea una Layer "SpawnVolume")
    var hits = Physics.RaycastAll(origin, dir, dist, ~0, QueryTriggerInteraction.Ignore);

    int count = 0;
    for (int i = 0; i < hits.Length; i++)
        if (hits[i].collider == spawnArea) count++;

    return (count % 2) == 1; // impar = dentro
}


     }
    
