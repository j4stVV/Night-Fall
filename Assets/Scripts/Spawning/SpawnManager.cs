using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    int currentWaveIndex; //The index of the current wave
    int currentWaveSpawnCount = 0;

    public WaveData[] data;
    public Camera referenceCamera;

    [Tooltip("If there are more than this number of enemies, stop spawning any more.")]
    public int maximumEnemyCount = 300;
    float spawnTimer;
    float currentWaveDuration = 0f;
    public bool boostedByCurse = true;

    public static SpawnManager Instance;

    private void Start()
    {
        if (Instance) Debug.LogWarning("There is more than 1 Spawn Manager in the Scene");
        Instance = this;
    }

    private void Update()
    {
        spawnTimer -= Time.deltaTime;
        currentWaveDuration += Time.deltaTime;

        if (spawnTimer <= 0f)
        {
            if (HasWaveEnded())
            {
                currentWaveIndex++;
                Debug.Log(string.Format("Wave Index {0}", currentWaveIndex));
                currentWaveDuration = currentWaveSpawnCount = 0;

                if (currentWaveIndex >= data.Length)
                {
                    Debug.Log("All wave has been spawned! Shutting down", this);
                    enabled = false;
                }
                return;
            }

            if (!CanSpawn())
            {
                ActivateCooldown();
                return;
            }

            GameObject[] spawns = data[currentWaveIndex].GetSpawns(EnemyStats.count);

            foreach(GameObject prefab in spawns)
            {
                if (!CanSpawn()) continue;

                Instantiate(prefab, GeneratePosition(), Quaternion.identity);
                currentWaveSpawnCount++;
            }

            ActivateCooldown();
        }
    }

    public void ActivateCooldown()
    {
        float curseBoost = boostedByCurse ? GameManager.GetCumulativeCurse() : 1;
        spawnTimer += data[currentWaveIndex].GetSpawnInterval() / curseBoost;
    }

    public bool CanSpawn()
    {
        if (HasExceedMaxEnemies()) return false;    

        if (Instance.currentWaveSpawnCount > Instance.data[Instance.currentWaveIndex].totalSpawns) return false;

        if (Instance.currentWaveDuration > Instance.data[Instance.currentWaveIndex].duration) return false;

        return true;
    }

    public static bool HasExceedMaxEnemies()
    {
        if (!Instance) return false;
        if (EnemyStats.count > Instance.maximumEnemyCount) return true;
        return false;
    }

    public bool HasWaveEnded()
    {
        WaveData currentWave = data[currentWaveIndex];

        if ((currentWave.exitConditions & WaveData.ExitCondition.waveDuration) > 0)
        {
            if (currentWaveDuration < currentWave.duration) return false;
        }

        if ((currentWave.exitConditions & WaveData.ExitCondition.reachedTotalSpawns) > 0)
        {
            if (currentWaveSpawnCount < currentWave.totalSpawns) return false;
        }

        if (currentWave.mustKillAll && EnemyStats.count > 0)
        {
            return false;
        }

        return true;
    }

    private void Reset()
    {
        referenceCamera = Camera.main;
    }

    public static Vector3 GeneratePosition()
    {
        if (!Instance.referenceCamera) Instance.referenceCamera = Camera.main;

        if (!Instance.referenceCamera.orthographic) Debug.LogWarning("This camera is not orthographic");

        float x = Random.Range(0f, 1f), y = Random.Range(0f, 1f);

        switch(Random.Range(0, 2))
        {
            case 0: default:
                return Instance.referenceCamera.ViewportToWorldPoint(new Vector3(Mathf.Round(x), y));
            case 1:
                return Instance.referenceCamera.ViewportToWorldPoint(new Vector3(x, Mathf.Round(y)));
        }
    }

    public static bool IsWithinBoundaries(Transform checkObject)
    {
        Camera cam = Instance && Instance.referenceCamera ? Instance.referenceCamera : Camera.main;

        Vector2 viewport = cam.WorldToViewportPoint(checkObject.position);
        if (viewport.x < 0f || viewport.x > 1f) return false;
        if (viewport.y < 0f || viewport.y > 1f) return false;
        return true;
    }
}
