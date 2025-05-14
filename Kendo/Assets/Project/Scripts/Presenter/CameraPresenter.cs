using UnityEngine;

public class CameraPresenter : MonoBehaviour
{
    private GameObject player;
    [SerializeField] Vector3 offset = new Vector3(0, 30, 0);

    [SerializeField] private CameraView view;

    void Awake()
    {
        //view = new CameraView();
    }

    void Start()
    {
        player = GameObject.Find("Player");

        view.Initialize(player, offset);
    }

    void LateUpdate()
    {

        view.SetCameraPosition();
    }
}
