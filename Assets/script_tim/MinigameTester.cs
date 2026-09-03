using UnityEngine;

public class MinigameTester : MonoBehaviour
{
    void Update()
    {
        // Touche W = gagner, touche L = perdre
        if (Input.GetKeyDown(KeyCode.W))
            GameManager.Instance.MinigameWon();

        if (Input.GetKeyDown(KeyCode.L))
            GameManager.Instance.MinigameLost();
    }
}