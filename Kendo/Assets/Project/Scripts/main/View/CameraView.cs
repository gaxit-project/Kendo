using UnityEngine;
using Main.Presenter; // MapPresenterの名前空間を適切に指定してください

public class CameraView : MonoBehaviour
{
    private GameObject _player;
    private Vector3 _baseOffset;
    private Camera _controlledCamera;
    private MapPresenter _mapPresenter;

    [Header("Camera Movement Settings")]
    [Tooltip("カメラが目標位置に滑らかに追従する際の時間。小さいほど速く追従します。")]
    [SerializeField] private float cameraSmoothTime = 0.1f;
    [Tooltip("カメラの最大追従速度。")]
    [SerializeField] private float cameraMaxSpeed = Mathf.Infinity; // デフォルトは制限なし
    
    private Vector3 _cameraVelocity = Vector3.zero; // カスタムSmoothDampで使用する現在のカメラ速度

    public void Initialize(GameObject playerToFollow, Vector3 initialOffset, MapPresenter mapInfoProvider, Camera cameraComponent)
    {
        _player = playerToFollow;
        _baseOffset = initialOffset;
        _mapPresenter = mapInfoProvider;
        _controlledCamera = cameraComponent;

        if (_controlledCamera == null)
        {
            Debug.LogError("[CameraView] Camera component not provided during initialization.");
            enabled = false;
            return;
        }

        if (!_controlledCamera.orthographic)
        {
            Debug.LogWarning("[CameraView] This script is designed for an orthographic camera. Clamping behavior might be unexpected for perspective cameras.");
        }
        
        if (_player != null)
        {
            transform.position = _player.transform.position + _baseOffset;
            SetCameraPosition(); 
        }
    }

    public void SetCameraPosition()
    {
        if (_player == null || _controlledCamera == null || _mapPresenter == null)
        {
            return;
        }

        Vector3 desiredPosition = _player.transform.position + _baseOffset;
        float camHalfHeight = _controlledCamera.orthographicSize;
        float camHalfWidth = _controlledCamera.orthographicSize * _controlledCamera.aspect;
        float mapHalfSideLength = _mapPresenter.GetCurrentMapSize();
        
        float minMapX = -mapHalfSideLength;
        float maxMapX = mapHalfSideLength;
        float minMapZ = -mapHalfSideLength;
        float maxMapZ = mapHalfSideLength;

        float minAllowedCameraX = minMapX + camHalfWidth;
        float maxAllowedCameraX = maxMapX - camHalfWidth;
        float minAllowedCameraZ = minMapZ + camHalfHeight;
        float maxAllowedCameraZ = maxMapZ - camHalfHeight;

        Vector3 targetClampedPosition = desiredPosition;

        if (minAllowedCameraX > maxAllowedCameraX)
        {
            targetClampedPosition.x = (minMapX + maxMapX) / 2f; 
        }
        else
        {
            targetClampedPosition.x = Mathf.Clamp(desiredPosition.x, minAllowedCameraX, maxAllowedCameraX);
        }

        if (minAllowedCameraZ > maxAllowedCameraZ)
        {
            targetClampedPosition.z = (minMapZ + maxMapZ) / 2f;
        }
        else
        {
            targetClampedPosition.z = Mathf.Clamp(desiredPosition.z, minAllowedCameraZ, maxAllowedCameraZ);
        }
        
        targetClampedPosition.y = desiredPosition.y;

        
        // 自作のSmoothDampメソッドを使用する代わりに、Unity標準のSmoothDampを使用
        transform.position = Vector3.SmoothDamp(
            transform.position,     // 現在の位置
            targetClampedPosition,  // 目標位置
            ref _cameraVelocity,    // 現在の速度 (参照渡しで更新される)
            cameraSmoothTime        // 目標に到達するまでのおおよその時間
        );
        
        
        /*
        // 自作のSmoothDampメソッドを使用
        transform.position = MathModel.SmoothDampCustom(
            transform.position,
            targetClampedPosition,
            ref _cameraVelocity,
            cameraSmoothTime,
            cameraMaxSpeed,
            Time.deltaTime // Update/LateUpdateから呼ばれる場合はTime.deltaTime
        );
        */
    }
}