using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0f, 25f, -10f);
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (!player) return;

        Vector3 targetPos = player.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
