using UnityEngine;

public class FlagGoal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("CHIẾN THẮNG! Mario đã tới cờ!");
            Time.timeScale = 0f;
            // Sau này thay bằng: hiện UI "You Win" giống Game Over
        }
    }
}