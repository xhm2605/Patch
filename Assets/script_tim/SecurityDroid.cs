using System.Collections.Generic;
using UnityEngine;

public class SecurityDroid : MonoBehaviour
{
    public float moveSpeed = 2.8f;
    public float turnSpeed = 150f;
    public float sightRange = 7.5f;
    public float sightHalfAngle = 34f;

    private static Sprite bodySprite;
    private static Sprite coneSprite;

    private SpriteRenderer body;
    private SpriteRenderer cone;
    private Transform coneT;

    private enum Mode { Travel, Wander, Sweep }

    private Mode mode = Mode.Travel;
    private int node;
    private int previous = -1;
    private Vector2 target;
    private Vector2 facing = Vector2.up;
    private float sweepUntil;
    private float sweepAngle;
    private int wanderLeft;
    private float bob;

    private static readonly Vector2[] Nodes =
    {
        new Vector2(0f, 0f),       // 0  Control
        new Vector2(0f, 11f),      // 1
        new Vector2(0f, 22f),      // 2  Arcade
        new Vector2(0f, -11f),     // 3
        new Vector2(0f, -22f),     // 4  Engine
        new Vector2(-13f, 0f),     // 5
        new Vector2(-26f, 0f),     // 6  Shield
        new Vector2(13f, 0f),      // 7
        new Vector2(26f, 0f),      // 8  Oxygen
        new Vector2(13f, 22f),     // 9
        new Vector2(26f, 22f),     // 10 Coolant
        new Vector2(-13f, 22f),    // 11
        new Vector2(-26f, 21f),    // 12 Cargo
        new Vector2(-26f, 11f),    // 13
        new Vector2(-26f, -11f),   // 14
        new Vector2(-26f, -21f),   // 15 Quarters
        new Vector2(-13f, -22f),   // 16
        new Vector2(13f, -22f),    // 17
        new Vector2(26f, -21f),    // 18 Observation
        new Vector2(26f, -11f),    // 19
        new Vector2(26f, 11f),     // 20
        new Vector2(-33f, 24f),    // 21 bras de la soute
        new Vector2(-33f, 30f),    // 22
        new Vector2(-27f, -24f),   // 23 alcove des quartiers
        new Vector2(-27f, -30f),   // 24
        new Vector2(31f, -24f),    // 25 coupole d'observation
        new Vector2(31f, -30f)     // 26
    };

    // Rayon d'errance : les salles laissent le droide tourner autour, pas les couloirs
    private static readonly float[] Roam =
    {
        4.5f, 0f, 4.5f, 0f, 4.5f, 0f, 4.5f, 0f, 4.5f, 0f, 4.5f,
        0f, 4f, 0f, 0f, 4f, 0f, 0f, 4f, 0f, 0f,
        2.5f, 2.5f, 2f, 2f, 2.5f, 2.5f
    };

    private static readonly int[][] Links =
    {
        new[] { 1, 3, 5, 7 },      // 0
        new[] { 0, 2 },            // 1
        new[] { 1, 9, 11 },        // 2
        new[] { 0, 4 },            // 3
        new[] { 3, 16, 17 },       // 4
        new[] { 0, 6 },            // 5
        new[] { 5, 13, 14 },       // 6
        new[] { 0, 8 },            // 7
        new[] { 7, 19, 20 },       // 8
        new[] { 2, 10 },           // 9
        new[] { 9, 20 },           // 10
        new[] { 2, 12 },           // 11
        new[] { 11, 13, 21 },      // 12
        new[] { 6, 12 },           // 13
        new[] { 6, 15 },           // 14
        new[] { 14, 16, 23 },      // 15
        new[] { 4, 15 },           // 16
        new[] { 4, 18 },           // 17
        new[] { 17, 19, 25 },      // 18
        new[] { 8, 18 },           // 19
        new[] { 8, 10 },           // 20
        new[] { 12, 22 },          // 21
        new[] { 21 },              // 22
        new[] { 15, 24 },          // 23
        new[] { 23 },              // 24
        new[] { 18, 26 },          // 25
        new[] { 25 }               // 26
    };

    public static Vector2 NodeAt(int index)
    {
        return Nodes[Mathf.Clamp(index, 0, Nodes.Length - 1)];
    }

    public static int NodeCount { get { return Nodes.Length; } }

    public void Spawn(int startNode, float speed)
    {
        node = Mathf.Clamp(startNode, 0, Nodes.Length - 1);
        moveSpeed = speed;

        transform.position = Nodes[node];
        target = Nodes[node];
        Decide();
    }

    void Awake()
    {
        EnsureSprites();
        Build();
    }

    void Build()
    {
        GameObject coneGo = new GameObject("Cone");
        coneGo.transform.SetParent(transform, false);
        coneT = coneGo.transform;

        cone = coneGo.AddComponent<SpriteRenderer>();
        cone.sprite = coneSprite;
        cone.sortingOrder = 12;
        cone.color = new Color(1f, 0.22f, 0.20f, 0.26f);

        GameObject bodyGo = new GameObject("Body");
        bodyGo.transform.SetParent(transform, false);

        body = bodyGo.AddComponent<SpriteRenderer>();
        body.sprite = bodySprite;
        body.sortingOrder = 420;
    }

    // Que faire en arrivant quelque part : flaner, balayer, ou repartir
    void Decide()
    {
        float roll = Random.value;

        if (roll < 0.34f)
        {
            mode = Mode.Sweep;
            sweepUntil = Time.time + Random.Range(1.3f, 2.8f);

            float turn = Random.Range(70f, 165f) * (Random.value < 0.5f ? -1f : 1f);
            sweepAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg + turn;
            return;
        }

        if (Roam[node] > 0.1f && (wanderLeft > 0 || roll < 0.72f))
        {
            if (wanderLeft <= 0) wanderLeft = Random.Range(1, 4);
            wanderLeft--;

            mode = Mode.Wander;

            Vector2 offset = Random.insideUnitCircle * Roam[node];
            target = Nodes[node] + offset;
            return;
        }

        wanderLeft = 0;
        mode = Mode.Travel;
        NextNode();
    }

    void NextNode()
    {
        int[] options = Links[node];
        List<int> pool = new List<int>();

        // Demi-tour autorise de temps en temps, sinon on evite de revenir sur ses pas
        bool allowBack = Random.value < 0.22f || options.Length == 1;

        foreach (int n in options)
            if (allowBack || n != previous) pool.Add(n);

        if (pool.Count == 0) pool.AddRange(options);

        previous = node;
        node = pool[Random.Range(0, pool.Count)];
        target = Nodes[node];
    }

    void Update()
    {
        float dt = Time.deltaTime;

        bob += dt * 3.4f;
        body.transform.localPosition = new Vector3(0f, Mathf.Sin(bob) * 0.09f, 0f);

        if (mode == Mode.Sweep) Sweep(dt);
        else Move(dt);

        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg - 90f;
        coneT.localEulerAngles = new Vector3(0f, 0f, angle);
        body.transform.localEulerAngles = new Vector3(0f, 0f, angle);

        Scan();
    }

    // Le droide s'arrete et promene son faisceau autour de lui
    void Sweep(float dt)
    {
        float current = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        float next = Mathf.MoveTowardsAngle(current, sweepAngle, turnSpeed * 0.75f * dt);

        facing = new Vector2(Mathf.Cos(next * Mathf.Deg2Rad), Mathf.Sin(next * Mathf.Deg2Rad));

        if (Time.time >= sweepUntil) Decide();
    }

    void Move(float dt)
    {
        Vector2 here = transform.position;
        Vector2 toTarget = target - here;

        if (toTarget.sqrMagnitude < 0.2f)
        {
            Decide();
            return;
        }

        Vector2 dir = toTarget.normalized;
        facing = Vector3.RotateTowards(facing, dir, turnSpeed * Mathf.Deg2Rad * dt, 0f);

        // En flanant il ralentit, comme s'il inspectait la piece
        float speed = (mode == Mode.Wander) ? moveSpeed * 0.62f : moveSpeed;
        transform.position = here + dir * speed * dt;
    }

    void Scan()
    {
        if (SecurityDirector.Instance == null) return;

        Transform player = SecurityDirector.Instance.Player;
        if (player == null) return;

        bool visible = CanSee(player.position);
        cone.color = visible
            ? new Color(1f, 0.20f, 0.18f, 0.46f)
            : new Color(1f, 0.22f, 0.20f, 0.26f);

        if (visible) SecurityDirector.Instance.ReportSighting(this);
    }

    public bool CanSee(Vector3 point)
    {
        if (SecurityDirector.Instance != null && SecurityDirector.Instance.PlayerHidden) return false;

        Vector2 here = transform.position;
        Vector2 to = (Vector2)point - here;

        float distance = to.magnitude;
        if (distance > sightRange) return false;
        if (Vector2.Angle(facing, to) > sightHalfAngle) return false;

        RaycastHit2D[] hits = Physics2D.LinecastAll(here, point);

        foreach (RaycastHit2D h in hits)
        {
            if (h.collider == null || h.collider.isTrigger) continue;
            if (h.collider.transform.IsChildOf(transform)) continue;
            if (h.collider.GetComponent<PlayerMovement>() != null) continue;

            return false;
        }

        return true;
    }

    // ---------- Textures ----------

    static void EnsureSprites()
    {
        if (bodySprite == null) bodySprite = BuildBody(72);
        if (coneSprite == null) coneSprite = BuildCone(192);
    }

    static Sprite BuildBody(int s)
    {
        Color[] px = new Color[s * s];
        float c = s * 0.5f;
        float r = s * 0.38f;

        Color shell = new Color(0.52f, 0.56f, 0.63f);
        Color dark = new Color(0.16f, 0.18f, 0.22f);

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x - c, dy = y - c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                int i = y * s + x;

                if (d > r + 7f)
                {
                    px[i] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                if (d > r)
                {
                    bool pod = Mathf.Abs(dy) < 5.5f && Mathf.Abs(dx) > r - 2f && Mathf.Abs(dx) < r + 6.5f;
                    px[i] = pod ? new Color(dark.r * 1.8f, dark.g * 1.8f, dark.b * 2f, 1f)
                                : new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                float lift = Mathf.Clamp01(0.5f + dy / (2f * r));
                Color col = Color.Lerp(dark, shell, lift);

                if (r - d < 2.5f) col = dark;

                if (dy > r * 0.12f && Mathf.Abs(dx) < r * 0.74f && dy < r * 0.72f)
                    col = new Color(0.09f, 0.10f, 0.14f);

                float eye = Vector2.Distance(new Vector2(dx, dy), new Vector2(0f, r * 0.42f));
                if (eye < 4.2f)
                {
                    float k = 1f - eye / 4.2f;
                    col = Color.Lerp(new Color(0.7f, 0.1f, 0.1f), new Color(1f, 0.45f, 0.4f), k);
                }

                px[i] = new Color(col.r, col.g, col.b, 1f);
            }
        }

        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 45f);
    }

    static Sprite BuildCone(int s)
    {
        Color[] px = new Color[s * s];
        float ox = s * 0.5f;
        float half = 34f * Mathf.Deg2Rad;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = x - ox;
                float dy = y;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                int i = y * s + x;

                if (d < 1f || d > s)
                {
                    px[i] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                float a = Mathf.Atan2(dx, dy);

                if (Mathf.Abs(a) > half)
                {
                    px[i] = new Color(0f, 0f, 0f, 0f);
                    continue;
                }

                float fade = 1f - d / s;
                float side = 1f - Mathf.Abs(a) / half;
                float alpha = fade * fade * (0.45f + side * 0.55f);

                if (Mathf.Abs(a) > half - 0.035f) alpha = Mathf.Max(alpha, fade * 0.9f);

                px[i] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }
        }

        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0f), s / 7.5f);
    }
}
