using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

public class ObstacleSpawner : MonoBehaviour
{

    [SerializeField] Transform internalPositions;
    [SerializeField] Transform player;
    [SerializeField] TimerController timer;
    [SerializeField] private List<GameObject> obstaclePrefabs;
    //How far can an obstacle spawn from the player's current position
    [SerializeField] private float playerSpawnDist = 15f;
    private List<Transform> obstaclePositions;
    private List<GameObject> spawnedObstacles;
    //HashSet hace contains ++ rapidos
    private HashSet<Transform> occupied;
    public event Action<string, float> onObstacleSpawn;

    public float spawnInterval = 10f; // cada 30 s
    private float nextSpawn;

    private bool spawningEnabled = false;

    private void Awake()
    {
        obstaclePositions = new List<Transform>();
        occupied = new HashSet<Transform>();
        spawnedObstacles = new List<GameObject>();

        foreach (Transform position in internalPositions)
        {
            obstaclePositions.Add(position);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        nextSpawn = spawnInterval + Time.time;
    }

    // Update is called once per frame
    private void Update()
    {
        if (!spawningEnabled) return;
        if (Time.time >= nextSpawn)
        {
            SpawnObstacle();
            nextSpawn = Time.time + spawnInterval;
        }
    }

    private void SpawnObstacle()
    {
        // buscar spots libres
        List<Transform> freeSpots = new List<Transform>();
        foreach (var s in obstaclePositions)
            if (!occupied.Contains(s)) freeSpots.Add(s);

        if (freeSpots.Count == 0) return; // ya llenos
        Transform spot = null;
        List<Transform> validSpots = new List<Transform>();
        float sqrMinDistance = playerSpawnDist * playerSpawnDist;
        foreach (Transform currentSpot in freeSpots)
        {
            float dx = currentSpot.position.x - player.position.x;
            float dz = currentSpot.position.z - player.position.z;
            float d = dx * dx + dz * dz;
            if (d > sqrMinDistance)
            {
                validSpots.Add(currentSpot);
            }

        }

        if (validSpots.Count == 0)
        {
            Debug.Log("Player adjacent to all free spots.");
            return;
        }

        // elegir spot aleatorio
        spot = validSpots[UnityEngine.Random.Range(0, validSpots.Count)];

        //elegir obstaculo aleatoria de la lista de prefabs
        int index = Random.Range(0, obstaclePrefabs.Count);
        GameObject obstacle = obstaclePrefabs[index];

        GameObject instance = Instantiate(obstacle, spot.position, Quaternion.identity);
        occupied.Add(spot);
        spawnedObstacles.Add(instance);
        String spawnMessage = "";
        //GasObstacles necesitan timeController
        if (instance.TryGetComponent<ToxicGas>(out ToxicGas gas))
        {
            gas.AddTimerController(timer);
            spawnMessage = "PRECAUCION: Se ha detectado una bolsa de gas tóxico";
        }
        else if (instance.TryGetComponent<OilSpill>(out OilSpill oil))
        {
            spawnMessage = "PRECAUCION: Se ha derramado aceite resbaladizo";
        }
        else
        {
            spawnMessage = "PRECAUCION: Ha ocurrido un derrumbe";
        }

        onObstacleSpawn?.Invoke(spawnMessage, 3f);


    }

    public void ResetObstacleSpawner()
    {
        if (spawnedObstacles.Count == 0) return;

        foreach (var item in spawnedObstacles)
        {
            Destroy(item);
        }

        // Vaciar listas
        spawnedObstacles.Clear();
        occupied.Clear();

        // Reiniciar temporizador de spawn
        nextSpawn = Time.time + spawnInterval;
        ResumeSpawning();
    }

    public void ResumeSpawning()
    {
        spawningEnabled = true;
    }

    public void StopSpawning()
    {
        spawningEnabled = false;
    }
}
