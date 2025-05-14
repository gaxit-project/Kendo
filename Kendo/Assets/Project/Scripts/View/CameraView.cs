using UnityEngine;

public class CameraView : MonoBehaviour
{
    private GameObject player;
    private Vector3 offset;

    public Vector3 Offset { get; private set; }

    public void Initialize(GameObject player, Vector3 offset)
    {
        this.player = player;
        Offset = offset;
        transform.position = player.transform.position + offset;
    }

    public void SetCameraPosition()
    {
        Vector3 target = new Vector3(player.transform.position.x, player.transform.position.y + 30f, player.transform.position.z);
        transform.position = target;//player.transform.position + offset;
    }
}
