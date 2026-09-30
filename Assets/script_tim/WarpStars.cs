using UnityEngine;
using UnityEngine.UI;

public class WarpStars : MonoBehaviour
{
    public int count = 150;
    public float startSpeed = 1.05f;
    public float endSpeed = 2.05f;
    public float rampSeconds = 18f;
    public float maxRadius = 1280f;
    public float trail = 0.22f;
    public float masterAlpha = 0.8f;

    private static Sprite streakSprite;

    private RectTransform[] rects;
    private Image[] images;
    private Vector2[] dirs;
    private float[] dists;
    private float[] widths;
    private float[] shades;

    private float elapsed;

    public static WarpStars Attach(Transform parent, int starCount)
    {
        GameObject go = new GameObject("WarpStars", typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Canvas imbrique : les etoiles ne forcent pas le texte a se reconstruire
        go.AddComponent<Canvas>();

        WarpStars w = go.AddComponent<WarpStars>();
        w.count = starCount;
        return w;
    }

    void Start()
    {
        EnsureSprite();

        rects = new RectTransform[count];
        images = new Image[count];
        dirs = new Vector2[count];
        dists = new float[count];
        widths = new float[count];
        shades = new float[count];

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject("Streak", typeof(RectTransform));
            go.layer = gameObject.layer;
            go.transform.SetParent(transform, false);

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);

            Image img = go.AddComponent<Image>();
            img.sprite = streakSprite;
            img.raycastTarget = false;

            rects[i] = rt;
            images[i] = img;

            Spawn(i, Random.Range(20f, maxRadius));
        }
    }

    void Spawn(int i, float distance)
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);

        dirs[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        dists[i] = distance;
        widths[i] = Random.Range(1.4f, 3.4f);
        shades[i] = Random.Range(0.35f, 1f);

        rects[i].localEulerAngles = new Vector3(0f, 0f, angle * Mathf.Rad2Deg);
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;
        elapsed += dt;

        float ramp = Mathf.Clamp01(elapsed / rampSeconds);
        float speed = Mathf.Lerp(startSpeed, endSpeed, ramp * ramp);
        float grow = Mathf.Exp(speed * dt);

        for (int i = 0; i < count; i++)
        {
            float d = dists[i] * grow + 26f * dt;

            if (d > maxRadius)
            {
                Spawn(i, Random.Range(6f, 26f));
                d = dists[i];
            }
            else
            {
                dists[i] = d;
            }

            float length = Mathf.Max(4f, d * trail);

            rects[i].anchoredPosition = dirs[i] * d;
            rects[i].sizeDelta = new Vector2(length, widths[i]);

            float a = Mathf.Clamp01(d / 170f) * shades[i] * masterAlpha;
            images[i].color = new Color(1f, 1f, 1f, a);
        }
    }

    static void EnsureSprite()
    {
        if (streakSprite != null) return;

        int w = 64, h = 4;
        Color[] px = new Color[w * h];

        for (int x = 0; x < w; x++)
        {
            float u = x / (float)(w - 1);
            float a = Mathf.Pow(u, 2.8f);
            Color c = Color.Lerp(new Color(0.52f, 0.74f, 1f), Color.white, u * u);

            for (int y = 0; y < h; y++)
            {
                float vy = (y == 0 || y == h - 1) ? 0.42f : 1f;
                px[y * w + x] = new Color(c.r, c.g, c.b, a * vy);
            }
        }

        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        streakSprite = Sprite.Create(tex, new Rect(0f, 0f, w, h), new Vector2(0f, 0.5f), 100f);
    }
}
