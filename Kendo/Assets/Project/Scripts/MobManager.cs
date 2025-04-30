using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobManager : MonoBehaviour
{
    public static MobManager Instance { get; private set; }

    [SerializeField] private GameObject mobPrefab;
    [SerializeField] private int poolSize = 100;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private Transform playerTransform;

    private Queue<GameObject> mobPool = new Queue<GameObject>();
    private List<GameObject> activeMobs = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject mob = Instantiate(mobPrefab);
            mob.SetActive(false);
            mobPool.Enqueue(mob);
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnMobsRoutine());
    }

    private IEnumerator SpawnMobsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnMob();
        }
    }

    private void SpawnMob()
    {
        if (mobPool.Count > 0)
        {
            Vector3 spawnPos = GetRandomSpawnPosition(playerTransform.position, spawnRadius);
            GameObject mob = mobPool.Dequeue();
            mob.transform.position = spawnPos;
            mob.SetActive(true);
            activeMobs.Add(mob);
        }
    }

    public void ReleaseMob(GameObject mob)
    {
        mob.SetActive(false);
        activeMobs.Remove(mob);
        mobPool.Enqueue(mob);
    }

    private Vector3 GetRandomSpawnPosition(Vector3 center, float radius)
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * radius;
        return new Vector3(center.x + randomCircle.x, center.y, center.z + randomCircle.y);
    }
}
