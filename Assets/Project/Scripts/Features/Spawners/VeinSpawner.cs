using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns and repositions coal veins within a defined area on top of the terrain.
/// Enforces minimum separation from other veins and the preconfigurated obstacle positions.
/// Respawns collected veins in a valid location at the end of frame.
/// </summary>
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
    //Prevents multiple replace request over same vein
    private HashSet<CoalVein> replaceController;
    private static WaitForEndOfFrame WaitEOF;
    private List<Vector3> obstaclePos;
    private int spawnAreaLayerMask;

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// Stores terrain data, obstacle positions, the spawn-area layer and initializes internal collections.
    /// </summary>
    private void Awake()
    {
        veinPositions = new Dictionary<CoalVein, Vector3>();
        spawnedVeins = new List<GameObject>();
        obstaclePos = new List<Vector3>();
        replaceController = new HashSet<CoalVein>();
        WaitEOF = new();

        terrainData = terrain.terrainData;
        terrainPos = terrain.transform.position;
        terrainSize = terrainData.size;

        spawnAreaLayerMask = 1 << spawnArea.gameObject.layer;

        if (obstaclePositions != null)
        {
            foreach (Transform transform in obstaclePositions) obstaclePos.Add(transform.position);
        }
    }

    /// <summary>
    /// Intantiates a single vein at a valid location if one can be found
    /// </summary>
    /// <returns>False is cannot find a valid point, otherwise returns true.</returns>
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

    /// <summary>
    /// Prepares a collected vein to be moved to a new valid position at the end of frame.
    /// Prevents multiple request on the same vein using a replaceController.
    /// </summary>
    /// <param name="veinCollected">Vein that was collected.</param>
    public void ReplaceVein(CoalVein veinCollected)
    {
        if (!veinCollected || replaceController.Contains(veinCollected)) return;
        replaceController.Add(veinCollected);
        StartCoroutine(ReplaceVeinEndOfFrame(veinCollected));
    }

    /// <summary>
    /// Disables the received vein game object and waits for an Update cycle (frame) to avoid acting during the same update/physics step.
    /// Then searches for a valid location, repositions the vein and re-enables it, clearing the guard.
    /// </summary>
    /// <param name="vein">Vein to be repositioned.</param>
    /// 
    private System.Collections.IEnumerator ReplaceVeinEndOfFrame(CoalVein vein)
    {
        if (!vein) yield break;

        vein.gameObject.SetActive(false);

        yield return WaitEOF;

        if (!FindValidPoint(out Vector3 position))
        {
            Debug.Log("VeinSpawner: No available positon for collected vein");
            yield break;
        }

        veinPositions[vein] = position;
        vein.transform.SetPositionAndRotation(position, Quaternion.identity);
        vein.gameObject.SetActive(true);
        replaceController.Remove(vein);
    }

    /// <summary>
    /// Attempts to find a random valid position withing the spawn area.
    /// The position has to be inside the volume prokection, on an appropiate terrain heigh (no spawns inside terrain)
    /// and far enough from other veins and obstacles.
    /// </summary>
    /// <param name="position">Valid position within the terrain, otherwise is null.</param>
    /// <returns>True if a valid position was found within the max amount of tries, otherwise false.</returns>
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
                float d2 = dx * dx + dz * dz;
                if (d2 < sqrDistXZ) { tooClose = true; break; }
            }

            if (tooClose) continue;

            position = possible;
            return true;
        }

        position = default;
        return false;
    }

    /// <summary>
    /// Checks whether the proposed XZ position is within the minimum obstacle distance.
    /// </summary>
    /// <param name="x">World X coordinate.</param>
    /// <param name="z">World Z coordinate.</param>
    /// <returns>True if the point is too close to an obstable, otherwise returns false.</returns>
    private bool InsideObstacleRangeXZ(float x, float z)
    {
        if (obstaclePositions == null) return false;
        float minDistObstacleSqr = minDistanceObstacles * minDistanceObstacles;

        foreach (Vector3 obstacle in obstaclePos)
        {
            float dx = obstacle.x - x;
            float dz = obstacle.z - z;
            float d = dx * dx + dz * dz;
            if (d < minDistObstacleSqr) return true;
        }

        return false;
    }

    /// <summary>
    /// Checks whether the proposed XZ position is withing the spawnArea volume using RayCast.
    /// </summary>
    /// <param name="x">X World position.</param>
    /// <param name="z">Z World position</param>
    /// <returns>True if the position is inside the spawn area, otherwise returns false.</returns>
    private bool InsideVolumeXZ(float x, float z)
    {
        Bounds b = spawnArea.bounds;
        Vector3 origin = new Vector3(x, b.max.y + 5f, z);
        Vector3 dir = Vector3.down;
        float dist = b.size.y + 10f;

        int hits = Physics.RaycastNonAlloc(origin, dir, rayCastBuffer, dist, spawnAreaLayerMask, QueryTriggerInteraction.Ignore);

        if (hits == rayCastBuffer.Length)
        {
            // Doubles buffer if full
            rayCastBuffer = new RaycastHit[rayCastBuffer.Length * 2];
            hits = Physics.RaycastNonAlloc(origin, dir, rayCastBuffer, dist, spawnAreaLayerMask, QueryTriggerInteraction.Ignore);
        }

        int count = 0;
        for (int i = 0; i < hits; i++)
            if (rayCastBuffer[i].collider == spawnArea) count++;

        return (count % 2) == 1; // impar = dentro
    }

    /// <summary>
    /// Destroys all current veins, clears the internal collections and spawns new veins at valid locations.
    /// </summary>
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
    
