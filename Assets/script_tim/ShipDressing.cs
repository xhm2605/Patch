using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ShipDressing : MonoBehaviour
{
    public static ShipDressing Instance;

    private static Sprite floorSprite;
    private static Sprite hullSprite;
    private static Sprite stripeSprite;
    private static Sprite panelSprite;
    private static Sprite pipeSprite;
    private static Sprite glowSprite;
    private static Sprite ringSprite;
    private static Sprite crateSprite;
    private static Sprite tankSprite;
    private static Sprite ventSprite;
    private static Sprite consoleSprite;
    private static Sprite chevronSprite;
    private static Sprite padSprite;

    private const int OrderHull = -260;
    private const int OrderFloor = -240;
    private const int OrderMark = -220;
    private const int OrderPool = -215;
    private const int OrderProp = 2;

    private static readonly Color Steel = new Color(0.48f, 0.54f, 0.62f);
    private static readonly Color Cyan = new Color(0.40f, 0.80f, 1f);
    private static readonly Color Amber = new Color(1f, 0.72f, 0.35f);
    private static readonly Color Red = new Color(1f, 0.45f, 0.40f);
    private static readonly Color Green = new Color(0.55f, 0.95f, 0.70f);

    private Transform root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("ShipDressing");
        go.AddComponent<ShipDressing>();
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
        if (SceneManager.GetActiveScene().name == "Main") Build();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Main") Build();
    }

    static Rect Control = Rect.MinMaxRect(-8f, -6f, 8f, 6f);
    static Rect Arcade = Rect.MinMaxRect(-8f, 16f, 8f, 28f);
    static Rect Engine = Rect.MinMaxRect(-8f, -28f, 8f, -16f);
    static Rect Shield = Rect.MinMaxRect(-34f, -6f, -18f, 6f);
    static Rect Oxygen = Rect.MinMaxRect(18f, -6f, 34f, 6f);
    static Rect CorrN = Rect.MinMaxRect(-2f, 6f, 2f, 16f);
    static Rect CorrS = Rect.MinMaxRect(-2f, -16f, 2f, -6f);
    static Rect CorrE = Rect.MinMaxRect(8f, -2f, 18f, 2f);
    static Rect CorrW = Rect.MinMaxRect(-18f, -2f, -8f, 2f);
    static Rect Coolant = Rect.MinMaxRect(18f, 16f, 34f, 28f);
    static Rect CorrNE = Rect.MinMaxRect(24f, 6f, 28f, 16f);

    // Rien ne se pose la : bornes, console de code, depart du joueur
    static readonly Rect[] Reserved =
    {
        Rect.MinMaxRect(3.0f, 0.8f, 8.4f, 6.4f),
        Rect.MinMaxRect(-2.8f, 22.2f, 2.8f, 27.8f),
        Rect.MinMaxRect(-2.8f, -27.8f, 2.8f, -22.2f),
        Rect.MinMaxRect(-28.8f, 0.2f, -23.2f, 5.8f),
        Rect.MinMaxRect(23.2f, 0.2f, 28.8f, 5.8f),
        Rect.MinMaxRect(23.2f, 22.2f, 28.8f, 27.8f),
        Rect.MinMaxRect(-2.6f, -2.6f, 2.6f, 2.6f)
    };

    static bool Free(Vector2 pos, Vector2 size)
    {
        Rect r = new Rect(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, size.x, size.y);

        foreach (Rect b in Reserved)
            if (r.Overlaps(b)) return false;

        return true;
    }

    void Build()
    {
        if (GameObject.Find("ShipDressingRoot") != null) return;

        EnsureSprites();
        root = new GameObject("ShipDressingRoot").transform;

        Rect[] zones =
        {
            Control, Arcade, Engine, Shield, Oxygen,
            CorrN, CorrS, CorrE, CorrW,
            Coolant, CorrNE
        };

        foreach (Rect z in zones) Hull(z);
        foreach (Rect z in zones) Floor(z);

        Markings();
        ServicePads();
        WallProps();
        RoomProps();
        Labels();
    }

    // ---------- Coque et sol ----------

    void Hull(Rect z)
    {
        Rect g = Rect.MinMaxRect(z.xMin - 0.75f, z.yMin - 0.75f, z.xMax + 0.75f, z.yMax + 0.75f);
        Tile("Hull", hullSprite, g, OrderHull, new Color(0.19f, 0.22f, 0.28f));
    }

    void Floor(Rect z)
    {
        Rect g = Rect.MinMaxRect(z.xMin - 0.3f, z.yMin - 0.3f, z.xMax + 0.3f, z.yMax + 0.3f);
        Tile("Floor", floorSprite, g, OrderFloor, new Color(0.62f, 0.68f, 0.76f));
    }

    SpriteRenderer Tile(string objectName, Sprite sprite, Rect area, int order, Color tint)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(area.center.x, area.center.y, 0f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(area.width, area.height);
        sr.sortingOrder = order;
        sr.color = tint;
        return sr;
    }

    SpriteRenderer Piece(string objectName, Sprite sprite, Vector2 pos, Vector2 size,
                         int order, Color tint, float angle = 0f)
    {
        GameObject go = new GameObject(objectName);
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localEulerAngles = new Vector3(0f, 0f, angle);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = size;
        sr.sortingOrder = order;
        sr.color = tint;
        return sr;
    }

    // ---------- Marquages ----------

    void Markings()
    {
        Stripe(new Vector2(0f, 6.4f), new Vector2(4f, 0.5f));
        Stripe(new Vector2(0f, -6.4f), new Vector2(4f, 0.5f));
        Stripe(new Vector2(8.4f, 0f), new Vector2(0.5f, 4f));
        Stripe(new Vector2(-8.4f, 0f), new Vector2(0.5f, 4f));
        Stripe(new Vector2(0f, 15.6f), new Vector2(4f, 0.5f));
        Stripe(new Vector2(0f, -15.6f), new Vector2(4f, 0.5f));
        Stripe(new Vector2(17.6f, 0f), new Vector2(0.5f, 4f));
        Stripe(new Vector2(-17.6f, 0f), new Vector2(0.5f, 4f));
        Stripe(new Vector2(26f, 6.4f), new Vector2(4f, 0.5f));
        Stripe(new Vector2(26f, 15.6f), new Vector2(4f, 0.5f));

        Ring(new Vector2(-3.4f, 0f), 3.5f, new Color(0.45f, 0.72f, 0.92f, 0.22f));
        Ring(new Vector2(-3.4f, 0f), 2.2f, new Color(0.45f, 0.72f, 0.92f, 0.14f));

        Chevrons(new Vector2(0f, 8.5f), 90f, 3, Cyan);
        Chevrons(new Vector2(0f, -8.5f), -90f, 3, Cyan);
        Chevrons(new Vector2(10.5f, 0f), 0f, 3, Cyan);
        Chevrons(new Vector2(-10.5f, 0f), 180f, 3, Cyan);
        Chevrons(new Vector2(26f, 11f), 90f, 3, Green);

        Vent(new Vector2(21f, 19f));
        Vent(new Vector2(-6.2f, -4.2f));
        Vent(new Vector2(6.2f, -4.2f));
        Vent(new Vector2(-6.2f, 19f));
        Vent(new Vector2(6.2f, -19f));
        Vent(new Vector2(-21f, 4.2f));
        Vent(new Vector2(21f, -4.2f));
    }

    void Stripe(Vector2 pos, Vector2 size)
    {
        SpriteRenderer sr = Piece("Hazard", stripeSprite, pos, size, OrderMark, new Color(1f, 1f, 1f, 0.7f));
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
    }

    void Ring(Vector2 pos, float radius, Color c)
    {
        GameObject go = new GameObject("Ring");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = Vector3.one * (radius * 2f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = ringSprite;
        sr.sortingOrder = OrderMark;
        sr.color = c;
    }

    void Chevrons(Vector2 center, float angle, int n, Color c)
    {
        Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

        for (int i = 0; i < n; i++)
        {
            Vector2 p = center + dir * ((i - (n - 1) * 0.5f) * 1.5f);
            if (!Free(p, new Vector2(1.2f, 1.2f))) continue;

            float fade = 0.30f - i * 0.05f;
            Piece("Chevron", chevronSprite, p, new Vector2(1.5f, 0.85f), OrderMark,
                new Color(c.r, c.g, c.b, fade), angle);
        }
    }

    void Vent(Vector2 pos)
    {
        if (!Free(pos, new Vector2(1.5f, 1.5f))) return;
        Piece("Vent", ventSprite, pos, new Vector2(1.5f, 1.5f), OrderMark, new Color(0.55f, 0.60f, 0.68f, 0.9f));
    }

    // ---------- Dalles devant les bornes ----------

    void ServicePads()
    {
        Pad(new Vector2(0f, 21.4f), new Vector2(5.2f, 2.8f), Amber);
        Pad(new Vector2(0f, -21.4f), new Vector2(5.2f, 2.8f), Red);
        Pad(new Vector2(-26f, -0.6f), new Vector2(5.2f, 2.8f), Cyan);
        Pad(new Vector2(26f, -0.6f), new Vector2(5.2f, 2.8f), Green);
        Pad(new Vector2(26f, 21.4f), new Vector2(5.2f, 2.8f), Cyan);
        Pad(new Vector2(5.6f, -0.4f), new Vector2(5.2f, 2.4f), Cyan);
    }

    void Pad(Vector2 pos, Vector2 size, Color c)
    {
        Piece("ServicePad", padSprite, pos, size, OrderPool, new Color(c.r, c.g, c.b, 0.30f));
    }

    // ---------- Equipements muraux ----------

    void WallProps()
    {
        Pipe(new Vector2(-1.74f, 11f), new Vector2(0.26f, 9f), 0f);
        Pipe(new Vector2(1.74f, -11f), new Vector2(0.26f, 9f), 0f);
        Pipe(new Vector2(-13f, 1.74f), new Vector2(0.26f, 9f), 90f);
        Pipe(new Vector2(13f, -1.74f), new Vector2(0.26f, 9f), 90f);
        Pipe(new Vector2(24.26f, 11f), new Vector2(0.26f, 9f), 0f);
        Pipe(new Vector2(27.74f, 11f), new Vector2(0.26f, 9f), 0f);

        Panel(new Vector2(-7.8f, 2.6f), 90f, Cyan);
        Panel(new Vector2(-7.8f, -2.6f), 90f, Cyan);
        Panel(new Vector2(-3.4f, 16.2f), 0f, Amber);
        Panel(new Vector2(3.4f, -16.2f), 0f, Red);
        Panel(new Vector2(-18.2f, 3.4f), 90f, Cyan);
        Panel(new Vector2(18.2f, -3.4f), 90f, Green);
        Panel(new Vector2(-30f, -5.8f), 0f, Cyan);
        Panel(new Vector2(30f, 5.8f), 0f, Green);
        Panel(new Vector2(-2.6f, 27.8f), 0f, Amber);
        Panel(new Vector2(2.6f, -27.8f), 0f, Red);
        Panel(new Vector2(26f, 27.8f), 0f, Cyan);
        Panel(new Vector2(18.2f, 20f), 90f, Cyan);
        Panel(new Vector2(33.8f, 24f), 90f, Cyan);

        Led(new Vector2(-7.85f, 0f), Cyan, 1.7f);
        Led(new Vector2(0f, 15.85f), Amber, 1.1f);
        Led(new Vector2(0f, -15.85f), Red, 0.8f);
        Led(new Vector2(-17.85f, 0f), Cyan, 2.9f);
        Led(new Vector2(17.85f, 0f), Green, 1.4f);
        Led(new Vector2(0f, 6.45f), Cyan, 2.2f);
        Led(new Vector2(0f, -6.45f), Cyan, 1.9f);
        Led(new Vector2(26f, 15.85f), Cyan, 1.3f);
        Led(new Vector2(26f, 6.45f), Cyan, 2.4f);
    }

    void Pipe(Vector2 pos, Vector2 size, float angle)
    {
        SpriteRenderer sr = Piece("Pipe", pipeSprite, pos, size, OrderProp, new Color(0.78f, 0.82f, 0.88f), angle);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.size = size;
    }

    void Panel(Vector2 pos, float angle, Color glow)
    {
        Piece("Panel", panelSprite, pos, new Vector2(1.7f, 0.62f), OrderProp, Steel, angle);

        GameObject go = new GameObject("PanelGlow");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localEulerAngles = new Vector3(0f, 0f, angle);
        go.transform.localScale = new Vector3(1.05f, 0.34f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = glowSprite;
        sr.sortingOrder = OrderProp + 1;
        sr.color = glow;

        Blink(go, glow, Random.Range(0.8f, 1.5f), 0.30f, 0.80f);
    }

    void Led(Vector2 pos, Color c, float speed)
    {
        GameObject go = new GameObject("Led");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localScale = Vector3.one * 0.42f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = glowSprite;
        sr.sortingOrder = OrderProp + 1;
        sr.color = c;

        Blink(go, c, speed, 0.15f, 1f);
    }

    void Blink(GameObject go, Color c, float speed, float low, float high)
    {
        ShipBlink b = go.AddComponent<ShipBlink>();
        b.baseColor = c;
        b.speed = speed;
        b.phase = Random.Range(0f, 6f);
        b.low = low;
        b.high = high;
    }

    // ---------- Mobilier des salles ----------

    void RoomProps()
    {
        Console(new Vector2(-6.4f, 3.4f), 0f, Cyan);
        Console(new Vector2(-6.4f, -3.4f), 0f, Cyan);

        Crate(new Vector2(-6.3f, 25.8f), 12f);
        Crate(new Vector2(-4.6f, 24.4f), -8f);
        Crate(new Vector2(6.4f, 18.6f), 22f);

        Tank(new Vector2(-6.4f, -25.4f), Red);
        Tank(new Vector2(6.4f, -25.4f), Amber);
        Crate(new Vector2(-6.2f, -18.6f), -14f);

        Crate(new Vector2(-31.6f, 4.2f), 6f);
        Crate(new Vector2(-30.2f, 2.8f), -18f);
        Console(new Vector2(-20.6f, -3.6f), 0f, Cyan);

        Tank(new Vector2(31.8f, 4.2f), Green);
        Tank(new Vector2(30.2f, 4.2f), Green);
        Console(new Vector2(20.6f, -3.6f), 0f, Green);

        Tank(new Vector2(20.2f, 25.6f), Cyan);
        Tank(new Vector2(21.8f, 25.6f), Cyan);
        Console(new Vector2(31.4f, 25.2f), 0f, Cyan);
        Crate(new Vector2(31.8f, 18.8f), 10f);
        Crate(new Vector2(30.1f, 20.2f), -12f);
    }

    void Crate(Vector2 pos, float angle)
    {
        if (!Free(pos, new Vector2(1.7f, 1.7f))) return;
        Piece("Crate", crateSprite, pos, new Vector2(1.6f, 1.6f), OrderProp, new Color(0.82f, 0.80f, 0.74f), angle);
    }

    void Tank(Vector2 pos, Color c)
    {
        if (!Free(pos, new Vector2(1.3f, 2.4f))) return;

        Piece("Tank", tankSprite, pos, new Vector2(1.2f, 2.3f), OrderProp, new Color(0.86f, 0.89f, 0.94f));

        GameObject go = new GameObject("TankLight");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y + 0.85f, 0f);
        go.transform.localScale = Vector3.one * 0.35f;

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = glowSprite;
        sr.sortingOrder = OrderProp + 1;
        sr.color = c;

        Blink(go, c, Random.Range(1f, 2f), 0.2f, 0.9f);
    }

    void Console(Vector2 pos, float angle, Color c)
    {
        if (!Free(pos, new Vector2(2.4f, 1.4f))) return;

        Piece("Console", consoleSprite, pos, new Vector2(2.2f, 1.2f), OrderProp, Steel, angle);

        GameObject go = new GameObject("ConsoleScreen");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y + 0.16f, 0f);
        go.transform.localEulerAngles = new Vector3(0f, 0f, angle);
        go.transform.localScale = new Vector3(1.3f, 0.5f, 1f);

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = glowSprite;
        sr.sortingOrder = OrderProp + 1;
        sr.color = c;

        Blink(go, c, Random.Range(0.6f, 1.2f), 0.35f, 0.75f);
    }

    // ---------- Etiquettes ----------

    void Labels()
    {
        Label("CONTROL", new Vector2(-3.4f, -4.9f), 2.3f, new Color(0.55f, 0.78f, 0.95f, 0.36f));
        Label("ARCADE BAY", new Vector2(0f, 17.1f), 2.1f, new Color(1f, 0.78f, 0.45f, 0.32f));
        Label("ENGINE", new Vector2(0f, -17.1f), 2.1f, new Color(1f, 0.55f, 0.45f, 0.32f));
        Label("SHIELD", new Vector2(-26f, -4.9f), 2.1f, new Color(0.55f, 0.80f, 1f, 0.32f));
        Label("OXYGEN", new Vector2(26f, -4.9f), 2.1f, new Color(0.60f, 0.95f, 0.72f, 0.32f));
        Label("COOLANT", new Vector2(26f, 17.1f), 2.1f, new Color(0.55f, 0.85f, 1f, 0.32f));
    }

    void Label(string content, Vector2 pos, float size, Color c)
    {
        GameObject go = new GameObject("Label_" + content);
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        TextMeshPro t = go.AddComponent<TextMeshPro>();
        t.text = content;
        t.fontSize = size;
        t.color = c;
        t.alignment = TextAlignmentOptions.Center;
        t.characterSpacing = 14f;
        t.rectTransform.sizeDelta = new Vector2(14f, 3f);

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr != null) mr.sortingOrder = OrderMark + 1;
    }

    // ---------- Textures ----------

    static void EnsureSprites()
    {
        if (floorSprite != null) return;

        floorSprite = Full(BuildFloor(256), 64f);
        hullSprite = Full(BuildHull(128), 64f);
        stripeSprite = Full(BuildStripes(64), 64f);
        panelSprite = Full(BuildPanel(96, 40), 48f);
        pipeSprite = Full(BuildPipe(32, 96), 64f);
        glowSprite = Full(BuildGlow(96), 96f);
        ringSprite = Full(BuildRing(256), 256f);
        crateSprite = Full(BuildCrate(96), 64f);
        tankSprite = Full(BuildTank(64, 128), 64f);
        ventSprite = Full(BuildVent(64), 64f);
        consoleSprite = Full(BuildConsole(128, 64), 64f);
        chevronSprite = Full(BuildChevron(64, 40), 64f);
        padSprite = Full(BuildPad(128, 72), 64f);
    }

    static Sprite Full(Texture2D tex, float ppu)
    {
        return Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height),
            new Vector2(0.5f, 0.5f), ppu, 0, SpriteMeshType.FullRect);
    }

    static Texture2D Make(int w, int h, Color[] px)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();
        return tex;
    }

    static Texture2D BuildFloor(int s)
    {
        Color[] px = new Color[s * s];
        int plate = s / 2;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                int lx = x % plate, ly = y % plate;

                float n = Mathf.PerlinNoise(x * 0.09f, y * 0.09f) * 0.10f;
                float grain = Mathf.PerlinNoise(x * 0.55f, y * 0.55f) * 0.05f;
                float v = 0.50f + n + grain;

                int edge = Mathf.Min(Mathf.Min(lx, plate - 1 - lx), Mathf.Min(ly, plate - 1 - ly));
                if (edge < 2) v *= 0.52f;
                else if (edge < 4) v *= 0.80f;

                float rd = Mathf.Min(
                    Vector2.Distance(new Vector2(lx, ly), new Vector2(9f, 9f)),
                    Vector2.Distance(new Vector2(lx, ly), new Vector2(plate - 9f, plate - 9f)));

                if (rd < 3.2f) v += (1f - rd / 3.2f) * 0.22f;

                px[y * s + x] = new Color(v * 0.86f, v * 0.92f, v, 1f);
            }
        }

        return Make(s, s, px);
    }

    static Texture2D BuildHull(int s)
    {
        Color[] px = new Color[s * s];

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.06f, y * 0.06f) * 0.22f;
                float streak = Mathf.PerlinNoise(x * 0.02f, y * 0.6f) * 0.12f;
                float v = 0.42f + n + streak;

                if (y % 32 < 2) v *= 0.60f;

                px[y * s + x] = new Color(v * 0.88f, v * 0.94f, v, 1f);
            }
        }

        return Make(s, s, px);
    }

    static Texture2D BuildStripes(int s)
    {
        Color[] px = new Color[s * s];
        Color warn = new Color(0.95f, 0.72f, 0.12f);
        Color dark = new Color(0.10f, 0.11f, 0.13f);

        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
                px[y * s + x] = ((x + y) % 24) < 12 ? warn : dark;

        return Make(s, s, px);
    }

    static Texture2D BuildPanel(int w, int h)
    {
        Color[] px = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int e = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));

                float v = 0.34f + Mathf.PerlinNoise(x * 0.12f, y * 0.12f) * 0.10f;
                if (e < 2) v = 0.58f;
                else if (e < 4) v = 0.22f;

                px[y * w + x] = new Color(v * 0.88f, v * 0.94f, v, 1f);
            }
        }

        return Make(w, h, px);
    }

    static Texture2D BuildPipe(int w, int h)
    {
        Color[] px = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float u = x / (float)(w - 1);
                float round = Mathf.Sin(u * Mathf.PI);
                float v = 0.28f + round * round * 0.55f;

                if (y % 24 < 3) v *= 0.72f;

                float a = round > 0.12f ? 1f : round / 0.12f;
                px[y * w + x] = new Color(v * 0.88f, v * 0.93f, v, Mathf.Clamp01(a));
            }
        }

        return Make(w, h, px);
    }

    static Texture2D BuildGlow(int s)
    {
        Color[] px = new Color[s * s];
        float c = s * 0.5f;

        for (int y = 0; y < s; y++)
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c;
                px[y * s + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(Mathf.Exp(-d * d * 6f)));
            }

        return Make(s, s, px);
    }

    static Texture2D BuildRing(int s)
    {
        Color[] px = new Color[s * s];
        float c = s * 0.5f;
        float r = s * 0.46f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float band = Mathf.Abs(d - r);

                float a = band < 2.5f ? 1f : 0f;
                if (band >= 2.5f && band < 4f) a = 1f - (band - 2.5f) / 1.5f;

                float ang = Mathf.Atan2(y - c, x - c);
                if (Mathf.Repeat(ang, Mathf.PI / 8f) < 0.10f) a *= 0.25f;

                px[y * s + x] = new Color(1f, 1f, 1f, a);
            }
        }

        return Make(s, s, px);
    }

    static Texture2D BuildCrate(int s)
    {
        Color[] px = new Color[s * s];
        float cut = s * 0.14f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float fx = x, fy = y;
                bool corner = (fx + fy < cut) || (fx + (s - fy) < cut)
                           || ((s - fx) + fy < cut) || ((s - fx) + (s - fy) < cut);

                if (corner)
                {
                    px[y * s + x] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                int e = Mathf.Min(Mathf.Min(x, s - 1 - x), Mathf.Min(y, s - 1 - y));
                float v = 0.52f + Mathf.PerlinNoise(x * 0.10f, y * 0.10f) * 0.12f;

                if (e < 3) v = 0.30f;
                else if (e < 6) v = 0.68f;

                float dia = Mathf.Min(Mathf.Abs(x - y), Mathf.Abs(x - (s - 1 - y)));
                if (dia < 3f && e > 7) v *= 0.72f;

                if (y > s * 0.42f && y < s * 0.58f && e > 7) v = 0.80f;

                px[y * s + x] = new Color(v * 0.94f, v * 0.90f, v * 0.78f, 1f);
            }
        }

        return Make(s, s, px);
    }

    static Texture2D BuildTank(int w, int h)
    {
        Color[] px = new Color[w * h];
        float c = w * 0.5f;
        float r = w * 0.42f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - c);
                float capTop = h - w * 0.5f;
                float capBot = w * 0.5f;

                float dist;
                if (y > capTop) dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, capTop));
                else if (y < capBot) dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, capBot));
                else dist = dx;

                if (dist > r)
                {
                    px[y * w + x] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                float round = Mathf.Cos(dx / r * 1.35f);
                float v = 0.30f + round * 0.55f;

                if (y % 30 < 4) v *= 0.68f;

                float a = Mathf.Clamp01((r - dist) * 1.2f);
                px[y * w + x] = new Color(v * 0.90f, v * 0.94f, v, a);
            }
        }

        return Make(w, h, px);
    }

    static Texture2D BuildVent(int s)
    {
        Color[] px = new Color[s * s];

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                int e = Mathf.Min(Mathf.Min(x, s - 1 - x), Mathf.Min(y, s - 1 - y));

                float v;
                if (e < 3) v = 0.62f;
                else if (e < 6) v = 0.30f;
                else v = (y % 9 < 5) ? 0.16f : 0.46f;

                px[y * s + x] = new Color(v * 0.90f, v * 0.94f, v, 1f);
            }
        }

        return Make(s, s, px);
    }

    static Texture2D BuildConsole(int w, int h)
    {
        Color[] px = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int e = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));

                float v = 0.30f + Mathf.PerlinNoise(x * 0.09f, y * 0.09f) * 0.08f;

                if (e < 3) v = 0.55f;
                else if (e < 5) v = 0.18f;

                if (y > h * 0.52f && y < h * 0.88f && x > w * 0.12f && x < w * 0.88f) v = 0.12f;
                if (y < h * 0.34f && y > h * 0.14f && ((x / 9) % 2 == 0) && e > 6) v = 0.62f;

                float a = 1f;
                if ((x + y < 8) || (w - x + y < 8) || (x + h - y < 8) || (w - x + h - y < 8)) a = 0f;

                px[y * w + x] = new Color(v * 0.88f, v * 0.94f, v, a);
            }
        }

        return Make(w, h, px);
    }

    static Texture2D BuildChevron(int w, int h)
    {
        Color[] px = new Color[w * h];
        float mid = h * 0.5f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float target = w * 0.30f + Mathf.Abs(y - mid) * 1.15f;
                float band = Mathf.Abs(x - target);

                float a = band < 5f ? 1f : 0f;
                if (band >= 5f && band < 7.5f) a = 1f - (band - 5f) / 2.5f;

                px[y * w + x] = new Color(1f, 1f, 1f, a);
            }
        }

        return Make(w, h, px);
    }

    static Texture2D BuildPad(int w, int h)
    {
        Color[] px = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int e = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));

                float a = 0.22f;

                bool dash = (x / 8 + y / 8) % 2 == 0;
                if (e < 3 && dash) a = 0.95f;
                else if (e < 3) a = 0.10f;

                bool corner = (x < 16 && y < 16) || (x < 16 && y > h - 17)
                           || (x > w - 17 && y < 16) || (x > w - 17 && y > h - 17);
                if (corner && e < 7) a = 0.95f;

                px[y * w + x] = new Color(1f, 1f, 1f, a);
            }
        }

        return Make(w, h, px);
    }
}

public class ShipBlink : MonoBehaviour
{
    public Color baseColor = Color.white;
    public float speed = 1f;
    public float phase = 0f;
    public float low = 0.2f;
    public float high = 1f;

    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (sr == null) return;

        float k = 0.5f + 0.5f * Mathf.Sin(Time.time * speed + phase);
        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, Mathf.Lerp(low, high, k));
    }
}
