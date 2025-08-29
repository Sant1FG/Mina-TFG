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
    public event Action<string, float> onObstacleSpawn;
    private Dictionary<Vector3, GameObject> obstacleDictionary;
    private HashSet<Vector3> activatedPositions;
    private List<Vector3> obstaclePositions;

    public float spawnInterval = 10f;
    private float nextSpawn;

    private bool spawningEnabled = false;

    private void Awake()
    {
        obstacleDictionary = new Dictionary<Vector3, GameObject>();
        activatedPositions = new HashSet<Vector3>();
        obstaclePositions = new List<Vector3>();

        foreach (Transform transform in internalPositions)
        {
            obstaclePositions.Add(transform.position);
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
            ActivateRandomObstacle();
            nextSpawn = Time.time + spawnInterval;
        }
    }

    private void InstantiateObstacles()
    {
        foreach (Vector3 position in obstaclePositions)
        {
            int index = Random.Range(0, obstaclePrefabs.Count);
            GameObject obstacle = obstaclePrefabs[index];
            GameObject instance = Instantiate(obstacle,position, Quaternion.identity);
            instance.SetActive(false);
            obstacleDictionary.Add(position,instance);
        }
    }

    private void ActivateRandomObstacle()
    {
        List<Vector3> freeSpots = new List<Vector3>();
        foreach (Vector3 s in obstacleDictionary.Keys) if (!activatedPositions.Contains(s)) freeSpots.Add(s);
        //Test if player is close enough to any obstacle
        if (freeSpots.Count == 0) return;
        List<Vector3> validSpots = new List<Vector3>();
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
            Debug.Log("Player adjacent to all free spots.");
            return;
        }

        // elegir spot aleatorio
        Vector3 randomValidPosition = validSpots[UnityEngine.Random.Range(0, validSpots.Count)];
        GameObject selected = obstacleDictionary[randomValidPosition];
        selected.SetActive(true);
        activatedPositions.Add(randomValidPosition);
        ActivateObstacleNotification(selected);

    }

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

        onObstacleSpawn?.Invoke(spawnMessage, 3f);
    }

    public void ResetObstacleSpawner()
    {
        if (obstaclePositions.Count == 0) return;

        foreach (var item in obstacleDictionary.Values)
        {
            if(item != null) Destroy(item);
        }

        // Vaciar listas
        obstacleDictionary.Clear();
        activatedPositions.Clear();

        InstantiateObstacles();
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
