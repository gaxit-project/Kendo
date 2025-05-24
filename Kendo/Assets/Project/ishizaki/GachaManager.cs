using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance { get; private set; }

    [SerializeField] private Image gachaImage;           //�K�`����Image�R���|�[�l���g
    [SerializeField] private Sprite[] rollingSprites;    //�e��A�C�e���摜
    [SerializeField] private float rollInterval = 0.1f;  //�K�`���̉�]�Ԋu
    [SerializeField] private float totalRollTime = 2.0f; //�K�`���̉�]����

    private bool isRolling = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    //Mob���u���b�N�z�[���ɋz�����܂ꂽ��Ăяo��
    public void Gacha()
    {
        if (!isRolling)
        {
            StartCoroutine(GachaStart());
            Debug.Log("�K�`���X�^�[�g�I");
        }
        else
        {

        }
    }

    IEnumerator GachaStart()
    {
        isRolling = true;
        float timer = 0f;
        int index = 0;

        while (timer < totalRollTime)
        {
            gachaImage.sprite = rollingSprites[index % rollingSprites.Length];
            index++;
            timer += rollInterval;
            yield return new WaitForSeconds(rollInterval);
        }

        // �����_���ȃA�C�e����\��
        //index = Random.Range(0, rollingSprites.Length);
        //Sprite selected = rollingSprites[index];
        //gachaImage.sprite = selected;
        index = 0;
        gachaImage.sprite = rollingSprites[0];

        //���ʔ���
        switch (index)
        {
            case 0:
                health();
                break;
        }

        isRolling = false;
    }

    //���ʔ����p���\�b�h
    private void health()
    {
        int hp;

        PlayerHP.Instance.RecoverHP();
        hp = PlayerHP.Instance?.GetCurrentHP() ?? 0;
        Debug.Log("HP�񕜁F" + hp);
    }
}
