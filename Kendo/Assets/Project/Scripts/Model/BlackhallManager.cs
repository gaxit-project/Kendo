using UnityEngine;
using System.Collections;

public class BlackhallManager : MonoBehaviour
{
    [Header("SuckOption")]
    [SerializeField] private string mobTag = "Mob";
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float suckDuration = 1.5f; // �z�����݂ɂ����鎞��


    [Header("ExpandOption")]
    /*[SerializeField] private CircleManager circleManager;
    [SerializeField] private Vector3 initialScale;
    [SerializeField] private int DestroyedEnemy;
    [SerializeField] private int NumExpand = 1; // ��萔�Ŋg��
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private GameObject barrier;
    */
    [SerializeField] private Vector3 initialScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float expandRatePerSecond = 0.1f;  // ���Ԃ��Ƃ̊g�嗦
    [SerializeField] private float shrinkFactor = 0.9f;         // �z�����݂��Ƃ̏k�����i��F90%�j
    //[SerializeField] private GameObject barrier;

    //[SerializeField] private CircleManager circleManager;


    private void Awake()
    {
        initialScale = transform.localScale;
    }

    private void Start()
    {
        transform.localScale = initialScale;
    }

    private void Update()
    {
        // ���b�������傫������
        Vector3 scale = transform.localScale;
        scale.x += expandRatePerSecond * Time.deltaTime;
        scale.z += expandRatePerSecond * Time.deltaTime;
        scale.y = initialScale.y; // �����͈ێ�
        transform.localScale = scale;

        //circleManager?.UpdateCircleScale(transform.localScale);


        // �X�P�[���̉����i�ŏ��l��initialScale���Ȃ킿�����l�j
        transform.localScale = new Vector3(
        Mathf.Max(transform.localScale.x, initialScale.x),
        initialScale.y,
        Mathf.Max(transform.localScale.z, initialScale.z)
        );
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(mobTag))
        {
            var mobController = other.GetComponent<MobController>();
            if (mobController != null && mobController.GetIsKnockback())
            {
                // �m�b�N�o�b�N�� �� �z�����݁{�j��{�K�`���{�k��
                StartCoroutine(SuckAndDestroy(other.gameObject));
            }
            else
            {
                // �m�b�N�o�b�N���łȂ� �� �z�����݁{�j��̂�
                StartCoroutine(SuckAndDestroyOnly(other.gameObject));
            }
            //StartCoroutine(SuckAndDestroy(other.gameObject));
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
        
        // �u���b�N�z�[���������k��
        Vector3 newScale = transform.localScale;
        newScale.x = Mathf.Max(newScale.x * shrinkFactor, initialScale.x);
        newScale.z = Mathf.Max(newScale.z * shrinkFactor, initialScale.z);
        newScale.y = initialScale.y; 
        transform.localScale = newScale;

        //circleManager?.UpdateCircleScale(newScale);

        //�����ɃX�R�A������
        ScoreManager.Instance?.AddKill();
    }

    //�m�b�N�o�b�N������Ȃ��Ƃ��͋z�����܂�Ĕj�󂷂邾��
    private IEnumerator SuckAndDestroyOnly(GameObject mob)
    {
        SoundSE.Instance?.Play("warp");

        Rigidbody rb = mob.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Vector3 startPos = mob.transform.position;
        Vector3 endPos = transform.position;
        Vector3 startScale = mob.transform.localScale;
        Vector3 endScale = startScale * 0.5f;
        float timer = 0f;
        float rotationSpeed = 180f;

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

        // �j��̂݁i�K�`����k���Ȃ��j
        Destroy(mob);
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
    /*
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
    */
}
