using UnityEngine;



/// <summary>
/// 常にカメラの方を向くオブジェクト回転をカメラに固定
/// </summary>
public class Billboard : MonoBehaviour
{
    private RectTransform rectTransform;
    [SerializeField]
    private float offset;

    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
        // 回転をカメラと同期させる
        rectTransform = GetComponent<RectTransform>();
    }
    
    void LateUpdate()
    {
        // 回転をカメラと同期させる
        transform.position = new Vector3(camera.transform.position.x, camera.transform.position.y - offset, camera.transform.position.z);
        transform.rotation = camera.transform.rotation;

        // Orthographic Sizeを基にCanvasサイズを計算
        float height = camera.orthographicSize * 2f;
        float width = height * camera.aspect;

        // RectTransformのサイズを設定
        rectTransform.sizeDelta = new Vector2(width, height);
    }
}