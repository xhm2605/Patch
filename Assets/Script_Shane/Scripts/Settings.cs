using UnityEngine;

public class Settings : MonoBehaviour
{
    private int lives;
    private int score;

    public GameObject successPanel;
    public GameObject failedPanel;

    private bool ended;

    private void Start()
    {
        NewGame();
        successPanel.SetActive(false);
        failedPanel.SetActive(false);
    }

    private void NewGame()
    {
        lives = 3;
        score = 0;
    }

    public void LevelComplete()
    {
        if (ended) return;

        score += 1000;
        successPanel.SetActive(true);
        StartCoroutine(EndMinigame(true));
    }

    public void LevelFailed()
    {
        if (ended) return;

        lives--;
        failedPanel.SetActive(true);
        StartCoroutine(EndMinigame(false));
    }

    private System.Collections.IEnumerator EndMinigame(bool won)
    {
        ended = true;

        yield return new WaitForSecondsRealtime(1.5f);

        Time.timeScale = 1f;

        if (GameManager.Instance == null) yield break;

        if (won) GameManager.Instance.MinigameWon();
        else GameManager.Instance.MinigameLost();
    }
}