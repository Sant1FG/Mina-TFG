using System;
using System.Collections.Generic;
using System.Linq;
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
    private Vector3 terrainPos;
    private Vector3 terrainSize;
    private TerrainData terrainData;
    private Dictionary<CoalVein, Vector3> veinPositions;
    private List<GameObject> spawnedVeins;
    private static RaycastHit[] rayCastBuffer = new RaycastHit[16];
    private readonly HashSet<CoalVein> replaceController = new();

    private void Awake()
    {
        veinPositions = new Dictionary<CoalVein, Vector3>();
        spawnedVeins = new List<GameObject>();
        terrainData = terrain.terrainData;
        terrainPos = terrain.transform.position;
        terrainSize = terrainData.size;
    }

    private bool SpawnOneVein()
    {
        if (!FindValidPoint(out Vector3 position))
        {
            return false;
        }

        GameObject spawned = Instantiate(prefab, position, Quaternion.identity, transform);
        CoalVein vein = spawned.GetComponent<CoalVein>();
        if (vein != null)
        {
            veinPositions.Add(vein, position);
            spawnedVeins.Add(vein.gameObject);
        }
        return true;
    }

    public void ReplaceVein(CoalVein veinCollected)
    {
        if (!veinCollected || replaceController.Contains(veinCollected)) return;
        replaceController.Add(veinCollected);
        StartCoroutine(ReplaceVeinEndOfFrame(veinCollected));
    }

    private System.Collections.IEnumerator ReplaceVeinEndOfFrame(CoalVein vein)
    {
        if (!vein) yield break;

        vein.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        if (!FindValidPoint(out Vector3 position))
        {
            Debug.Log("No available positon for collected vein");
            yield break;
        }

        veinPositions[vein] = position;
        vein.transform.SetPositionAndRotation(position, Quaternion.identity);
        vein.gameObject.SetActive(true);
        replaceController.Remove(vein);
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

            float nx = (x - terrainPos.x) / terrainSize.x;
            float nz = (z - terrainPos.z) / terrainSize.z;

            if (nx < 0f || nx > 1f || nz < 0f || nz > 1f) continue;
            float y = terrainData.GetInterpolatedHeight(nx, nz) + terrainPos.y;

            Vector3 possible = new Vector3(x, y, z);
            bool tooClose = false;
            float sqrDistXZ = minDistanceXZ * minDistanceXZ;
            foreach (var pos in veinPositions.Values)
            {
                float dx = possible.x - pos.x;
                float dz = possible.z - pos.z;
                float d2 = dx*dx + dz*dz;
                if (d2 < sqrDistXZ) { tooClose = true; break; }
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
        int hits = Physics.RaycastNonAlloc(origin, dir, rayCastBuffer, dist,~0, QueryTriggerInteraction.Ignore);

        if (hits == rayCastBuffer.Length)
        {
        // Buffer lleno; ampliamos y repetimos una vez (extremadamente raro)
        rayCastBuffer = new RaycastHit[rayCastBuffer.Length * 2];
        hits = Physics.RaycastNonAlloc(origin, dir, rayCastBuffer, dist, ~0, QueryTriggerInteraction.Ignore);
        }

        int count = 0;
        for (int i = 0; i < hits; i++)
            if (rayCastBuffer[i].collider == spawnArea) count++;

        return (count % 2) == 1; // impar = dentro
    }

    public void ResetVeinSpawner()
    {
        foreach (var vein in spawnedVeins)
        {
            if (vein != null) Destroy(vein);
        }

        veinPositions.Clear();
        spawnedVeins.Clear();

        for (int i = 0; i < initialVeins; i++)
        {
            SpawnOneVein();
        }
    }


}
    
