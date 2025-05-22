using UnityEngine;
using System.Collections.Generic;

public class CircleManager : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.8f;
    [SerializeField] private float rotateSpeed = 30f;

    private List<GameObject> obstacles = new List<GameObject>();

    private void Start()
    {
        SpawnObstacles();
    }

    private void Update()
    {
        RotateObstacles();
    }

    private void SpawnObstacles()
    {
        int count = 16;
        float angleStep = 22.5f;

        for (int i = 0; i < count; i++)
        {
            if (Random.value > spawnChance)
                continue;

            float angle = i * angleStep;

            // ������]�iX��90�x�Q�����āAY��]�j
            Quaternion rotation = Quaternion.Euler(90, angle, 0);
            GameObject obj = Instantiate(obstaclePrefab, Vector3.zero, rotation);

            // ��Q�����X�g�ɒǉ�
            obstacles.Add(obj);
        }
    }

    private void RotateObstacles()
    {
        foreach (GameObject obj in obstacles)
        {
            if (obj == null) continue;

            Vector3 euler = obj.transform.rotation.eulerAngles;
            euler.y = (euler.y + rotateSpeed * Time.deltaTime) % 360f; // Y�������Z�����[�v
            obj.transform.rotation = Quaternion.Euler(euler);
        }
    }
}
