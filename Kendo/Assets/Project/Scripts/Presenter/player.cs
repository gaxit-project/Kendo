using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class player : MonoBehaviour
{
    // InputActionAsset�ւ̎Q��
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private float _moveSpeed = 5f;

    //�e�N�X�`���ύX�p
    private Renderer rend;
    [SerializeField] private float switchInterval = 0.3f;
    private float timer = 0f;
    [SerializeField] private Texture idleTexture;
    [SerializeField] private Texture[] textures;
    private int textureIndex = 0;

    //���G�p
    [SerializeField] private float invincibleTime = 1.0f; // ���G����
    [SerializeField] private float blinkInterval = 0.1f;   // �_�ŊԊu
    private bool isInvincible = false; // ���G�t���O


    private Vector2 _moveInput;

    private void Awake()
    {
        _moveAction.action.performed += OnMove;
        _moveAction.action.canceled += OnMove;
        rend = GetComponent<Renderer>();
        rend.material.mainTexture = idleTexture;
    }

    private void OnDestroy()
    {
        _moveAction.action.performed -= OnMove;
        _moveAction.action.canceled -= OnMove;
    }

    private void OnEnable() => _moveAction.action.Enable();
    private void OnDisable() => _moveAction.action.Disable();

    private void Update()
    {
        // x-z���ʂňړ�����
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);

        // �ړ����Ă���Ƃ�����������ς���
        if (move.sqrMagnitude > 0.01f)
        {
            //�ړ����͉����o�Ă�2��ނ̉摜�����݂ɐ؂�ւ���
            timer += Time.deltaTime;
            if (timer >= switchInterval)
            {
                // �e�N�X�`�������݂ɐ؂�ւ���
                textureIndex = 1 - textureIndex;
                rend.material.mainTexture = textures[textureIndex];
                timer = 0f;
            }

            // �L�����̌������ړ������ɕύX
            transform.forward = move.normalized;
        }
        else
        {
            rend.material.mainTexture = idleTexture;
            timer = 0f;
            textureIndex = 0;
        }

        // ���ۂ̈ړ�����
        transform.position += move * _moveSpeed * Time.deltaTime;
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // �G��G�e�Ƃ̏Փ˂Ń_���[�W
    private void OnTriggerEnter(Collider other)
    {
        if (isInvincible) return; // ���G���͕Ԃ�

        if (other.CompareTag("Mob") || other.CompareTag("Bullet"))
        {
            StartCoroutine(InvincibleCoroutine());
            PlayerHP.Instance?.TakeDamage();

            if (other.CompareTag("Bullet"))
            {
                other.gameObject.SetActive(false);// �e����
            }
        }
    }

    //���G����
    private IEnumerator InvincibleCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibleTime)
        {
            rend.enabled = false; // �����_���[���\����
            yield return new WaitForSeconds(blinkInterval);
            rend.enabled = true; // �����_���[��\��
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval * 2;
        }

        rend.enabled = true;
        isInvincible = false;
    }


}
