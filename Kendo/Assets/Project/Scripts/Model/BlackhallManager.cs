using UnityEngine;
using System.Collections;

public class BlackhallManager : MonoBehaviour
{
    [SerializeField] private string mobTag = "Mob";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float suckDuration = 1.5f; // �z�����݂ɂ����鎞��

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(mobTag))
        {
            //Destroy(other.gameObject);
            //GachaManager.Instance.Gacha();
            StartCoroutine(SuckAndDestroy(other.gameObject));
        }
        else if (other.CompareTag(playerTag))
        {
            PlayerHP.Instance.KillPlayer();

        }
    }
    private IEnumerator SuckAndDestroy(GameObject mob)
    {
        SoundSE.Instance?.Play("warp");

        // Rigidbody��ړ��𖳌���
        Rigidbody rb = mob.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // �J�n�ʒu�Ǝ��Ԃ̋L�^
        Vector3 startPos = mob.transform.position;
        Vector3 endPos = transform.position; // �u���b�N�z�[�����S
        Vector3 startScale = mob.transform.localScale;
        Vector3 endScale = startScale * 0.5f; // �ŏI�I��0.5�{��
        float timer = 0f;
        float rotationSpeed = 180f; // 1�b��360�x


        // �z�����܂��悤�ɏ��X�Ɉړ�
        while (timer < suckDuration)
        {
            if (mob == null) yield break;

            timer += Time.deltaTime;
            float t = timer / suckDuration;

            mob.transform.position = Vector3.Lerp(startPos, endPos, t);
            mob.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            mob.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // �Ō�ɔj��{�K�`����ri
        Destroy(mob);
        GachaManager.Instance.Gacha();
    }
}
