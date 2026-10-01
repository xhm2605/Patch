using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Deux fleches rouges sur les bords de l'ecran, vers les pannes les plus proches.
public class RepairCompass : MonoBehaviour
{
    public static RepairCompass Instance;

    public int arrowCount = 5;
    public int highlighted = 2;
    public float edgeInset = 0.90f;

    private static Sprite arrowSprite;

    private readonly List<RectTransform> arrows = new List<RectTransform>();
    private readonly List<Image> icons = new List<Image>();
    private readonly List<TMP_Text> labels = new List<TMP_Text>();

    private RectTransform canvasRect;
    private Camera cam;
    private Transform player;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("RepairCompass");
        go.AddComponent<RepairCompass>();
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

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        arrows.Clear();
        icons.Clear();
        labels.Clear();

        canvasRect = null;
        cam = null;
        player = null;

        if (scene.name == "Main") Build();
    }

    void Build()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        canvasRect = canvas.transform as RectTransform;
        EnsureSprite();

        for (int i = 0; i < arrowCount; i++)
        {
            GameObject go = NewUI("RepairArrow" + i, canvas.transform);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(72f, 72f);

            Image img = go.AddComponent<Image>();
            img.sprite = arrowSprite;
            img.raycastTarget = false;

            // Le nom reste droit, seule la fleche tourne
            GameObject textGo = NewUI("Name", canvas.transform);
            RectTransform trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0.5f, 0.5f);
            trt.anchorMax = new Vector2(0.5f, 0.5f);
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.sizeDelta = new Vector2(190f, 30f);

            TextMeshProUGUI txt = textGo.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 22f;
            txt.characterSpacing = 5f;
            txt.fontStyle = FontStyles.Bold;
            txt.alignment = TextAlignmentOptions.Center;
            txt.raycastTarget = false;

            arrows.Add(rt);
            icons.Add(img);
            labels.Add(txt);

            go.SetActive(false);
            textGo.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (arrows.Count == 0 || canvasRect == null) return;

        if (cam == null) cam = Camera.main;

        if (player == null)
        {
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
        }

        if (cam == null || player == null || GameManager.Instance == null)
        {
            HideAll();
            return;
        }

        if (!GameManager.Instance.timerRunning || IntroCrawl.IsPlaying)
        {
            HideAll();
            return;
        }

        List<ArcadeTerminal> targets = NearestBroken();

        for (int i = 0; i < arrows.Count; i++)
        {
            if (i >= targets.Count)
            {
                arrows[i].gameObject.SetActive(false);
                labels[i].gameObject.SetActive(false);
                continue;
            }

            Place(i, targets[i], i < highlighted);
        }
    }

    void HideAll()
    {
        for (int i = 0; i < arrows.Count; i++)
        {
            arrows[i].gameObject.SetActive(false);
            labels[i].gameObject.SetActive(false);
        }
    }

    List<ArcadeTerminal> NearestBroken()
    {
        List<ArcadeTerminal> list = new List<ArcadeTerminal>();

        foreach (ArcadeTerminal t in FindObjectsByType<ArcadeTerminal>(FindObjectsInactive.Exclude))
        {
            if (GameManager.Instance.IsRepaired(t.terminalId)) continue;
            if (IsOnScreen(t.transform.position)) continue;

            list.Add(t);
        }

        list.Sort((a, b) =>
        {
            float da = ((Vector2)(a.transform.position - player.position)).sqrMagnitude;
            float db = ((Vector2)(b.transform.position - player.position)).sqrMagnitude;
            return da.CompareTo(db);
        });

        if (list.Count > arrowCount) list.RemoveRange(arrowCount, list.Count - arrowCount);
        return list;
    }

    bool IsOnScreen(Vector3 world)
    {
        Vector3 vp = cam.WorldToViewportPoint(world);
        return vp.z > 0f && vp.x > 0.07f && vp.x < 0.93f && vp.y > 0.07f && vp.y < 0.93f;
    }

    void Place(int index, ArcadeTerminal target, bool near)
    {
        Vector3 vp = cam.WorldToViewportPoint(target.transform.position);

        Vector2 dir = new Vector2(vp.x - 0.5f, vp.y - 0.5f);
        if (vp.z < 0f) dir = -dir;

        float span = Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
        if (span < 0.0001f) return;

        Vector2 edge = dir / span * 0.5f * edgeInset;

        float w = canvasRect.rect.width;
        float h = canvasRect.rect.height;
        Vector2 pos = new Vector2(edge.x * w, edge.y * h);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        float distance = Vector2.Distance(player.position, target.transform.position);

        // Les deux plus proches clignotent en grand, les autres restent en veille
        float beat = near
            ? 0.62f + 0.38f * Mathf.Sin(Time.time * Mathf.Lerp(5.5f, 2f, Mathf.Clamp01(distance / 60f)))
            : 0.34f;

        float size = near ? 72f : 46f;

        arrows[index].gameObject.SetActive(true);
        arrows[index].sizeDelta = new Vector2(size, size);
        arrows[index].anchoredPosition = pos;
        arrows[index].localEulerAngles = new Vector3(0f, 0f, angle);
        icons[index].color = new Color(1f, 0.30f, 0.26f, beat);

        if (!near)
        {
            labels[index].gameObject.SetActive(false);
            return;
        }

        labels[index].gameObject.SetActive(true);
        Vector2 inward = edge.sqrMagnitude > 0.0001f ? edge.normalized : Vector2.down;
        labels[index].rectTransform.anchoredPosition = pos - inward * 58f;
        labels[index].text = target.terminalId.ToUpper();
        labels[index].color = new Color(1f, 0.52f, 0.46f, Mathf.Min(1f, beat + 0.2f));
    }

    GameObject NewUI(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }

    static void EnsureSprite()
    {
        if (arrowSprite != null) return;

        int s = 64;
        Color[] px = new Color[s * s];
        float cx = s * 0.5f;

        for (int y = 0; y < s; y++)
        {
            for (int x = 0; x < s; x++)
            {
                float dx = Mathf.Abs(x - cx);
                float a = 0f;

                // Pointe en haut, base large en bas
                float limit = (46f - y) * 0.55f;
                if (y >= 22 && y <= 46 && dx <= limit) a = 1f;

                // Hampe
                if (y > 8 && y < 26 && dx < 6.5f) a = 1f;

                if (a > 0f)
                {
                    float shade = 0.75f + 0.25f * (1f - dx / Mathf.Max(1f, limit));
                    px[y * s + x] = new Color(shade, shade, shade, 1f);
                }
                else
                {
                    px[y * s + x] = new Color(0f, 0f, 0f, 0f);
                }
            }
        }

        Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.SetPixels(px);
        tex.Apply();

        arrowSprite = Sprite.Create(tex, new Rect(0f, 0f, s, s), new Vector2(0.5f, 0.5f), 64f);
    }
}
