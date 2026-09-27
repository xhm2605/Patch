using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroCrawl : MonoBehaviour
{
    public static bool IsPlaying = false;

    public string title = "PATCH";

    [TextArea(8, 25)]
    public string introText =
        "Episode I\n" +
        "THE DRIFTING STATION\n\n" +
        "Far beyond the outer rim, the survey ship PATCH " +
        "has lost all contact with fleet command.\n\n" +
        "A power surge has crippled four critical systems. " +
        "The emergency lockdown scrambled the master override " +
        "code into four fragments, each one sealed behind an " +
        "old arcade terminal left behind by the previous crew.\n\n" +
        "The reactor will not hold much longer.\n\n" +
        "You are the only crew member still awake. Repair every " +
        "system, recover the fragments, and reassemble the code " +
        "before the countdown reaches zero...";

    [Tooltip("Vitesse de defilement en pixels par seconde. Plus petit = plus lent.")]
    public float readingSpeed = 55f;

    [Tooltip("Marge sous le bas de l'ecran au depart. Plus petit = le texte arrive plus vite.")]
    public float startMargin = 30f;

    public float crawlHeight = 2600f;
    public float fontSize = 46f;
    public float endScale = 0.55f;

    private GameObject panel;
    private RectTransform canvasRect;
    private RectTransform crawl;
    private TextMeshProUGUI crawlText;
    private float startY;
    private float screenHeight = 1080f;
    private float textHeight = 0f;
    private float fadeSpan = 2000f;
    private bool running = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.introShown) return;
            GameManager.Instance.introShown = true;
        }

        Build();
    }

    void Update()
    {
        if (!running) return;

        if (textHeight <= 0f) MeasureText();

        float y = crawl.anchoredPosition.y + readingSpeed * Time.unscaledDeltaTime;
        crawl.anchoredPosition = new Vector2(0f, y);

        float t = Mathf.Clamp01((y - startY) / fadeSpan);
        float s = Mathf.Lerp(1f, endScale, t);
        crawl.localScale = new Vector3(s, s, 1f);

        // Fin des que la derniere ligne est sortie par le haut
        if (y - textHeight * s > 40f) Finish();
    }

    void MeasureText()
    {
        // Le Canvas Scaler n'est calibre qu'apres Start : on mesure ici
        Canvas.ForceUpdateCanvases();

        if (canvasRect != null && canvasRect.rect.height > 100f)
            screenHeight = canvasRect.rect.height;

        crawlText.ForceMeshUpdate();
        textHeight = crawlText.preferredHeight;

        if (textHeight < 500f) textHeight = 1800f;

        fadeSpan = textHeight + screenHeight;

        startY = -(screenHeight + startMargin);
        crawl.anchoredPosition = new Vector2(0f, startY);
    }

    public void Finish()
    {
        running = false;
        IsPlaying = false;
        Time.timeScale = 1f;

        if (panel != null) Destroy(panel);
        if (GameManager.Instance != null) GameManager.Instance.StartTimer();
    }

    void Build()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        canvasRect = canvas.transform as RectTransform;
        if (canvasRect != null && canvasRect.rect.height > 100f)
            screenHeight = canvasRect.rect.height;

        panel = NewUI("IntroPanel", canvas.transform);
        Stretch(panel.GetComponent<RectTransform>());
        Image bg = panel.AddComponent<Image>();
        bg.color = Color.black;

        BuildCrawl();
        BuildFade();
        BuildSkipButton();

        panel.transform.SetAsLastSibling();

        IsPlaying = true;
        running = true;
        Time.timeScale = 0f;
    }

    GameObject NewUI(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void BuildCrawl()
    {
        GameObject go = NewUI("Crawl", panel.transform);

        crawl = go.GetComponent<RectTransform>();
        crawl.anchorMin = new Vector2(0.5f, 1f);
        crawl.anchorMax = new Vector2(0.5f, 1f);
        crawl.pivot = new Vector2(0.5f, 1f);
        crawl.sizeDelta = new Vector2(1000f, crawlHeight);

        // Le haut du texte se place juste sous le bas de l'ecran reel
        startY = -(screenHeight + startMargin);
        crawl.anchoredPosition = new Vector2(0f, startY);

        crawlText = go.AddComponent<TextMeshProUGUI>();
        crawlText.text = "<size=170%><b>" + title + "</b></size>\n\n" + introText;
        crawlText.fontSize = fontSize;
        crawlText.color = new Color(1f, 0.84f, 0.29f);
        crawlText.alignment = TextAlignmentOptions.Top;
        crawlText.lineSpacing = 14f;
        crawlText.raycastTarget = false;
    }

    void BuildFade()
    {
        GameObject go = NewUI("TopFade", panel.transform);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.62f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        int h = 128;
        Texture2D tex = new Texture2D(1, h);
        tex.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < h; y++)
        {
            float a = Mathf.Pow((float)y / (h - 1), 1.4f);
            tex.SetPixel(0, y, new Color(0f, 0f, 0f, a));
        }

        tex.Apply();

        Image img = go.AddComponent<Image>();
        img.sprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, h), new Vector2(0.5f, 0.5f));
        img.raycastTarget = false;
    }

    void BuildSkipButton()
    {
        GameObject go = NewUI("SkipButton", panel.transform);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-40f, -30f);
        rt.sizeDelta = new Vector2(160f, 52f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.18f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(Finish);

        GameObject textGo = NewUI("Label", go.transform);
        Stretch(textGo.GetComponent<RectTransform>());

        TextMeshProUGUI txt = textGo.AddComponent<TextMeshProUGUI>();
        txt.text = "SKIP >>";
        txt.fontSize = 22f;
        txt.color = Color.white;
        txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;
    }
}
