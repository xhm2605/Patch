using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuStyler : MonoBehaviour
{
    public static MenuStyler Instance;

    private static Sprite solidSprite;
    private static Sprite ghostSprite;

    private Image titleRule;

    private static readonly Color Cyan = new Color(0.42f, 0.78f, 1f);
    private static readonly Color Ink = new Color(0.85f, 0.94f, 1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("MenuStyler");
        go.AddComponent<MenuStyler>();
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
        if (SceneManager.GetActiveScene().name == "MainMenu") StartCoroutine(StyleNextFrame());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        titleRule = null;
        if (scene.name == "MainMenu") StartCoroutine(StyleNextFrame());
    }

    IEnumerator StyleNextFrame()
    {
        yield return null;
        Style();
    }

    void Update()
    {
        if (titleRule == null) return;

        float a = 0.35f + 0.25f * Mathf.Sin(Time.unscaledTime * 1.6f);
        Color c = titleRule.color;
        titleRule.color = new Color(c.r, c.g, c.b, a);
    }

    void Style()
    {
        EnsureSprites();

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        RectTransform title = Find(canvas, "GameTitle");
        RectTransform start = Find(canvas, "StartBtn");
        RectTransform diff = Find(canvas, "DifficultyBtn");
        RectTransform quit = Find(canvas, "QuitBtn");
        RectTransform audio = Find(canvas, "AudioOptions");

        StyleTitle(title, canvas);

        StyleButton(start, new Vector2(0f, 24f), new Vector2(380f, 70f), 30f, true);
        StyleButton(diff, new Vector2(0f, -62f), new Vector2(470f, 54f), 23f, false);
        StyleButton(quit, new Vector2(0f, -144f), new Vector2(380f, 60f), 26f, false);

        if (audio != null) audio.anchoredPosition = new Vector2(0f, -250f);
    }

    RectTransform Find(Canvas canvas, string objectName)
    {
        Transform t = canvas.transform.Find(objectName);
        if (t != null) return t as RectTransform;

        foreach (RectTransform rt in canvas.GetComponentsInChildren<RectTransform>(true))
            if (rt.name == objectName) return rt;

        return null;
    }

    void StyleTitle(RectTransform title, Canvas canvas)
    {
        if (title == null) return;

        title.anchorMin = new Vector2(0.5f, 0.5f);
        title.anchorMax = new Vector2(0.5f, 0.5f);
        title.pivot = new Vector2(0.5f, 0.5f);
        title.anchoredPosition = new Vector2(0f, 232f);
        title.sizeDelta = new Vector2(1000f, 170f);

        TMP_Text txt = title.GetComponent<TMP_Text>();
        if (txt == null) txt = title.GetComponentInChildren<TMP_Text>();

        if (txt != null)
        {
            txt.fontSize = 118f;
            txt.characterSpacing = 22f;
            txt.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
        }

        if (canvas.transform.Find("TitleRule") == null)
        {
            GameObject rule = NewUI("TitleRule", canvas.transform);
            RectTransform rrt = rule.GetComponent<RectTransform>();
            rrt.anchorMin = new Vector2(0.5f, 0.5f);
            rrt.anchorMax = new Vector2(0.5f, 0.5f);
            rrt.pivot = new Vector2(0.5f, 0.5f);
            rrt.anchoredPosition = new Vector2(0f, 152f);
            rrt.sizeDelta = new Vector2(440f, 2f);

            titleRule = rule.AddComponent<Image>();
            titleRule.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.5f);
            titleRule.raycastTarget = false;
        }

        if (canvas.transform.Find("TitleSub") == null)
        {
            GameObject sub = NewUI("TitleSub", canvas.transform);
            RectTransform srt = sub.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.5f, 0.5f);
            srt.anchorMax = new Vector2(0.5f, 0.5f);
            srt.pivot = new Vector2(0.5f, 0.5f);
            srt.anchoredPosition = new Vector2(0f, 116f);
            srt.sizeDelta = new Vector2(900f, 40f);

            TextMeshProUGUI stx = sub.AddComponent<TextMeshProUGUI>();
            stx.text = "SURVEY SHIP  ·  FOUR SYSTEMS OFFLINE";
            stx.fontSize = 21f;
            stx.characterSpacing = 11f;
            stx.color = new Color(Cyan.r, Cyan.g, Cyan.b, 0.75f);
            stx.alignment = TextAlignmentOptions.Center;
            stx.raycastTarget = false;
        }
    }

    void StyleButton(RectTransform rt, Vector2 pos, Vector2 size, float fontSize, bool solid)
    {
        if (rt == null) return;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = rt.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = solid ? solidSprite : ghostSprite;
            img.type = Image.Type.Sliced;
            img.color = Color.white;
            img.pixelsPerUnitMultiplier = 1f;
        }

        Button btn = rt.GetComponent<Button>();
        if (btn != null)
        {
            btn.transition = Selectable.Transition.ColorTint;
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(0.80f, 0.86f, 0.93f);
            cb.highlightedColor = Color.white;
            cb.pressedColor = new Color(0.52f, 0.70f, 0.84f);
            cb.selectedColor = new Color(0.86f, 0.92f, 0.98f);
            cb.disabledColor = new Color(0.4f, 0.44f, 0.5f, 0.6f);
            cb.fadeDuration = 0.12f;
            btn.colors = cb;
        }

        TMP_Text txt = rt.GetComponentInChildren<TMP_Text>();
        if (txt != null)
        {
            txt.fontSize = fontSize;
            txt.characterSpacing = 7f;
            txt.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
            txt.color = solid ? new Color(0.06f, 0.10f, 0.16f) : Ink;
            txt.alignment = TextAlignmentOptions.Center;
            txt.raycastTarget = false;
        }
    }

    GameObject NewUI(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    // ---------- Sprites de bouton ----------

    static void EnsureSprites()
    {
        if (solidSprite == null)
            solidSprite = RoundedRect(96, 96, 16f, 2.5f,
                new Color(0.62f, 0.86f, 1f, 0.94f),
                new Color(0.80f, 0.95f, 1f, 1f));

        if (ghostSprite == null)
            ghostSprite = RoundedRect(96, 96, 16f, 2f,
                new Color(0.05f, 0.11f, 0.20f, 0.72f),
                new Color(0.34f, 0.70f, 0.95f, 0.95f));
    }

    public static Sprite RoundedRect(int w, int h, float radius, float border, Color fill, Color edge)
    {
        Color[] px = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float d = RoundedDistance(x + 0.5f, y + 0.5f, w, h, radius);

                Color c;

                if (d > 0f)
                {
                    c = new Color(edge.r, edge.g, edge.b, 0f);
                }
                else if (d > -border)
                {
                    c = edge;
                }
                else
                {
                    float t = Mathf.Clamp01((-d - border) / 2.5f);
                    c = Color.Lerp(edge, fill, t);
                }

                float aa = Mathf.Clamp01(-d + 0.5f);
                px[y * w + x] = new Color(c.r, c.g, c.b, c.a * aa);
            }
        }

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        float b = radius + 4f;
        return Sprite.Create(tex, new Rect(0f, 0f, w, h), new Vector2(0.5f, 0.5f),
            100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
    }

    static float RoundedDistance(float x, float y, float w, float h, float r)
    {
        float qx = Mathf.Abs(x - w * 0.5f) - (w * 0.5f - r);
        float qy = Mathf.Abs(y - h * 0.5f) - (h * 0.5f - r);

        float ax = Mathf.Max(qx, 0f);
        float ay = Mathf.Max(qy, 0f);

        return Mathf.Sqrt(ax * ax + ay * ay) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
    }
}
