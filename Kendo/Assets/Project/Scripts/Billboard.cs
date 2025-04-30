using UnityEngine;



/// <summary>
/// 常にカメラの方を向くオブジェクト回転をカメラに固定
/// </summary>
public class Billboard : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField]
    private float offset;

    private void Awake()
    {
        // 回転をカメラと同期させる
        rectTransform = GetComponent<RectTransform>();
    }
    
    void LateUpdate()
    {
        // 回転をカメラと同期させる
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y - offset, Camera.main.transform.position.z);
        transform.rotation = Camera.main.transform.rotation;

        // Orthographic Sizeを基にCanvasサイズを計算
        float height = Camera.main.orthographicSize * 2f;
        float width = height * Camera.main.aspect;

        // RectTransformのサイズを設定
        rectTransform.sizeDelta = new Vector2(width, height);
    }
}