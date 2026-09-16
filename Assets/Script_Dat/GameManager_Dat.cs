using UnityEngine;

public class GameManager_Dat : MonoBehaviour
{
    public static GameManager_Dat Instance;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    void Awake()
    {
        Instance = this;
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Win()
    {
        winPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}