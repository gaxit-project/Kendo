using UnityEngine;

public class test : MonoBehaviour
{
    public GameObject destroyEffectPrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Vector3 spawnPos = transform.position + Vector3.up * 1f;
            Quaternion spawnRot = Quaternion.Euler(90f, 0f, 0f);  // XŽ²‚É90“x‰ñ“]

            GameObject effect = Instantiate(destroyEffectPrefab, spawnPos, spawnRot);
            Destroy(effect, 1f);
        }
    }
}
