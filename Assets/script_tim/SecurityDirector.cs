using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Pilote les droides : combien, a quelle vitesse, et ce qui arrive au joueur.
public class SecurityDirector : MonoBehaviour
{
    public static SecurityDirector Instance;

    public int maxLives = 3;

    private const float FrozenTime = 2.8f;
    private const float SafeTime = 3.6f;

    private readonly List<SecurityDroid> droids = new List<SecurityDroid>();

    private Transform player;
    private PlayerMovement movement;

    private int lives;
    private float frozenUntil;
    private float safeUntil;
    private float nextSpawn;

    private bool inMainScene = false;

    private int maxDroids;
    private float spawnEvery;
    private float droidSpeed;

    private readonly List<Image> pips = new List<Image>();
    private TMP_Text warning;
    private Image flash;

    public Transform Player { get { return player; } }

    public bool PlayerHidden
    {
        get
        {
            if (player == null) return false;

            foreach (Vector2 spot in ShipDressing.HidePositions)
                if (Vector2.Distance(player.position, spot) < ShipDressing.HideRadius) return true;

            return false;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("SecurityDirector");
        go.AddComponent<SecurityDirector>();
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

    // La scene de depart n'emet pas sceneLoaded : on l'attrape ici.
    void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        droids.Clear();
        pips.Clear();

        player = null;
        movement = null;
        warning = null;
        flash = null;

        inMainScene = (scene.name == "Main");
        if (!inMainScene) return;

        ApplyDifficulty();
        BuildUI();

        lives = maxLives;
        frozenUntil = 0f;
        safeUntil = 0f;
        nextSpawn = spawnEvery;
    }

    void ApplyDifficulty()
    {
        if (GameSettings.difficulty == 0)
        {
            droidSpeed = 2.3f;
            maxDroids = 2;
            spawnEvery = 180f;
        }
        else if (GameSettings.difficulty == 2)
        {
            droidSpeed = 3.5f;
            maxDroids = 4;
            spawnEvery = 110f;
        }
        else
        {
            droidSpeed = 2.9f;
            maxDroids = 3;
            spawnEvery = 140f;
        }
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (!inMainScene)
        {
            if (droids.Count > 0) ClearDroids();
            return;
        }

        bool running = GameManager.Instance.timerRunning && !IntroCrawl.IsPlaying;

        if (!running)
        {
            if (droids.Count > 0) ClearDroids();
            ShowUI(false);
            return;
        }

        ShowUI(true);

        if (player == null)
        {
            movement = FindAnyObjectByType<PlayerMovement>();
            if (movement != null) player = movement.transform;
        }

        // Premier droide des que la partie demarre, puis renfort regulier
        if (droids.Count == 0) SpawnDroid();

        float elapsed = GameManager.Instance.totalTime - GameManager.Instance.GetTimeLeft();

        if (droids.Count < maxDroids && elapsed > nextSpawn)
        {
            SpawnDroid();
            nextSpawn += spawnEvery;
        }

        UpdatePlayerState();
        UpdateUI();
    }

    void SpawnDroid()
    {
        if (GameSettings.difficulty == 2 && droids.Count == 0)
        {
            Create();
            Create();
            return;
        }

        Create();
    }

    void Create()
    {
        GameObject go = new GameObject("SecurityDroid");
        SecurityDroid d = go.AddComponent<SecurityDroid>();

        int node = FarNode();
        d.Spawn(node, droidSpeed);

        droids.Add(d);
    }

    // On fait apparaitre le droide loin du joueur, jamais dans son dos immediat
    int FarNode()
    {
        if (player == null) return Random.Range(0, SecurityDroid.NodeCount);

        int best = 0;
        float bestDistance = -1f;

        for (int i = 0; i < SecurityDroid.NodeCount; i++)
        {
            float d = Vector2.Distance(SecurityDroid.NodeAt(i), player.position);
            float jitter = Random.Range(0f, 8f);

            if (d + jitter > bestDistance)
            {
                bestDistance = d + jitter;
                best = i;
            }
        }

        return best;
    }

    void ClearDroids()
    {
        foreach (SecurityDroid d in droids)
            if (d != null) Destroy(d.gameObject);

        droids.Clear();
    }

    // ---------- Capture ----------

    public void ReportSighting(SecurityDroid droid)
    {
        if (Time.time < safeUntil) return;
        if (Time.time < frozenUntil) return;

        Caught();
    }

    void Caught()
    {
        lives--;

        frozenUntil = Time.time + FrozenTime;
        safeUntil = Time.time + FrozenTime + SafeTime;

        SoundManager.PlayFail();

        if (lives <= 0)
        {
            lives = 0;
            if (movement != null) movement.enabled = true;
            GameManager.Instance.GameOver("CAUGHT BY SECURITY");
        }
    }

    void UpdatePlayerState()
    {
        if (movement == null) return;

        bool frozen = Time.time < frozenUntil;
        if (movement.enabled == frozen) movement.enabled = !frozen;

        SpriteRenderer sr = movement.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        if (frozen)
        {
            sr.color = new Color(1f, 0.45f, 0.42f);
        }
        else if (Time.time < safeUntil)
        {
            // Clignotement d'invulnerabilite
            float k = Mathf.PingPong(Time.time * 7f, 1f);
            sr.color = new Color(1f, 1f, 1f, 0.35f + k * 0.65f);
        }
        else
        {
            sr.color = Color.white;
        }
    }

    // ---------- Affichage ----------

    void BuildUI()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject root = NewUI("SecurityHud", canvas.transform);
        RectTransform rt = root.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.anchoredPosition = new Vector2(34f, 34f);
        rt.sizeDelta = new Vector2(260f, 52f);

        GameObject title = NewUI("HullLabel", root.transform);
        RectTransform trt = title.GetComponent<RectTransform>();
        trt.anchorMin = new Vector2(0f, 0.5f);
        trt.anchorMax = new Vector2(0f, 0.5f);
        trt.pivot = new Vector2(0f, 0.5f);
        trt.anchoredPosition = new Vector2(0f, 0f);
        trt.sizeDelta = new Vector2(112f, 40f);

        TextMeshProUGUI txt = title.AddComponent<TextMeshProUGUI>();
        txt.text = "ALERT";
        txt.fontSize = 26f;
        txt.characterSpacing = 6f;
        txt.fontStyle = FontStyles.Bold;
        txt.color = new Color(0.72f, 0.80f, 0.92f);
        txt.alignment = TextAlignmentOptions.Left;
        txt.raycastTarget = false;

        for (int i = 0; i < maxLives; i++)
        {
            GameObject pip = NewUI("Pip" + i, root.transform);
            RectTransform prt = pip.GetComponent<RectTransform>();
            prt.anchorMin = new Vector2(0f, 0.5f);
            prt.anchorMax = new Vector2(0f, 0.5f);
            prt.pivot = new Vector2(0f, 0.5f);
            prt.anchoredPosition = new Vector2(124f + i * 38f, 0f);
            prt.sizeDelta = new Vector2(30f, 30f);

            Image img = pip.AddComponent<Image>();
            img.raycastTarget = false;
            pips.Add(img);
        }

        GameObject warn = NewUI("SecurityWarning", canvas.transform);
        RectTransform wrt = warn.GetComponent<RectTransform>();
        wrt.anchorMin = new Vector2(0.5f, 0.5f);
        wrt.anchorMax = new Vector2(0.5f, 0.5f);
        wrt.pivot = new Vector2(0.5f, 0.5f);
        wrt.anchoredPosition = new Vector2(0f, 150f);
        wrt.sizeDelta = new Vector2(900f, 60f);

        warning = warn.AddComponent<TextMeshProUGUI>();
        warning.fontSize = 44f;
        warning.characterSpacing = 10f;
        warning.fontStyle = FontStyles.Bold;
        warning.color = new Color(1f, 0.30f, 0.26f);
        warning.alignment = TextAlignmentOptions.Center;
        warning.raycastTarget = false;
        warning.text = "";

        GameObject flashGo = NewUI("SecurityFlash", canvas.transform);
        RectTransform frt = flashGo.GetComponent<RectTransform>();
        frt.anchorMin = Vector2.zero;
        frt.anchorMax = Vector2.one;
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;

        flash = flashGo.AddComponent<Image>();
        flash.color = new Color(1f, 0f, 0f, 0f);
        flash.raycastTarget = false;

        flashGo.transform.SetAsFirstSibling();
    }

    void ShowUI(bool show)
    {
        foreach (Image pip in pips)
            if (pip != null && pip.transform.parent != null)
                pip.transform.parent.gameObject.SetActive(show);

        if (warning != null) warning.gameObject.SetActive(show);
        if (flash != null) flash.gameObject.SetActive(show);
    }

    void UpdateUI()
    {
        for (int i = 0; i < pips.Count; i++)
        {
            if (pips[i] == null) continue;

            bool alive = i < lives;
            pips[i].color = alive
                ? new Color(0.42f, 0.92f, 0.56f, 0.95f)
                : new Color(0.45f, 0.18f, 0.18f, 0.55f);
        }

        if (warning != null)
        {
            if (Time.time < frozenUntil)
            {
                warning.text = "SPOTTED";
                warning.color = new Color(1f, 0.30f, 0.26f,
                    0.55f + 0.45f * Mathf.PingPong(Time.time * 6f, 1f));
            }
            else if (PlayerHidden)
            {
                warning.text = "HIDDEN";
                warning.color = new Color(0.45f, 0.92f, 0.60f, 0.8f);
            }
            else
            {
                warning.text = "";
            }
        }

        if (flash != null)
        {
            float a = Time.time < frozenUntil
                ? 0.16f + 0.14f * Mathf.PingPong(Time.time * 6f, 1f)
                : 0f;

            flash.color = new Color(1f, 0.1f, 0.1f, a);
        }
    }

    GameObject NewUI(string objectName, Transform parent)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        go.transform.SetParent(parent, false);
        return go;
    }
}
