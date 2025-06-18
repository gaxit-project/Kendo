using UnityEngine;
using System.Collections;

public class BlackhallManager : MonoBehaviour
{
    [Header("SuckOption")]
    [SerializeField] private string mobTag = "Mob";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float suckDuration = 1.5f; // �z�����݂ɂ����鎞��

    [Header("ExpandOption")]
    [SerializeField] private CircleManager circleManager;
    [SerializeField] private Vector3 initialScale;
    [SerializeField] private int DestroyedEnemy;
    [SerializeField] private int NumExpand = 1; // ��萔�Ŋg��
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private GameObject barrier;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void Start()
    {
        transform.localScale = initialScale;
    }

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
            //PlayerHP.Instance.KillPlayer();
            StartCoroutine(SuckAndKillPlayer(other.gameObject));

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
        DestroyedEnemy++;

        if (DestroyedEnemy == NumExpand)
        {
            ExpandBlackHoleAndBarrier();
            DestroyedEnemy = 0;
        }
    }

    private IEnumerator SuckAndKillPlayer(GameObject player)
    {
        SoundSE.Instance?.Play("warp");

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // PlayerMovement��R���g���[���X�N���v�g���~
        MonoBehaviour[] components = player.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            comp.enabled = false;
        }

        Vector3 startPos = player.transform.position;
        Vector3 endPos = transform.position;
        Vector3 startScale = player.transform.localScale;
        Vector3 endScale = startScale * 0.5f;
        float timer = 0f;
        float rotationSpeed = 180f;

        while (timer < suckDuration)
        {
            if (player == null) yield break;

            timer += Time.deltaTime;
            float t = timer / suckDuration;

            player.transform.position = Vector3.Lerp(startPos, endPos, t);
            player.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
            player.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        // �v���C���[���S����
        PlayerHP.Instance.KillPlayer(); // GameOver��ʂȂǂɈڍs
    }

    private void ExpandBlackHoleAndBarrier()
    {
        transform.localScale *= scaleMultiplier;

        if (barrier != null)
        {
            barrier.transform.localScale *= scaleMultiplier;

            // Collider�̃T�C�Y����
            SphereCollider sc = barrier.GetComponent<SphereCollider>();
            if (sc != null)
            {
                sc.radius *= scaleMultiplier;
            }
        }

        // CircleManager���̏�ǃI�u�W�F�N�g�Q���g��
        if (circleManager != null)
        {
            circleManager.ExpandCircle(scaleMultiplier);
        }
    }
}
