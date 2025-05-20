using UnityEngine;
using System.Collections;

public class ruretto : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 100f;
    [SerializeField] private float reverseDuration = 2f;
    private float reverseTimer = 0f;

    private bool isRotating = true;
    private bool isReverse = false;
    private bool isStoppedByBomb = false;

    void Update()
    {
        // �{�����L���ɂȂ������~
        if (PlayerBom.bom && !isStoppedByBomb)
        {
            StopRouletteForSeconds(10f);
            return;
        }

        if (!isRotating || isStoppedByBomb)
            return;

        // ��]�����i�ʏ�^�t�j
        float direction = isReverse ? -1f : 1f;
        transform.Rotate(Vector3.up * direction * rotateSpeed * Time.deltaTime);

        // ��莞�Ԃŉ�]�����؂�ւ�
        reverseTimer += Time.deltaTime;
        if (reverseTimer >= reverseDuration)
        {
            isReverse = !isReverse;
            reverseTimer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mob"))
        {
            var mob = other.GetComponent<MobController>();
            if (mob != null /*&& mob.hit*/)
            {
                isRotating = false;
                Debug.Log("Mob��hit�Ń��[���b�g��~");
                other.gameObject.SetActive(false);
                // �X�R�A���Z�iMob���|���ꂽ�Ƃ��j
                ScoreManager.Instance?.AddKill();
            }
        }
    }

    private void StopRouletteForSeconds(float duration)
    {
        if (isStoppedByBomb) return;
        StartCoroutine(StopRoutine(duration));
    }

    private IEnumerator StopRoutine(float duration)
    {
        isStoppedByBomb = true;
        bool wasRotatingBefore = isRotating;
        isRotating = false;

        Debug.Log("�{���Ń��[���b�g��~");

        yield return new WaitForSeconds(duration);

        PlayerBom.bom = false;      // �{����Ԃ�����
        isStoppedByBomb = false;
        isRotating = true;

        Debug.Log("10�b�o�� �� ���[���b�g�ĊJ");
    }
}
