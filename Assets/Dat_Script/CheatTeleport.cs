using UnityEngine;

public class CheatTeleport : MonoBehaviour
{
    [Header("Player Target")]
    public Transform playerTransform;

    [Header("Checkpoint Transforms")]
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;

    private Rigidbody2D playerRb;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            TeleportTo(point1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
        {
            TeleportTo(point2);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
        {
            TeleportTo(point3);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
        {
            TeleportTo(point4);
        }
    }

    private void TeleportTo(Transform targetPoint)
    {
        if (playerTransform == null)
        {
            Debug.LogError("Player Transform is not assigned in the Inspector!");
            return;
        }

        if (targetPoint == null)
        {
            Debug.LogWarning("Target checkpoint transform is not assigned in the Inspector!");
            return;
        }

        // Teleport player
        playerTransform.position = targetPoint.position;

        // Reset velocity
        if (playerRb == null)
        {
            playerRb = playerTransform.GetComponent<Rigidbody2D>();
        }

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero; // Hoặc playerRb.velocity = Vector2.zero nếu dùng Unity bản cũ
        }

        Debug.Log($"[Cheat] Teleported player to: {targetPoint.name}");
    }
}