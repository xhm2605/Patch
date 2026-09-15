using UnityEngine;

public class GameManager_Dat : MonoBehaviour
{
    public static GameManager_Dat Instance;  
    public GameObject gameOverPanel;

    void Awake()
    {
        Instance = this;
    }

    public void GameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f; // đứng hình game lại
    }
}