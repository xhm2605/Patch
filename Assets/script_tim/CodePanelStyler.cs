using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodePanelStyler : MonoBehaviour
{
    public string titleLabel = "MASTER OVERRIDE CODE";

    public Color panelColor = new Color(0.055f, 0.078f, 0.133f, 1f);
    public Color borderColor = new Color(0.18f, 0.55f, 0.68f, 1f);
    public Color accentColor = new Color(0.36f, 0.85f, 0.95f, 1f);
    public Color slotColor = new Color(1f, 0.87f, 0.42f, 1f);
    public Color fragColor = new Color(0.20f, 0.42f, 0.68f, 1f);
    public Color submitColor = new Color(0.16f, 0.60f, 0.35f, 1f);
    public Color resetColor = new Color(0.33f, 0.36f, 0.43f, 1f);

    private bool styled = false;

    void OnEnable()
    {
        if (styled) return;
        styled = true;
        Apply();
    }

    void Apply()
    {
        RectTransform panel = transform as RectTransform;
        if (panel == null) return;

        Place(panel, 0f, 0f, 960f, 680f);

        Image bg = GetComponent<Image>();
        if (bg == null) bg = gameObject.AddComponent<Image>();
        bg.color = borderColor;
        bg.raycastTarget = true;

        AddFill(bg);

        StyleText("TitleText", titleLabel, 40f, accentColor, 0f, 262f, 860f, 70f);
        StyleText("SlotText", null, 78f, slotColor, 0f, 140f, 880f, 120f);
        StyleText("AttemptsText", null, 28f, new Color(0.72f, 0.76f, 0.84f), 0f, 22f, 700f, 48f);
        StyleText("FeedbackText", null, 28f, new Color(1f, 0.78f, 0.35f), 0f, -140f, 880f, 60f);

        float[] xs = { -291f, -97f, 97f, 291f };
        for (int i = 0; i < 4; i++)
            StyleButton("FragBtn" + i, fragColor, Color.white, xs[i], -40f, 176f, 84f, 36f);

        StyleButton("SubmitBtn", submitColor, Color.white, -145f, -252f, 260f, 68f, 28f);
        StyleButton("ResetBtn", resetColor, Color.white, 145f, -252f, 260f, 68f, 28f);
    }

    void AddFill(Image source)
    {
        Transform existing = transform.Find("PanelFill");
        if (existing != null) return;

        GameObject go = new GameObject("PanelFill", typeof(RectTransform));
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(5f, 5f);
        rt.offsetMax = new Vector2(-5f, -5f);

        Image img = go.AddComponent<Image>();
        img.color = panelColor;
        img.raycastTarget = false;

        if (source != null && source.sprite != null)
        {
            img.sprite = source.sprite;
            img.type = source.type;
        }
    }

    void Place(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
    }

    void StyleText(string childName, string content, float size, Color color, float x, float y, float w, float h)
    {
        Transform t = transform.Find(childName);
        if (t == null) return;

        Place(t as RectTransform, x, y, w, h);

        TMP_Text txt = t.GetComponent<TMP_Text>();
        if (txt == null) return;

        if (content != null) txt.text = content;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableAutoSizing = false;
        txt.raycastTarget = false;
    }

    void StyleButton(string childName, Color fill, Color labelColor, float x, float y, float w, float h, float size)
    {
        Transform t = transform.Find(childName);
        if (t == null) return;

        Place(t as RectTransform, x, y, w, h);

        Image img = t.GetComponent<Image>();
        if (img != null)
        {
            img.color = fill;
            img.raycastTarget = true;
        }

        Button btn = t.GetComponent<Button>();
        if (btn != null)
        {
            ColorBlock cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = new Color(0.82f, 0.9f, 1f);
            cb.pressedColor = new Color(0.65f, 0.72f, 0.82f);
            cb.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.6f);
            btn.colors = cb;
        }

        TMP_Text txt = t.GetComponentInChildren<TMP_Text>(true);
        if (txt == null) return;

        RectTransform trt = txt.rectTransform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
        trt.localScale = Vector3.one;

        txt.fontSize = size;
        txt.color = labelColor;
        txt.alignment = TextAlignmentOptions.Center;
        txt.enableAutoSizing = false;
        txt.raycastTarget = false;
    }
}
