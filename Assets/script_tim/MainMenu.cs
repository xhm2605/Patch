using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;
    public Button difficultyButton;
    public TMP_Text difficultyLabel;

    void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }

        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(QuitGame);
        }

        if (difficultyButton != null)
        {
            difficultyButton.onClick.RemoveAllListeners();
            difficultyButton.onClick.AddListener(CycleDifficulty);

            if (difficultyLabel == null)
                difficultyLabel = difficultyButton.GetComponentInChildren<TMP_Text>();
        }

        RefreshLabel();
    }

    void CycleDifficulty()
    {
        GameSettings.difficulty = (GameSettings.difficulty + 1) % 3;
        RefreshLabel();
    }

    void RefreshLabel()
    {
        if (difficultyLabel == null) return;

        int minutes = Mathf.FloorToInt(GameSettings.TotalTime() / 60f);
        int seconds = Mathf.FloorToInt(GameSettings.TotalTime() % 60f);

        difficultyLabel.text = string.Format("DIFFICULTY : {0}  -  {1}:{2:00}",
            GameSettings.DifficultyName(), minutes, seconds);
    }

    void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main");
    }

    void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}
