using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SpaceBackdrop : MonoBehaviour
{
    public static SpaceBackdrop Instance;

    private static Texture2D starTex;
    private static Texture2D planetTex;
    private static Sprite starSprite;
    private static Sprite planetSprite;

    private Transform worldStars;
    private Camera cam;
    private Vector3 anchorPoint;
    private float parallax = 0.35f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("SpaceBackdrop");
        go.AddComponent<SpaceBackdrop>();
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
        Apply(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Apply(scene.name);
    }

    void Apply(string sceneName)
    {
        worldStars = null;
        cam = null;

        if (sceneName == "MainMenu") BuildMenuBackdrop();
        else if (sceneName == "Main") BuildWorldBackdrop();
    }

    void LateUpdate()
    {
        if (worldStars == null || cam == null) return;

        Vector3 p = cam.transform.position;
        worldStars.position = new Vector3(
            anchorPoint.x + (p.x - anchorPoint.x) * (1f - parallax),
            anchorPoint.y + (p.y - anchorPoint.y) * (1f - parallax),
            10f);
    }

    // ---------- Menu : espace + planete ----------

    void BuildMenuBackdrop()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        EnsureSprites();

        GameObject sky = NewUI("SpaceBackdrop", canvas.transform);
        RectTransform srt = sky.GetComponent<RectTransform>();
        srt.anchorMin = Vector2.zero;
        srt.anchorMax = Vector2.one;
        srt.offsetMin = Vector2.zero;
        srt.offsetMax = Vector2.zero;

        Image simg = sky.AddComponent<Image>();
        simg.sprite = starSprite;
        simg.type = Image.Type.Simple;
        simg.preserveAspect = false;
        simg.raycastTarget = false;

        GameObject planet = NewUI("Planet", sky.transform);
        RectTransform prt = planet.GetComponent<RectTransform>();
        prt.anchorMin = new Vector2(0.5f, 0f);
        prt.anchorMax = new Vector2(0.5f, 0f);
        prt.pivot = new Vector2(0.5f, 0.5f);
        prt.sizeDelta = new Vector2(2600f, 2600f);
        prt.anchoredPosition = new Vector2(0f, -402f);

        Image pimg = planet.AddComponent<Image>();
        pimg.sprite = planetSprite;
        pimg.raycastTarget = false;

        sky.transform.SetAsFirstSibling();
    }

    // ---------- Partie : champ d'etoiles derriere la map ----------

    void BuildWorldBackdrop()
    {
        cam = Camera.main;
        if (cam == null) return;

        EnsureSprites();

        GameObject go = new GameObject("SpaceBackdropWorld");
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = starSprite;
        sr.sortingOrder = -500;
        sr.color = new Color(1f, 1f, 1f, 0.95f);

        float viewHeight = cam.orthographic ? cam.orthographicSize * 2f : 12f;
        float viewWidth = viewHeight * cam.aspect;
        float span = Mathf.Max(viewWidth, viewHeight) * 3.2f;

        float texSize = starSprite.bounds.size.x;
        if (texSize <= 0.001f) texSize = 1f;
        go.transform.localScale = Vector3.one * (span / texSize);

        anchorPoint = cam.transform.position;
        go.transform.position = new Vector3(anchorPoint.x, anchorPoint.y, 10f);

        worldStars = go.transform;
    }

    GameObject NewUI(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    // ---------- Textures ----------

    static void EnsureSprites()
    {
        if (starTex == null)
        {
            starTex = BuildStarfield(1024, 20251);
            starSprite = Sprite.Create(starTex,
                new Rect(0f, 0f, starTex.width, starTex.height),
                new Vector2(0.5f, 0.5f), 100f);
        }

        if (planetTex == null)
        {
            planetTex = BuildPlanet(512, 771);
            planetSprite = Sprite.Create(planetTex,
                new Rect(0f, 0f, planetTex.width, planetTex.height),
                new Vector2(0.5f, 0.5f), 100f);
        }
    }

    static Texture2D BuildStarfield(int size, int seed)
    {
        System.Random rng = new System.Random(seed);
        Color[] px = new Color[size * size];

        int n = 96;
        float[] blue = NoiseField(n, 2.6f, rng.Next(), 4);
        float[] rose = NoiseField(n, 4.4f, rng.Next(), 3);

        Color deep = new Color(0.010f, 0.016f, 0.040f);
        Color blueGlow = new Color(0.10f, 0.17f, 0.45f);
        Color roseGlow = new Color(0.26f, 0.06f, 0.24f);

        for (int y = 0; y < size; y++)
        {
            float v = y / (float)(size - 1);

            for (int x = 0; x < size; x++)
            {
                float u = x / (float)(size - 1);

                float a = Mathf.Pow(Mathf.Clamp01(Sample(blue, n, u, v) * 1.30f - 0.38f), 2.2f);
                float b = Mathf.Pow(Mathf.Clamp01(Sample(rose, n, u, v) * 1.25f - 0.48f), 2.8f);

                px[y * size + x] = deep + blueGlow * a + roseGlow * b;
            }
        }

        int stars = (size * size) / 780;

        for (int i = 0; i < stars; i++)
        {
            int x = rng.Next(size);
            int y = rng.Next(size);

            float m = (float)rng.NextDouble();
            float bright = Mathf.Lerp(0.18f, 1f, m * m * m);

            float warm = (float)rng.NextDouble();
            Color tint = warm < 0.18f ? new Color(1f, 0.82f, 0.66f)
                       : warm > 0.80f ? new Color(0.72f, 0.84f, 1f)
                       : Color.white;

            Blend(px, size, x, y, tint * bright);

            if (m > 0.955f)
            {
                int r = 2 + rng.Next(4);

                for (int dy = -r; dy <= r; dy++)
                {
                    for (int dx = -r; dx <= r; dx++)
                    {
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        if (d > r || (dx == 0 && dy == 0)) continue;

                        float f = Mathf.Exp(-d * 1.5f) * bright * 0.55f;
                        Blend(px, size, x + dx, y + dy, tint * f);
                    }
                }

                int arm = r + 2 + rng.Next(3);

                for (int k = 1; k <= arm; k++)
                {
                    float f = (1f - k / (float)arm) * bright * 0.30f;
                    Blend(px, size, x + k, y, tint * f);
                    Blend(px, size, x - k, y, tint * f);
                    Blend(px, size, x, y + k, tint * f);
                    Blend(px, size, x, y - k, tint * f);
                }
            }
        }

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGB24, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    static Texture2D BuildPlanet(int size, int seed)
    {
        Color[] px = new Color[size * size];

        float c = size * 0.5f;
        float R = size * 0.30f;

        Vector3 light = new Vector3(-0.38f, 0.52f, 0.76f).normalized;

        Color ocean = new Color(0.03f, 0.13f, 0.34f);
        Color shelf = new Color(0.06f, 0.30f, 0.46f);
        Color land = new Color(0.16f, 0.32f, 0.15f);
        Color dry = new Color(0.42f, 0.36f, 0.20f);
        Color air = new Color(0.36f, 0.64f, 1f);

        float so = seed * 0.37f;

        for (int y = 0; y < size; y++)
        {
            float dy = (y - c) / R;

            for (int x = 0; x < size; x++)
            {
                float dx = (x - c) / R;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                int idx = y * size + x;

                if (d <= 1f)
                {
                    float nz = Mathf.Sqrt(Mathf.Max(0f, 1f - d * d));
                    float lum = Mathf.Max(0f, dx * light.x + dy * light.y + nz * light.z);

                    float s = 1f / (nz + 0.30f);
                    float px1 = dx * s, py1 = dy * s;

                    float h = Fbm(px1 * 1.5f + so, py1 * 1.5f + so, 4);
                    Color surface;

                    if (h < 0.50f)
                    {
                        surface = Color.Lerp(ocean, shelf, Mathf.InverseLerp(0.36f, 0.50f, h));
                    }
                    else
                    {
                        float k = Mathf.InverseLerp(0.50f, 0.72f, h);
                        surface = Color.Lerp(land, dry, k);
                    }

                    float cl = Fbm(px1 * 3.0f + 41f, py1 * 3.0f - 27f, 3);
                    if (cl > 0.54f)
                        surface = Color.Lerp(surface, Color.white, Mathf.Clamp01((cl - 0.54f) * 2.6f) * 0.85f);

                    float shade = Mathf.Max(0.045f, lum);
                    surface *= shade;

                    if (nz < 0.42f)
                    {
                        float rim = Mathf.Pow(1f - nz / 0.42f, 2.4f);
                        surface += air * rim * lum * 0.55f;
                    }

                    float edge = Mathf.Clamp01((1f - d) * R * 1.6f);
                    px[idx] = new Color(surface.r, surface.g, surface.b, edge);
                }
                else if (d < 1.55f)
                {
                    float t = (d - 1f) / 0.55f;
                    float fall = Mathf.Exp(-t * 5.2f);

                    float dot = (dx * light.x + dy * light.y) / Mathf.Max(0.0001f, d);
                    float side = 0.30f + 0.70f * Mathf.Max(0f, dot);

                    px[idx] = new Color(air.r, air.g, air.b, fall * side * 0.60f);
                }
                else
                {
                    px[idx] = new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    static void Blend(Color[] px, int size, int x, int y, Color add)
    {
        if (x < 0 || y < 0 || x >= size || y >= size) return;

        int i = y * size + x;
        px[i] = new Color(
            Mathf.Min(1f, px[i].r + add.r),
            Mathf.Min(1f, px[i].g + add.g),
            Mathf.Min(1f, px[i].b + add.b));
    }

    static float[] NoiseField(int n, float scale, int seed, int octaves)
    {
        float[] f = new float[n * n];
        float ox = (seed % 1000) * 0.731f;
        float oy = (seed % 997) * 0.519f;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float u = x / (float)n * scale + ox;
                float v = y / (float)n * scale + oy;
                f[y * n + x] = Fbm(u, v, octaves);
            }
        }

        return f;
    }

    static float Fbm(float x, float y, int octaves)
    {
        float sum = 0f, amp = 0.5f, freq = 1f, norm = 0f;

        for (int i = 0; i < octaves; i++)
        {
            sum += Mathf.PerlinNoise(x * freq, y * freq) * amp;
            norm += amp;
            amp *= 0.5f;
            freq *= 2f;
        }

        return sum / norm;
    }

    static float Sample(float[] f, int n, float u, float v)
    {
        float fx = Mathf.Clamp01(u) * (n - 1);
        float fy = Mathf.Clamp01(v) * (n - 1);

        int x0 = Mathf.FloorToInt(fx), y0 = Mathf.FloorToInt(fy);
        int x1 = Mathf.Min(x0 + 1, n - 1), y1 = Mathf.Min(y0 + 1, n - 1);

        float tx = fx - x0, ty = fy - y0;

        float a = Mathf.Lerp(f[y0 * n + x0], f[y0 * n + x1], tx);
        float b = Mathf.Lerp(f[y1 * n + x0], f[y1 * n + x1], tx);
        return Mathf.Lerp(a, b, ty);
    }
}
