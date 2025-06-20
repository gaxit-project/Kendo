using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircleManager : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.8f;
    [SerializeField] private float rotateSpeed = 30f;
    [SerializeField] private Vector3 initialCircleScale = new Vector3(1500f, 1500f, 172f);

    private float currentSpeed = 0f;
    private List<GameObject> obstacles = new List<GameObject>();
    private bool checkingDeactivation = false;

    private void Start()
    {
        SpawnObstacles();
    }

    private void Update()
    {
        currentSpeed = PlayerBom.bom ? 0f : rotateSpeed;
        RotateObstacles();

        // 非アクティブチェック中でなければ開始
        if (!checkingDeactivation && AllObstaclesInactive())
        {
            checkingDeactivation = true;
            StartCoroutine(RespawnAfterDelay(10f));
        }
    }

    private void SpawnObstacles()
    {
        obstacles.Clear();

        int count = 16;
        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            if (Random.value > spawnChance)
                continue;

            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(90, angle, 0);
            Vector3 spawnPosition = new Vector3(0, -8, 0);
            GameObject obj = Instantiate(obstaclePrefab, spawnPosition, rotation);
            obj.transform.localScale = initialCircleScale;
            obstacles.Add(obj);
        }
    }

    private void RotateObstacles()
    {
        foreach (GameObject obj in obstacles)
        {
            if (obj == null || !obj.activeSelf) continue;

            Vector3 euler = obj.transform.rotation.eulerAngles;
            euler.y = (euler.y + currentSpeed * Time.deltaTime) % 360f;
            obj.transform.rotation = Quaternion.Euler(euler);
        }
    }

    private bool AllObstaclesInactive()
    {
        foreach (GameObject obj in obstacles)
        {
            if (obj != null && obj.activeSelf)
                return false;
        }
        return true;
    }

    private IEnumerator RespawnAfterDelay(float delay)
    {
        OnAllObstaclesDeactivated(); // 拡張処理呼び出し
        yield return new WaitForSeconds(delay);
        SpawnObstacles();
        checkingDeactivation = false;
    }

    // 障害物がすべて無効化されたときに実行される処理
    private void OnAllObstaclesDeactivated()
    {
        Debug.Log("全障害物が無効化されました。ここで演出や音などを追加できます。");
        // TODO: エフェクト・SE・UI演出などを追加してもいい
    }

    public void UpdateCircleScale(Vector3 blackHoleScale)
    {
        float scaleRatio = blackHoleScale.x / 18f;

        foreach (GameObject obj in obstacles)
        {
            if (obj == null) continue;

            obj.transform.localScale = initialCircleScale * scaleRatio;

            Collider col = obj.GetComponent<Collider>();
            if (col is SphereCollider sc)
            {
                sc.radius = 1f * scaleRatio; // 必要に応じて半径初期値に合わせて調整
            }
        }
    }



}
