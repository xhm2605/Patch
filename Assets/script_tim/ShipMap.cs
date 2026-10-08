using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Plan du vaisseau : les salles, ou se trouve le joueur, et ce qu'il reste a reparer.
public class ShipMap : MonoBehaviour
{
    public static ShipMap Instance;

    public static bool IsOpen { get; private set; }

    private const float Scale = 10.4f;

    private static Sprite panelSprite;
    private static Sprite dotSprite;

    private GameObject openButton;
    private GameObject panel;
    private RectTransform plan;

    private RectTransform playerDot;
    private readonly List<RectTransform> marks = new List<RectTransform>();
    private readonly List<Image> markIcons = new List<Image>();
    private readonly List<TMP_Text> markLabels = new List<TMP_Text>();
    private readonly List<ArcadeTerminal> terminals = new List<ArcadeTerminal>();

    private Transform player;

    private static readonly Color RoomFill = new Color(0.12f, 0.22f, 0.34f, 0.95f);
    private static readonly Color HallFill = new Color(0.08f, 0.14f, 0.22f, 0.95f);
    private static readonly Color Cyan = new Color(0.42f, 0.82f, 1f);
    private static readonly Color Alert = new Color(1f, 0.30f, 0.26f);
    private static readonly Color Done = new Color(0.40f, 0.92f, 0.56f);

    private static readonly Dictionary<string, Vector2> RoomLabels = new Dictionary<string, Vector2>
    {
        { "CONTROL", new Vector2(0f, 0f) },
        { "ARCADE BAY", new Vector2(0f, 22f) },
        { "ENGINE", new Vector2(0f, -22f) },
        { "SHIELD", new Vector2(-26f, 0f) },
        { "OXYGEN", new Vector2(26f, 0f) },
        { "COOLANT", new Vector2(26f, 22f) },
        { "CARGO BAY", new Vector2(-27f, 21f) },
        { "QUARTERS", new Vector2(-27f, -21f) },
        { "OBSERVATION", new Vector2(27f, -21f) }
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("ShipMap");
        go.AddComponent<ShipMap>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // On laisse passer une image : la borne Coolant est creee dans un autre Start
    System.Collections.IEnumerator Start()
    {
        yield return null;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        IsOpen = false;

        openButton = null;
        panel = null;
        plan = null;
        playerDot = null;
        player = null;

        marks.Clear();
        markIcons.Clear();
        markLabels.Clear();
        terminals.Clear();

        if (scene.name == "Main") Build();
    }

    // ---------- Construction ----------

    void Build()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        EnsureSprites();
        BuildOpenButton(canvas);
        BuildPanel(canvas);
    }

    void BuildOpenButton(Canvas canvas)
    {
        openButton = NewUI("MapButton", canvas.transform);

        RectTransform rt = openButton.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-28f, 28f);
        rt.sizeDelta = new Vector2(150f, 64f);

        Image img = openButton.AddComponent<Image>();
        img.sprite = panelSprite;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.08f, 0.17f, 0.28f, 0.90f);

        Button btn = openButton.AddComponent<Button>();
        btn.targetGraphic = img;
        Tint(btn);
        btn.onClick.AddListener(Open);

        Label(openButton.transform, "MAP", 26f, Cyan);
    }

    void BuildPanel(Canvas canvas)
    {
        panel = NewUI("MapPanel", canvas.transform);
        Stretch(panel.GetComponent<RectTransform>());

        Image bg = panel.AddComponent<Image>();
        bg.color = new Color(0.02f, 0.04f, 0.08f, 0.95f);

        GameObject title = NewUI("MapTitle", panel.transform);
        Place(title.GetComponent<RectTransform>(), 0f, 418f, 800f, 48f);

        TextMeshProUGUI t = title.AddComponent<TextMeshProUGUI>();
        t.text = "SHIP LAYOUT";
        t.fontSize = 38f;
        t.characterSpacing = 14f;
        t.fontStyle = FontStyles.Bold;
        t.color = Cyan;
        t.alignment = TextAlignmentOptions.Center;
        t.raycastTarget = false;

        GameObject planGo = NewUI("Plan", panel.transform);
        plan = planGo.GetComponent<RectTransform>();
        Place(plan, 0f, -16f, 820f, 760f);

        DrawZones();
        DrawMarks();
        BuildCloseButton();

        panel.SetActive(false);
    }

    void DrawZones()
    {
        foreach (Rect r in ShipDressing.CoreZones) Zone(r);
        foreach (Rect r in ShipExpansion.Zones) Zone(r);

        foreach (KeyValuePair<string, Vector2> entry in RoomLabels)
        {
            GameObject go = NewUI("Name_" + entry.Key, plan);
            Place(go.GetComponent<RectTransform>(),
                entry.Value.x * Scale, entry.Value.y * Scale, 220f, 34f);

            TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
            txt.text = entry.Key;
            txt.fontSize = 21f;
            txt.characterSpacing = 4f;
            txt.color = new Color(0.68f, 0.82f, 0.95f, 0.85f);
            txt.alignment = TextAlignmentOptions.Center;
            txt.raycastTarget = false;
        }
    }

    void Zone(Rect r)
    {
        bool isRoom = r.width > 6f && r.height > 6f;

        GameObject go = NewUI("Zone", plan);
        Place(go.GetComponent<RectTransform>(),
            r.center.x * Scale, r.center.y * Scale,
            r.width * Scale, r.height * Scale);

        Image img = go.AddComponent<Image>();
        img.sprite = panelSprite;
        img.type = Image.Type.Sliced;
        img.color = isRoom ? RoomFill : HallFill;
        img.raycastTarget = false;
    }

    void DrawMarks()
    {
        foreach (ArcadeTerminal t in FindObjectsByType<ArcadeTerminal>(FindObjectsInactive.Include))
        {
            terminals.Add(t);

            GameObject go = NewUI("Mark_" + t.terminalId, plan);
            RectTransform rt = go.GetComponent<RectTransform>();
            Place(rt, t.transform.position.x * Scale, t.transform.position.y * Scale, 32f, 32f);

            Image img = go.AddComponent<Image>();
            img.sprite = dotSprite;
            img.raycastTarget = false;

            GameObject nameGo = NewUI("MarkName", plan);
            Place(nameGo.GetComponent<RectTransform>(),
                t.transform.position.x * Scale, t.transform.position.y * Scale - 22f, 150f, 22f);

            TextMeshProUGUI txt = nameGo.AddComponent<TextMeshProUGUI>();
            txt.text = t.terminalId.ToUpper();
            txt.fontSize = 20f;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.raycastTarget = false;

            marks.Add(rt);
            markIcons.Add(img);
            markLabels.Add(txt);
        }

        CodeTerminal console = FindAnyObjectByType<CodeTerminal>();
        if (console != null)
        {
            GameObject cgo = NewUI("Mark_Console", plan);
            Place(cgo.GetComponent<RectTransform>(),
                console.transform.position.x * Scale, console.transform.position.y * Scale, 26f, 26f);

            Image cimg = cgo.AddComponent<Image>();
            cimg.sprite = dotSprite;
            cimg.color = Done;
            cimg.raycastTarget = false;

            GameObject cname = NewUI("MarkName", plan);
            Place(cname.GetComponent<RectTransform>(),
                console.transform.position.x * Scale,
                console.transform.position.y * Scale - 22f, 150f, 22f);

            TextMeshProUGUI ctxt = cname.AddComponent<TextMeshProUGUI>();
            ctxt.text = "CONSOLE";
            ctxt.fontSize = 20f;
            ctxt.fontStyle = FontStyles.Bold;
            ctxt.color = new Color(Done.r, Done.g, Done.b, 0.9f);
            ctxt.alignment = TextAlignmentOptions.Center;
            ctxt.raycastTarget = false;
        }

        GameObject dot = NewUI("PlayerDot", plan);
        playerDot = dot.GetComponent<RectTransform>();
        Place(playerDot, 0f, 0f, 30f, 30f);

        Image pimg = dot.AddComponent<Image>();
        pimg.sprite = dotSprite;
        pimg.color = Cyan;
        pimg.raycastTarget = false;

        // Le joueur se dessine par-dessus les autres reperes
        dot.transform.SetAsLastSibling();
    }

    void BuildCloseButton()
    {
        GameObject go = NewUI("MapClose", panel.transform);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-40f, -32f);
        rt.sizeDelta = new Vector2(168f, 58f);

        Image img = go.AddComponent<Image>();
        img.sprite = panelSprite;
        img.type = Image.Type.Sliced;
        img.color = new Color(0.42f, 0.14f, 0.16f, 0.95f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        Tint(btn);
        btn.onClick.AddListener(Close);

        Label(go.transform, "CLOSE", 24f, Color.white);
    }

    // ---------- Ouverture ----------

    public void Open()
    {
        if (panel == null) return;

        SoundManager.PlayOpen();
        IsOpen = true;
        panel.SetActive(true);
        panel.transform.SetAsLastSibling();

        if (openButton != null) openButton.SetActive(false);
    }

    public void Close()
    {
        if (panel == null) return;

        SoundManager.PlayClick();
        IsOpen = false;
        panel.SetActive(false);

        if (openButton != null) openButton.SetActive(true);
    }

    void Update()
    {
        if (openButton != null)
        {
            bool playable = GameManager.Instance == null || GameManager.Instance.timerRunning;
            bool show = playable && !IntroCrawl.IsPlaying && (panel == null || !panel.activeSelf);

            if (openButton.activeSelf != show) openButton.SetActive(show);
        }

        if (panel == null || !panel.activeSelf) return;

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.M))
        {
            Close();
            return;
        }

        Refresh();
    }

    void Refresh()
    {
        if (player == null)
        {
            // Le tag Player est aussi porte par la console : on vise le composant
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
        }

        if (player != null && playerDot != null)
        {
            playerDot.anchoredPosition = new Vector2(
                player.position.x * Scale, player.position.y * Scale);

            float beat = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 5f);
            playerDot.localScale = Vector3.one * (0.9f + beat * 0.35f);
        }

        float pulse = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * 4.5f);

        for (int i = 0; i < terminals.Count; i++)
        {
            if (terminals[i] == null) continue;

            bool repaired = GameManager.Instance != null
                            && GameManager.Instance.IsRepaired(terminals[i].terminalId);

            marks[i].anchoredPosition = new Vector2(
                terminals[i].transform.position.x * Scale,
                terminals[i].transform.position.y * Scale);

            if (repaired)
            {
                markIcons[i].color = new Color(Done.r, Done.g, Done.b, 0.75f);
                marks[i].sizeDelta = new Vector2(26f, 26f);
                markLabels[i].color = new Color(Done.r, Done.g, Done.b, 0.6f);
            }
            else
            {
                markIcons[i].color = new Color(Alert.r, Alert.g, Alert.b, 0.55f + pulse * 0.45f);
                marks[i].sizeDelta = new Vector2(38f + pulse * 10f, 38f + pulse * 10f);
                markLabels[i].color = new Color(1f, 0.58f, 0.52f, 1f);
            }
        }
    }

    // ---------- Outils ----------

    void Tint(Button btn)
    {
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.88f, 0.93f, 1f);
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.55f, 0.70f, 0.85f);
        cb.fadeDuration = 0.1f;
        btn.colors = cb;
    }

    TMP_Text Label(Transform parent, string content, float size, Color c)
    {
        GameObject go = NewUI("Label", parent);
        Stretch(go.GetComponent<RectTransform>());

        TextMeshProUGUI txt = go.AddComponent<TextMeshProUGUI>();
        txt.text = content;
        txt.fontSize = size;
        txt.characterSpacing = 6f;
        txt.fontStyle = FontStyles.Bold;
        txt.color = c;
        txt.alignment = TextAlignmentOptions.Center;
        txt.raycastTarget = false;
        return txt;
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

    void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    static void EnsureSprites()
    {
        if (panelSprite == null)
            panelSprite = MenuStyler.RoundedRect(64, 64, 8f, 1.5f,
                Color.white, new Color(0.38f, 0.72f, 0.95f, 0.75f));

        if (dotSprite == null) dotSprite = BuildDot(64);
    }

    static Sprite BuildDot(int s)
    {
        Color[] px = new Color[s * s];
        float c = s * 0.5f;
        float r = s * 0.42f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float a = Mathf.Clamp01(r - d);

                // Anneau clair pour detacher le point du fond
                if (Mathf.Abs(d - r * 0.72f) < 2f) a = Mathf.Min(a, 0.45f);

                px[y * s + x] = new Color(1f, 1f, 1f, a);
            }
        }

        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 64f);
    }
}
