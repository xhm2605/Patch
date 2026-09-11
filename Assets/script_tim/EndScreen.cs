using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class EndScreen : MonoBehaviour
{
    public static EndScreen Instance;

    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text reasonText;
    public Button menuButton;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);

        menuButton.onClick.RemoveAllListeners();
        menuButton.onClick.AddListener(BackToMenu);
    }

    public static void Show(bool win, string reason)
    {
        if (Instance == null) return;
        Instance.Display(win, reason);
    }

    void Display(bool win, string reason)
    {
        panel.SetActive(true);
        titleText.text = win ? "MISSION COMPLETE" : "GAME END";
        titleText.color = win ? Color.green : Color.red;
        reasonText.text = reason;

        Time.timeScale = 0f;   // fige le jeu
        Image bg = panel.GetComponent<Image>();
        if (bg != null)
        {
            bg.color = win 
                ? new Color(0f, 0.15f, 0f, 0.92f)   // vert très sombre
                : new Color(0.15f, 0f, 0f, 0.92f);  // rouge très sombre
        }
    }

    void BackToMenu()
    {
        Time.timeScale = 1f;
        Destroy(GameManager.Instance.gameObject);   // efface la partie en cours
        SceneManager.LoadScene("MainMenu");
    }
}