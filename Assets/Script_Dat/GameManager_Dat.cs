using UnityEngine;

public class GameManager_Dat : MonoBehaviour
{
    public static GameManager_Dat Instance;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    private bool ended = false;

    void Awake()
    {
        Instance = this;
    }

    public void GameOver()
    {
        if (ended) return;

        gameOverPanel.SetActive(true);
        StartCoroutine(EndMinigame(false));
    }

    public void Win()
    {
        if (ended) return;

        winPanel.SetActive(true);
        StartCoroutine(EndMinigame(true));
    }

    private System.Collections.IEnumerator EndMinigame(bool won)
    {
        ended = true;
        Time.timeScale = 1f;

        yield return new WaitForSeconds(1.5f);

        if (GameManager.Instance == null) yield break;

        if (won) GameManager.Instance.MinigameWon();
        else GameManager.Instance.MinigameLost();
    }
}