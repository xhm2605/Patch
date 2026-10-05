using UnityEngine;

public class FlagGoal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager_Dat.Instance.Win();
        }
    }
}