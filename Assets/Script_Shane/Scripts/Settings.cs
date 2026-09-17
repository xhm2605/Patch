using UnityEngine;

public class Settings : MonoBehaviour
{
    private int lives;
    private int score;

    public GameObject successPanel;
    public GameObject failedPanel;

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
        score += 1000;
        successPanel.SetActive(true);
    }

    public void LevelFailed()
    {
        lives--;
        failedPanel.SetActive(true);
    }
}
