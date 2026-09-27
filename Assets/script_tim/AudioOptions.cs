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

    private TMP_Text soundLabel;
    private TMP_Text volumeLabel;
    private Slider slider;

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

        GameObject root = NewUI("AudioOptions", host);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(width, 150f);

        BuildSoundButton(root.transform);
        BuildVolumeSlider(root.transform);

        Refresh();
    }

    void BuildSoundButton(Transform parent)
    {
        GameObject go = NewUI("SoundButton", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 42f);
        rt.sizeDelta = new Vector2(width, 54f);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.92f, 0.94f, 0.97f, 1f);

        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(OnToggleSound);

        GameObject textGo = NewUI("Label", go.transform);
        Stretch(textGo.GetComponent<RectTransform>());

        soundLabel = textGo.AddComponent<TextMeshProUGUI>();
        soundLabel.fontSize = 26f;
        soundLabel.color = new Color(0.08f, 0.1f, 0.14f);
        soundLabel.alignment = TextAlignmentOptions.Center;
        soundLabel.raycastTarget = false;
    }

    void BuildVolumeSlider(Transform parent)
    {
        GameObject labelGo = NewUI("VolumeLabel", parent);
        RectTransform lrt = labelGo.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0.5f, 0.5f);
        lrt.anchorMax = new Vector2(0.5f, 0.5f);
        lrt.pivot = new Vector2(0.5f, 0.5f);
        lrt.anchoredPosition = new Vector2(0f, 2f);
        lrt.sizeDelta = new Vector2(width, 34f);

        volumeLabel = labelGo.AddComponent<TextMeshProUGUI>();
        volumeLabel.fontSize = 22f;
        volumeLabel.color = Color.white;
        volumeLabel.alignment = TextAlignmentOptions.Center;
        volumeLabel.raycastTarget = false;

        GameObject go = NewUI("VolumeSlider", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, -50f);
        rt.sizeDelta = new Vector2(width, 26f);

        GameObject bgGo = NewUI("Background", go.transform);
        Stretch(bgGo.GetComponent<RectTransform>());
        Image bg = bgGo.AddComponent<Image>();
        bg.color = new Color(0.22f, 0.25f, 0.32f, 1f);

        GameObject fillArea = NewUI("Fill Area", go.transform);
        RectTransform fart = fillArea.GetComponent<RectTransform>();
        fart.anchorMin = new Vector2(0f, 0f);
        fart.anchorMax = new Vector2(1f, 1f);
        fart.offsetMin = new Vector2(0f, 0f);
        fart.offsetMax = new Vector2(0f, 0f);

        GameObject fillGo = NewUI("Fill", fillArea.transform);
        RectTransform frt = fillGo.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;
        Image fill = fillGo.AddComponent<Image>();
        fill.color = new Color(0.36f, 0.78f, 0.92f, 1f);

        GameObject handleArea = NewUI("Handle Slide Area", go.transform);
        RectTransform hart = handleArea.GetComponent<RectTransform>();
        hart.anchorMin = new Vector2(0f, 0f);
        hart.anchorMax = new Vector2(1f, 1f);
        hart.offsetMin = new Vector2(10f, 0f);
        hart.offsetMax = new Vector2(-10f, 0f);

        GameObject handleGo = NewUI("Handle", handleArea.transform);
        RectTransform hrt = handleGo.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(22f, 34f);
        Image handle = handleGo.AddComponent<Image>();
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
        if (soundLabel != null)
            soundLabel.text = GameSettings.soundOn ? "SOUND : ON" : "SOUND : OFF";

        if (volumeLabel != null)
            volumeLabel.text = "VOLUME  " + GameSettings.VolumePercent() + "%";
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
}
