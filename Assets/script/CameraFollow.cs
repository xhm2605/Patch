using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;              // le joueur à suivre
    public float smoothSpeed = 5f;        // douceur du mouvement
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}