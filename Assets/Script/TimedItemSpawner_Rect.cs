using System.Collections.Generic;
using UnityEngine;

public class TimedItemSpawner_Rect : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject coinPrefab;
    public GameObject diamondPrefab;
    public GameObject thunderPrefab;
    public GameObject poisonPrefab;

    [Header("Spawn Range")]
    public float minX = -20f;
    public float maxX = 65f;
    public float minZ = -20f;
    public float maxZ = 20f;
    public float spawnHeight = 4f;

    [Header("Spawn Rhythm")]
    public float spawnInterval = 5f;
    public int spawnPerWave = 3;

    [Header("Spawn Weights (total doesn't need to be 100, but keep ratios)")]
    [Range(0, 100)] public int coinWeight = 50;
    [Range(0, 100)] public int diamondWeight = 30;
    [Range(0, 100)] public int thunderWeight = 10;
    [Range(0, 100)] public int poisonWeight = 10;

    [Header("Limits")]
    public int maxAlive = 80;
    public float minDistanceFromPlayerSpawn = 6f;
    public float minSeparation = 1.2f;
    public int maxTriesPerItem = 20;

    [Header("Ground Check")]
    public LayerMask groundMask;
    public float raycastHeight = 30f;
    public float groundOffsetY = 0.15f;

    [Header("Player Spawn")]
    public Vector3 playerSpawn = new Vector3(0f, 4.72f, 0f);

    float timer;
    readonly List<Transform> alive = new List<Transform>();

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsEnded) return;

        Cleanup();

        if (alive.Count >= maxAlive) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnWave();
        }
    }

    void SpawnWave()
    {
        int canSpawn = Mathf.Min(spawnPerWave, maxAlive - alive.Count);
        for (int i = 0; i < canSpawn; i++)
            TrySpawnOne();
    }

    void TrySpawnOne()
    {
        GameObject prefab = PickPrefabByWeight();
        if (prefab == null) return;

        for (int attempt = 0; attempt < maxTriesPerItem; attempt++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            Vector3 pos = new Vector3(x, spawnHeight, z);

            // Raycast to ground
            Vector3 origin = pos + Vector3.up * raycastHeight;
            if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit,
                    raycastHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
                continue;

            Vector3 finalPos = hit.point + Vector3.up * groundOffsetY;

            // too close to spawn
            if (Vector3.Distance(finalPos, playerSpawn) < minDistanceFromPlayerSpawn)
                continue;

            // avoid overlap
            if (!FarEnough(finalPos))
                continue;

            GameObject obj = Instantiate(prefab, finalPos, Quaternion.identity);
            alive.Add(obj.transform);
            return;
        }
    }

    GameObject PickPrefabByWeight()
    {
        int total = coinWeight + diamondWeight + thunderWeight + poisonWeight;
        if (total <= 0) return null;

        int r = Random.Range(0, total);

        if (r < coinWeight) return coinPrefab;
        r -= coinWeight;

        if (r < diamondWeight) return diamondPrefab;
        r -= diamondWeight;

        if (r < thunderWeight) return thunderPrefab;
        return poisonPrefab;
    }

    bool FarEnough(Vector3 pos)
    {
        float sqr = minSeparation * minSeparation;
        for (int i = 0; i < alive.Count; i++)
        {
            if (alive[i] == null) continue;
            if ((pos - alive[i].position).sqrMagnitude < sqr)
                return false;
        }
        return true;
    }

    void Cleanup()
    {
        for (int i = alive.Count - 1; i >= 0; i--)
            if (alive[i] == null)
                alive.RemoveAt(i);
    }
}