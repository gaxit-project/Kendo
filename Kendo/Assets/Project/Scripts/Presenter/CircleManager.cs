using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircleManager : MonoBehaviour
{
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.8f;
    [SerializeField] private float rotateSpeed = 30f;

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

        // ��A�N�e�B�u�`�F�b�N���łȂ���ΊJ�n
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
        float angleStep = 22.5f;

        for (int i = 0; i < count; i++)
        {
            if (Random.value > spawnChance)
                continue;

            float angle = i * angleStep;
            Quaternion rotation = Quaternion.Euler(90, angle, 0);
            GameObject obj = Instantiate(obstaclePrefab, Vector3.zero, rotation);

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
        OnAllObstaclesDeactivated(); // �g�������Ăяo��
        yield return new WaitForSeconds(delay);
        SpawnObstacles();
        checkingDeactivation = false;
    }

    // �g���|�C���g�F��Q�������ׂĖ��������ꂽ�Ƃ��Ɏ��s����鏈��
    private void OnAllObstaclesDeactivated()
    {
        Debug.Log("�S��Q��������������܂����B�����ŉ��o�≹�Ȃǂ�ǉ��ł��܂��B");
        // TODO: �G�t�F�N�g�ESE�EUI���o�Ȃǂ�ǉ�
    }
}
