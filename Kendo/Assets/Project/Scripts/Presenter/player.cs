using UnityEngine;
using UnityEngine.InputSystem;

public class player : MonoBehaviour
{
    // InputActionAssetへの参照
    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private float _moveSpeed = 5f;

    //テクスチャ変更用
    private Renderer rend;
    [SerializeField] private float switchInterval = 0.3f;
    private float timer = 0f;
    [SerializeField] private Texture idleTexture;
    [SerializeField] private Texture[] textures;
    private int textureIndex = 0;

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
        // x-z平面で移動する
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);

        // 移動しているときだけ向きを変える
        if (move.sqrMagnitude > 0.01f)
        {
            //移動中は炎が出てる2種類の画像を交互に切り替える
            timer += Time.deltaTime;
            if (timer >= switchInterval)
            {
                // テクスチャを交互に切り替える
                textureIndex = 1 - textureIndex;
                rend.material.mainTexture = textures[textureIndex];
                timer = 0f;
            }

            // キャラの向きを移動方向に変更
            transform.forward = move.normalized;
        }
        else
        {
            rend.material.mainTexture = idleTexture;
            timer = 0f;
            textureIndex = 0;
        }

        // 実際の移動処理
        transform.position += move * _moveSpeed * Time.deltaTime;
    }


    private void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    // 敵や敵弾との衝突でダメージ
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Mob") || other.CompareTag("Bullet"))
        {
            PlayerHP.Instance?.TakeDamage();

            if (other.CompareTag("Bullet"))
            {
                other.gameObject.SetActive(false);// 弾消す
            }
        }
    }

}
