using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MapController : MonoBehaviour
{
    [SerializeField] private List<GameObject> terrainChunks;
    [SerializeField] private GameObject player;
    [SerializeField] private float checkerRadius;
    [SerializeField] private LayerMask terrainMask;
    public GameObject currentChunk;
    Vector3 playerLastPosition;

    [Header("Optimization")]
    [SerializeField] private List<GameObject> spawnedChunks;
    private GameObject latestChunk;
    [SerializeField] private float maxOpDist;
    private float opDist;
    private float optimizerCooldown;
    [SerializeField] private float optimizerCooldownDuration;


    void Start()
    {
        playerLastPosition = player.transform.position; 
    }

    void Update()
    {
        ChunkChecker();
        ChunkOptimizer();
    }

    void ChunkChecker()
    {
        if (!currentChunk)
        {
            return;   
        }
        
        Vector3 moveDir = player.transform.position - playerLastPosition;
        playerLastPosition = player.transform.position;

        string directionName = GetDirectionName(moveDir);
        
        CheckAndSpawnChunK(directionName);

        if (directionName.Contains("Up"))
        {
            CheckAndSpawnChunK("Up");
        }
        if (directionName.Contains("Down"))
        {
            CheckAndSpawnChunK("Down");
        }
        if (directionName.Contains("Right"))
        {
            CheckAndSpawnChunK("Right");
        }
        if (directionName.Contains("Left"))
        {
            CheckAndSpawnChunK("Left");
        }
    }

    void CheckAndSpawnChunK(string direction)
    {
        if (!Physics2D.OverlapCircle(currentChunk.transform.Find(direction).position, checkerRadius, terrainMask))
        {
            SpawnChunk(currentChunk.transform.Find(direction).position);
        }
    }

    string GetDirectionName(Vector3 direction)
    {
        direction = direction.normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            //Moving horizontally more than vertically
            if (direction.y > 0.5f)
            {
                return direction.x > 0 ? "Right Up" : "Left Up";
            }
            else if (direction.y < -0.5f) 
            {
                return direction.x > 0 ? "Right Down" : "Left Down";
            }
            else
            {
                return direction.x > 0 ? "Right" : "Left";
            }

        } 
        else
        {
            //Moving vertically more than horizontally
            if (direction.x > 0.5f)
            {
                return direction.y > 0 ? "Right Up" : "Left Up";
            }
            else if (direction.x < -0.5f)
            {
                return direction.y > 0 ? "Right Down" : "Left Down";
            }
            else
            {
                return direction.y > 0 ? "Up" : "Down";
            }
        }
    }

    void SpawnChunk(Vector3 spawnPosition)
    {
        int rand = Random.Range(0, terrainChunks.Count);
        latestChunk = Instantiate(terrainChunks[rand], spawnPosition, Quaternion.identity);
        spawnedChunks.Add(latestChunk);
    }

    void ChunkOptimizer()
    {
        optimizerCooldown -= Time.deltaTime;
        if (optimizerCooldown <= 0f)
        {
            optimizerCooldown = optimizerCooldownDuration;
        }
        else
        {
            return;
        }
        foreach(GameObject chunk  in spawnedChunks)
        {
            opDist = Vector3.Distance(player.transform.position, chunk.transform.position);
            if (opDist > maxOpDist)
            {
                chunk.SetActive(false);
            }
            else
            {
                chunk.SetActive(true); 
            }
        }
    }
}
