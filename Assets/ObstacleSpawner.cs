using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Spawns obstacle at predefined positions around the scenario.
/// Keeps a pool of instantiated random obstacles activating them at a predefined interval.
/// Raises an event when activating a new obstacle.
/// </summary>
public class ObstacleSpawner : MonoBehaviour
{

    [SerializeField] Transform internalPositions;
    [SerializeField] Transform player;
    [SerializeField] TimerController timer;
    [SerializeField] private List<GameObject> obstaclePrefabs;
    //How far can an obstacle spawn from the player's current position
    [SerializeField] private float playerSpawnDist = 15f;
    /// <summary>
    /// Invoked to signal that an obstacle has spawned. Sending a notification toast.
    /// </summary>
    public event Action<string, float> OnObstacleSpawn;
    private Dictionary<Vector3, GameObject> obstacleDictionary;
    private HashSet<Vector3> activatedPositions;
    private List<Vector3> obstaclePositions;
    private List<Vector3> freeSpots;
    private List<Vector3> validSpots;

    public float spawnInterval = 10f;
    private float nextSpawn;

    private bool spawningEnabled = false;

    /// <summary>
    /// Called by Unity when the script instance is being loaded.
    /// Stores spawn positions in obstaclePositions and initializes internal collections.
    /// </summary>
    private void Awake()
    {
        obstacleDictionary = new Dictionary<Vector3, GameObject>();
        activatedPositions = new HashSet<Vector3>();
        obstaclePositions = new List<Vector3>();
        freeSpots = new List<Vector3>();
        validSpots = new List<Vector3>();

        foreach (Transform transform in internalPositions)
        {
            obstaclePositions.Add(transform.position);
        }
    }

    /// <summary>
    /// Called by Unity before the first execution of Update.
    /// Sets a deadline for the next obstacle spawn.
    /// </summary>
    private void Start()
    {
        nextSpawn = spawnInterval + Time.time;
    }

    /// <summary>
    /// Called by Unity once per frame. Checks the spawn timer and activates a random obstacle when crosses the timer
    /// crosses the deadline. Only works if spawning is enabled.
    /// </summary>
    private void Update()
    {
        if (!spawningEnabled) return;
        if (Time.time >= nextSpawn)
        {
            ActivateRandomObstacle();
            nextSpawn = Time.time + spawnInterval;
        }
    }

    /// <summary>
    /// Creates one obstacle instance per configured position, initially inactive.
    /// A random prebaf is chosen for each obstacle instance.
    /// </summary>
    private void InstantiateObstacles()
    {
        foreach (Vector3 position in obstaclePositions)
        {
            int index = Random.Range(0, obstaclePrefabs.Count);
            GameObject obstacle = obstaclePrefabs[index];
            GameObject instance = Instantiate(obstacle, position, Quaternion.identity);
            instance.SetActive(false);
            obstacleDictionary.Add(position, instance);
        }
    }

    /// <summary>
    /// Picks a random inactive spot that is farther than playerSpawnDist from the player.
    /// Activates its pre-instantiated obstacle, marks it as active and notifies subscribers.
    /// Returns if there are no free spots left.
    /// </summary>
    private void ActivateRandomObstacle()
    {
        freeSpots.Clear();
        foreach (Vector3 s in obstacleDictionary.Keys) if (!activatedPositions.Contains(s)) freeSpots.Add(s);
        //Test if player is close enough to any obstacle
        if (freeSpots.Count == 0) return;
        validSpots.Clear();
        float sqrMinDistance = playerSpawnDist * playerSpawnDist;
        foreach (Vector3 currentSpot in freeSpots)
        {
            float dx = currentSpot.x - player.position.x;
            float dz = currentSpot.z - player.position.z;
            float d = dx * dx + dz * dz;
            if (d > sqrMinDistance)
            {
                validSpots.Add(currentSpot);
            }

        }
        if (validSpots.Count == 0)
        {
            Debug.Log("ObstacleSpawner: Player adjacent to all free spots.");
            return;
        }

        // elegir spot aleatorio
        Vector3 randomValidPosition = validSpots[UnityEngine.Random.Range(0, validSpots.Count)];
        GameObject selected = obstacleDictionary[randomValidPosition];
        selected.SetActive(true);
        activatedPositions.Add(randomValidPosition);
        ActivateObstacleNotification(selected);

    }

    /// <summary>
    /// Sends the spawn notification depending on the type of activated obstacle.
    /// </summary>
    /// <param name="activated">Obstacle chosen for activation</param>
    private void ActivateObstacleNotification(GameObject activated)
    {
        String spawnMessage = "";
        //GasObstacles necesitan timeController
        if (activated.TryGetComponent<ToxicGas>(out ToxicGas gas))
        {
            gas.AddTimerController(timer);
            spawnMessage = "PRECAUCION: Se ha detectado una bolsa de gas tóxico";
        }
        else if (activated.TryGetComponent<OilSpill>(out OilSpill oil))
        {
            spawnMessage = "PRECAUCION: Se ha derramado aceite resbaladizo";
        }
        else
        {
            spawnMessage = "PRECAUCION: Ha ocurrido un derrumbe";
        }

        OnObstacleSpawn?.Invoke(spawnMessage, 3f);
    }

    /// <summary>
    /// Resets the obstacle spawner: destroys all current obstacle instances, clear the internal collections, 
    /// re-instantiates all obstacles, restarts the next spawn deadline timer and resumes spawning.
    /// </summary>
    public void ResetObstacleSpawner()
    {
        if (obstaclePositions.Count == 0) return;

        foreach (var item in obstacleDictionary.Values)
        {
            if (item != null) Destroy(item);
        }

        // Vaciar listas
        obstacleDictionary.Clear();
        activatedPositions.Clear();

        InstantiateObstacles();
        // Reiniciar temporizador de spawn
        nextSpawn = Time.time + spawnInterval;
        ResumeSpawning();
    }

    /// <summary>
    /// Enables periodic obstacle activation.
    /// </summary>
    public void ResumeSpawning()
    {
        spawningEnabled = true;
    }

    /// <summary>
    /// Disables periodic obstacle activation.
    /// </summary>
    public void StopSpawning()
    {
        spawningEnabled = false;
    }
}
