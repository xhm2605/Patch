using UnityEngine;

public class CameraFollow_Dat : MonoBehaviour
{
    public Transform target;      // kéo Player vào đây
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 1, -10);

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}