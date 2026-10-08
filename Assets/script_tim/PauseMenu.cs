using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;

public class PauseMenu : MonoBehaviour
{
    public KeyCode pauseKey = KeyCode.Escape;
    public KeyCode altPauseKey = KeyCode.P;

    private GameObject panel;
    private GameObject pauseButton;
    private bool paused = false;

    void Start()
    {
        BuildUI();
        if (panel != null) panel.SetActive(false);
    }

    void Update()
    {
        if (panel == null || IntroCrawl.IsPlaying) return;

        // Echap ferme d'abord la carte, il ne doit pas ouvrir la pause en meme temps
        if (ShipMap.IsOpen) return;

        // Le bouton pause disparait quand la partie est terminee
        if (pauseButton != null && !paused)
        {
            bool canPause = (GameManager.Instance == null) || GameManager.Instance.timerRunning;
            if (pauseButton.activeSelf != canPause) pauseButton.SetActive(canPause);
        }

        if (Input.GetKeyDown(pauseKey) || Input.GetKeyDown(altPauseKey))
        {
            if (paused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        if (GameManager.Instance != null && !GameManager.Instance.timerRunning) return;

        paused = true;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();
        if (pauseButton != null) pauseButton.SetActive(false);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        paused = false;
        panel.SetActive(false);
        if (pauseButton != null) pauseButton.SetActive(true);
        Time.timeScale = 1f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        ClearGameManager();
        SceneManager.LoadScene("Main");
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        ClearGameManager();
        SceneManager.LoadScene("MainMenu");
    }

    void ClearGameManager()
    {
        if (GameManager.Instance == null) return;

        GameObject go = GameManager.Instance.gameObject;
        GameManager.Instance = null;
        Destroy(go);
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }

    void BuildUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("PauseMenu : aucun Canvas trouve dans la scene");
            return;
        }

        panel = NewUIObject("PausePanel", canvas.transform);
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.05f, 0.09f, 0.85f);

        CreateLabel("PAUSED", 80, new Vector2(0f, 170f), new Vector2(600f, 120f));
        CreateButton("RESUME", new Vector2(0f, 40f), Resume);
        CreateButton("RESTART", new Vector2(0f, -40f), Restart);
        CreateButton("QUIT TO MENU", new Vector2(0f, -120f), QuitToMenu);

        AudioOptions audio = panel.AddComponent<AudioOptions>();
        audio.customParent = rt;
        audio.position = new Vector2(0f, -240f);
        audio.width = 420f;

        BuildPauseButton(canvas);
    }

    // Bouton pause visible en permanence, en haut a droite
    void BuildPauseButton(Canvas canvas)
    {
        pauseButton = NewUIObject("PauseButton", canvas.transform);

        RectTransform rt = pauseButton.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-28f, -24f);
        rt.sizeDelta = new Vector2(64f, 64f);

        Image img = pauseButton.AddComponent<Image>();
        img.color = new Color(0.08f, 0.12f, 0.20f, 0.85f);

        Button btn = pauseButton.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(Pause);
        SoundManager.AttachClick(btn);

        GameObject textGo = NewUIObject("Label", pauseButton.transform);
        RectTransform trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        TextMeshProUGUI txt = textGo.AddComponent<TextMeshProUGUI>();
        txt.text = "II";
        txt.fontSize = 30f;
        txt.fontStyle = FontStyles.Bold;
        txt.color = new Color(0.85f, 0.92f, 1f);
        txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;
    }

    GameObject NewUIObject(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    void CreateLabel(string content, float size, Vector2 pos, Vector2 box)
    {
        GameObject go = NewUIObject(content, panel.transform);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;

        TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = content;
        txt.fontSize = size;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
    }

    void CreateButton(string label, Vector2 pos, UnityAction action)
    {
        GameObject go = NewUIObject(label, panel.transform);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(360f, 64f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.92f, 0.94f, 0.97f, 1f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(action);
        SoundManager.AttachClick(btn);

        GameObject textGo = NewUIObject("Label", go.transform);
        RectTransform trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        TextMeshProUGUI txt = textGo.AddComponent<TextMeshProUGUI>();
        txt.text = label;
        txt.fontSize = 28f;
        txt.color = new Color(0.08f, 0.1f, 0.14f);
        txt.alignment = TextAlignmentOptions.Center;
    }
}
