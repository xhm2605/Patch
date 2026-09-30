using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioOptions : MonoBehaviour
{
    [Tooltip("Position du bloc son, par rapport au centre de l'ecran.")]
    public Vector2 position = new Vector2(0f, -190f);

    public float width = 500f;

    [Tooltip("Laisser vide pour se placer directement dans le Canvas.")]
    public RectTransform customParent;

    private static Sprite panelSprite;
    private static Sprite knobSprite;
    private static Sprite speakerOnSprite;
    private static Sprite speakerOffSprite;

    private Image speakerIcon;
    private Image fill;
    private TMP_Text percent;
    private Slider slider;

    private static readonly Color Cyan = new Color(0.40f, 0.80f, 1f);
    private static readonly Color Muted = new Color(0.45f, 0.50f, 0.58f);

    void Start()
    {
        GameSettings.ApplyAudio();
        Build();
    }

    void Build()
    {
        Transform host = customParent;

        if (host == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null) return;
            host = canvas.transform;
        }

        EnsureSprites();

        GameObject root = NewUI("AudioOptions", host);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(width, 58f);

        Image bg = root.AddComponent<Image>();
        bg.sprite = panelSprite;
        bg.type = Image.Type.Sliced;
        bg.color = new Color(0.05f, 0.11f, 0.20f, 0.72f);

        BuildSpeaker(root.transform);
        BuildSlider(root.transform);
        BuildPercent(root.transform);

        Refresh();
    }

    void BuildSpeaker(Transform parent)
    {
        GameObject go = NewUI("SoundToggle", parent);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(34f, 0f);
        rt.sizeDelta = new Vector2(38f, 38f);

        speakerIcon = go.AddComponent<Image>();
        speakerIcon.sprite = speakerOnSprite;
        speakerIcon.color = Cyan;

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = speakerIcon;
        btn.transition = Selectable.Transition.ColorTint;

        ColorBlock cb = btn.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.6f, 0.7f, 0.8f);
        cb.fadeDuration = 0.1f;
        btn.colors = cb;

        btn.onClick.AddListener(OnToggleSound);
    }

    void BuildSlider(Transform parent)
    {
        float sliderWidth = Mathf.Max(120f, width - 150f);
        float centerX = (68f + (width - 82f)) * 0.5f - width * 0.5f;

        GameObject go = NewUI("VolumeSlider", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(centerX, 0f);
        rt.sizeDelta = new Vector2(sliderWidth, 24f);

        GameObject bgGo = NewUI("Background", go.transform);
        RectTransform brt = bgGo.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 0.5f);
        brt.anchorMax = new Vector2(1f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.offsetMin = new Vector2(0f, -3f);
        brt.offsetMax = new Vector2(0f, 3f);

        Image bg = bgGo.AddComponent<Image>();
        bg.color = new Color(0.16f, 0.22f, 0.32f, 0.95f);

        GameObject fillArea = NewUI("Fill Area", go.transform);
        RectTransform fart = fillArea.GetComponent<RectTransform>();
        fart.anchorMin = new Vector2(0f, 0.5f);
        fart.anchorMax = new Vector2(1f, 0.5f);
        fart.pivot = new Vector2(0.5f, 0.5f);
        fart.offsetMin = new Vector2(0f, -3f);
        fart.offsetMax = new Vector2(0f, 3f);

        GameObject fillGo = NewUI("Fill", fillArea.transform);
        RectTransform frt = fillGo.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;

        fill = fillGo.AddComponent<Image>();
        fill.color = Cyan;

        GameObject handleArea = NewUI("Handle Slide Area", go.transform);
        RectTransform hart = handleArea.GetComponent<RectTransform>();
        hart.anchorMin = new Vector2(0f, 0f);
        hart.anchorMax = new Vector2(1f, 1f);
        hart.offsetMin = new Vector2(9f, 0f);
        hart.offsetMax = new Vector2(-9f, 0f);

        GameObject handleGo = NewUI("Handle", handleArea.transform);
        RectTransform hrt = handleGo.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(18f, 18f);

        Image handle = handleGo.AddComponent<Image>();
        handle.sprite = knobSprite;
        handle.color = Color.white;

        slider = go.AddComponent<Slider>();
        slider.fillRect = frt;
        slider.handleRect = hrt;
        slider.targetGraphic = handle;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = GameSettings.volume;
        slider.onValueChanged.AddListener(OnVolumeChanged);
    }

    void BuildPercent(Transform parent)
    {
        GameObject go = NewUI("VolumePercent", parent);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 0.5f);
        rt.anchorMax = new Vector2(1f, 0.5f);
        rt.pivot = new Vector2(1f, 0.5f);
        rt.anchoredPosition = new Vector2(-16f, 0f);
        rt.sizeDelta = new Vector2(64f, 34f);

        percent = go.AddComponent<TextMeshProUGUI>();
        percent.fontSize = 20f;
        percent.characterSpacing = 3f;
        percent.color = new Color(0.78f, 0.88f, 1f);
        percent.alignment = TextAlignmentOptions.Right;
        percent.raycastTarget = false;
    }

    void OnToggleSound()
    {
        GameSettings.ToggleSound();
        Refresh();
    }

    void OnVolumeChanged(float value)
    {
        GameSettings.SetVolume(value);
        Refresh();
    }

    void Refresh()
    {
        bool on = GameSettings.soundOn;

        if (speakerIcon != null)
        {
            speakerIcon.sprite = on ? speakerOnSprite : speakerOffSprite;
            speakerIcon.color = on ? Cyan : Muted;
        }

        if (fill != null) fill.color = on ? Cyan : Muted;

        if (percent != null)
        {
            percent.text = on ? GameSettings.VolumePercent() + "%" : "MUTED";
            percent.color = on ? new Color(0.78f, 0.88f, 1f) : Muted;
        }

        if (slider != null && Mathf.Abs(slider.value - GameSettings.volume) > 0.001f)
            slider.SetValueWithoutNotify(GameSettings.volume);
    }

    GameObject NewUI(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    // ---------- Sprites ----------

    static void EnsureSprites()
    {
        if (panelSprite == null)
            panelSprite = MenuStyler.RoundedRect(96, 96, 18f, 1.5f,
                Color.white,
                new Color(0.32f, 0.62f, 0.85f, 0.55f));

        if (knobSprite == null) knobSprite = BuildKnob(48);
        if (speakerOnSprite == null) speakerOnSprite = BuildSpeakerIcon(64, true);
        if (speakerOffSprite == null) speakerOffSprite = BuildSpeakerIcon(64, false);
    }

    static Sprite BuildKnob(int s)
    {
        Color[] px = new Color[s * s];
        float c = s * 0.5f;
        float r = s * 0.46f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float a = Mathf.Clamp01(r - d);
                float shade = Mathf.Lerp(0.80f, 1f, Mathf.Clamp01((y - c) / r * 0.5f + 0.5f));

                px[y * s + x] = new Color(shade, shade, shade, a);
            }
        }

        return Tex(px, s, s, s);
    }

    static Sprite BuildSpeakerIcon(int s, bool on)
    {
        Color[] px = new Color[s * s];
        float mid = s * 0.5f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float a = 0f;

                if (x >= 12 && x <= 24 && y >= 24 && y <= 40) a = 1f;

                if (x > 24 && x <= 40)
                {
                    float half = Mathf.Lerp(8f, 19f, (x - 24f) / 16f);
                    if (Mathf.Abs(y - mid) <= half) a = 1f;
                }

                if (on)
                {
                    float dx = x - 40f, dy = y - mid;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float ang = Mathf.Abs(Mathf.Atan2(dy, dx));

                    if (ang < 1.0f && dx > 0f)
                    {
                        if (Mathf.Abs(d - 9f) < 1.6f) a = 1f;
                        if (Mathf.Abs(d - 16f) < 1.6f) a = 1f;
                    }
                }
                else
                {
                    float u = x - 47f, v = y - mid;

                    if (Mathf.Abs(u) < 8f)
                    {
                        if (Mathf.Abs(u - v) < 1.9f) a = 1f;
                        if (Mathf.Abs(u + v) < 1.9f) a = 1f;
                    }
                }

                px[y * s + x] = new Color(1f, 1f, 1f, a);
            }
        }

        return Tex(px, s, s, s);
    }

    static Sprite Tex(Color[] px, int w, int h, float ppu)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0f, 0f, w, h), new Vector2(0.5f, 0.5f),
            ppu, 0, SpriteMeshType.FullRect);
    }
}
