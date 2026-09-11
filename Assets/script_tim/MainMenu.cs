using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        startButton.onClick.RemoveAllListeners();
        startButton.onClick.AddListener(StartGame);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitGame);
    }

    void StartGame()
    {
        Time.timeScale = 1f;   // sécurité si une partie précédente était figée
        SceneManager.LoadScene("Main");
    }

    void QuitGame()
    {
        Debug.Log("Quitter le jeu");
        Application.Quit();
    }
}