using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipExpansion : MonoBehaviour
{
    public static ShipExpansion Instance;

    private const string SnakeKeyword = "snake";
    private const string SnakeFallback = "Snake";
    private const float Thick = 0.5f;

    public const int PlayerOrder = 500;
    public const int HideOrder = 600;

    public static Vector2 CoolantTerminal = new Vector2(31f, 25.5f);

    // Sols ajoutes au vaisseau : ShipDressing les reprend pour poser plaques et coque
    public static readonly Rect[] Zones =
    {
        Rect.MinMaxRect(18f, 16f, 34f, 28f),      // Coolant
        Rect.MinMaxRect(-36f, 16f, -18f, 26f),    // Cargo, corps
        Rect.MinMaxRect(-36f, 26f, -30f, 34f),    // Cargo, bras nord
        Rect.MinMaxRect(-36f, -26f, -18f, -16f),  // Quarters, corps
        Rect.MinMaxRect(-30f, -34f, -24f, -26f),  // Quarters, alcove sud
        Rect.MinMaxRect(18f, -26f, 36f, -16f),    // Observation, corps
        Rect.MinMaxRect(26f, -34f, 36f, -26f),    // Observation, coupole sud

        Rect.MinMaxRect(24f, 6f, 28f, 16f),       // Oxygen vers Coolant
        Rect.MinMaxRect(8f, 20f, 18f, 24f),       // Arcade vers Coolant
        Rect.MinMaxRect(-18f, 20f, -8f, 24f),     // Arcade vers Cargo
        Rect.MinMaxRect(-28f, 6f, -24f, 16f),     // Shield vers Cargo
        Rect.MinMaxRect(-28f, -16f, -24f, -6f),   // Shield vers Quarters
        Rect.MinMaxRect(-18f, -24f, -8f, -20f),   // Engine vers Quarters
        Rect.MinMaxRect(8f, -24f, 18f, -20f),     // Engine vers Observation
        Rect.MinMaxRect(24f, -16f, 28f, -6f)      // Oxygen vers Observation
    };

    private Transform root;
    private GameObject wallModel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("ShipExpansion");
        go.AddComponent<ShipExpansion>();
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

    void Build()
    {
        if (GameObject.Find("ShipExpansionRoot") != null) return;

        wallModel = FindWallModel();
        if (wallModel == null)
        {
            Debug.LogWarning("ShipExpansion : aucun mur modele trouve, le vaisseau n'est pas agrandi");
            return;
        }

        root = new GameObject("ShipExpansionRoot").transform;

        OpenExistingRooms();
        BuildCoolant();
        BuildCargo();
        BuildQuarters();
        BuildObservation();
        BuildCorridors();
        SpawnTerminal();
        ArrangeTerminals();
        SortPlayer();
        BuildVents();
    }

    // ---------- Ouverture des salles d'origine ----------

    void OpenExistingRooms()
    {
        // Plafond et plancher d'Oxygen
        Pierce(new Vector2(26f, 6f));
        WallH(18f, 24f, 6f);
        WallH(28f, 34f, 6f);

        Pierce(new Vector2(26f, -6f));
        WallH(18f, 24f, -6f);
        WallH(28f, 34f, -6f);

        // Plafond et plancher de Shield
        Pierce(new Vector2(-26f, 6f));
        WallH(-34f, -28f, 6f);
        WallH(-24f, -18f, 6f);

        Pierce(new Vector2(-26f, -6f));
        WallH(-34f, -28f, -6f);
        WallH(-24f, -18f, -6f);

        // Flancs d'Arcade bay
        Pierce(new Vector2(8f, 22f));
        WallV(16f, 20f, 8f);
        WallV(24f, 28f, 8f);

        Pierce(new Vector2(-8f, 22f));
        WallV(16f, 20f, -8f);
        WallV(24f, 28f, -8f);

        // Flancs d'Engine room
        Pierce(new Vector2(8f, -22f));
        WallV(-28f, -24f, 8f);
        WallV(-20f, -16f, 8f);

        Pierce(new Vector2(-8f, -22f));
        WallV(-28f, -24f, -8f);
        WallV(-20f, -16f, -8f);
    }

    // ---------- Salles ----------

    void BuildCoolant()
    {
        WallH(18f, 34f, 28f);
        WallH(18f, 24f, 16f);
        WallH(28f, 34f, 16f);
        WallV(16f, 20f, 18f);
        WallV(24f, 28f, 18f);
        WallV(16f, 28f, 34f);
    }

    // Soute en L : le corps s'etend vers l'ouest, un bras remonte au nord
    void BuildCargo()
    {
        WallH(-36f, -28f, 16f);
        WallH(-24f, -18f, 16f);
        WallV(16f, 20f, -18f);
        WallV(24f, 26f, -18f);
        WallH(-30f, -18f, 26f);
        WallV(26f, 34f, -30f);
        WallH(-36f, -30f, 34f);
        WallV(16f, 34f, -36f);
    }

    // Quartiers avec une alcove qui descend au sud
    void BuildQuarters()
    {
        WallH(-36f, -28f, -16f);
        WallH(-24f, -18f, -16f);
        WallV(-26f, -24f, -18f);
        WallV(-20f, -16f, -18f);
        WallH(-36f, -30f, -26f);
        WallH(-24f, -18f, -26f);
        WallV(-34f, -26f, -30f);
        WallH(-30f, -24f, -34f);
        WallV(-34f, -26f, -24f);
        WallV(-26f, -16f, -36f);
    }

    // Poste d'observation : corps allonge et coupole decalee vers le sud-est
    void BuildObservation()
    {
        WallH(18f, 24f, -16f);
        WallH(28f, 36f, -16f);
        WallV(-26f, -24f, 18f);
        WallV(-20f, -16f, 18f);
        WallH(18f, 26f, -26f);
        WallV(-34f, -26f, 26f);
        WallH(26f, 36f, -34f);
        WallV(-34f, -16f, 36f);
    }

    void BuildCorridors()
    {
        WallV(6f, 16f, 24f);
        WallV(6f, 16f, 28f);

        WallH(8f, 18f, 20f);
        WallH(8f, 18f, 24f);

        WallH(-18f, -8f, 20f);
        WallH(-18f, -8f, 24f);

        WallV(6f, 16f, -28f);
        WallV(6f, 16f, -24f);

        WallV(-16f, -6f, -28f);
        WallV(-16f, -6f, -24f);

        WallH(-18f, -8f, -24f);
        WallH(-18f, -8f, -20f);

        WallH(8f, 18f, -24f);
        WallH(8f, 18f, -20f);

        WallV(-16f, -6f, 24f);
        WallV(-16f, -6f, 28f);
    }

    // Le joueur reste au premier plan : seules les piles de carton le masquent
    void SortPlayer()
    {
        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player == null) return;

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = PlayerOrder;
    }

    // Chaque borne est adossee au mur le plus eloigne des entrees de sa salle
    void ArrangeTerminals()
    {
        Dictionary<string, Vector2> spots = new Dictionary<string, Vector2>
        {
            { "Comms", new Vector2(0f, 25.5f) },
            { "Engine", new Vector2(5.5f, -18f) },
            { "Shield", new Vector2(-31.5f, 4f) },
            { "Oxygen", new Vector2(31.5f, 4f) },
            { "Coolant", CoolantTerminal }
        };

        foreach (ArcadeTerminal t in FindObjectsByType<ArcadeTerminal>(FindObjectsInactive.Include))
        {
            if (!spots.ContainsKey(t.terminalId)) continue;

            Vector2 p = spots[t.terminalId];
            t.transform.position = new Vector3(p.x, p.y, t.transform.position.z);
            t.transform.localRotation = Quaternion.identity;
            t.transform.localScale = Vector3.one;

            Sprite off = TerminalSkins.Broken(t.terminalId);
            Sprite lit = TerminalSkins.Repaired(t.terminalId);

            if (off != null) t.brokenSprite = off;
            if (lit != null) t.fixedSprite = lit;
        }
    }

    // ---------- Conduits d'aeration ----------

    void BuildVents()
    {
        Link(Vent(new Vector2(-33f, 31f), "soute"), Vent(new Vector2(33f, -31f), "observation"));
        Link(Vent(new Vector2(20.5f, 18f), "refroidissement"), Vent(new Vector2(-27f, -31f), "quartiers"));
        Link(Vent(new Vector2(-6f, 26f), "salle d'arcade"), Vent(new Vector2(6f, -26f), "moteurs"));
    }

    VentShaft Vent(Vector2 pos, string label)
    {
        GameObject go = new GameObject("VentShaft");
        go.transform.SetParent(root, false);
        go.transform.position = new Vector3(pos.x, pos.y, 0f);

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.9f;

        VentShaft v = go.AddComponent<VentShaft>();
        v.destinationName = label;
        return v;
    }

    void Link(VentShaft a, VentShaft b)
    {
        a.linked = b;
        b.linked = a;
    }

    // ---------- Outils de construction ----------

    void WallH(float x1, float x2, float y)
    {
        Spawn(new Vector2((x1 + x2) * 0.5f, y), new Vector2(Mathf.Abs(x2 - x1) + Thick, Thick));
    }

    void WallV(float y1, float y2, float x)
    {
        Spawn(new Vector2(x, (y1 + y2) * 0.5f), new Vector2(Thick, Mathf.Abs(y2 - y1) + Thick));
    }

    void Spawn(Vector2 pos, Vector2 size)
    {
        GameObject go = Instantiate(wallModel, root);
        go.name = "ExpansionWall";
        go.SetActive(true);

        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        if (go.GetComponent<BoxCollider2D>() == null)
            go.AddComponent<BoxCollider2D>();
    }

    // Retire le mur d'origine le plus proche du point donne
    void Pierce(Vector2 wallCenter)
    {
        GameObject walls = GameObject.Find("Walls");
        if (walls == null) return;

        Transform target = null;
        float best = 0.8f;

        foreach (SpriteRenderer sr in walls.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.GetComponent<ArcadeTerminal>() != null) continue;

            float d = Vector2.Distance(sr.transform.position, wallCenter);
            if (d < best)
            {
                best = d;
                target = sr.transform;
            }
        }

        if (target != null) target.gameObject.SetActive(false);
    }

    GameObject FindWallModel()
    {
        GameObject walls = GameObject.Find("Walls");
        if (walls == null) return null;

        foreach (SpriteRenderer sr in walls.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.GetComponent<BoxCollider2D>() == null) continue;
            if (sr.GetComponent<ArcadeTerminal>() != null) continue;
            if (sr.name.StartsWith("Corridor")) return sr.gameObject;
        }

        foreach (SpriteRenderer sr in walls.GetComponentsInChildren<SpriteRenderer>(true))
            if (sr.GetComponent<BoxCollider2D>() != null) return sr.gameObject;

        return null;
    }

    // ---------- Borne du refroidissement ----------

    void SpawnTerminal()
    {
        ArcadeTerminal model = null;

        foreach (ArcadeTerminal t in FindObjectsByType<ArcadeTerminal>(FindObjectsInactive.Include))
        {
            if (t.terminalId == "Oxygen") { model = t; break; }
            if (model == null) model = t;
        }

        if (model == null)
        {
            Debug.LogWarning("ShipExpansion : aucune borne modele, le terminal Coolant n'est pas cree");
            return;
        }

        GameObject go = Instantiate(model.gameObject, root);
        go.name = "Terminal_Coolant";
        go.transform.position = new Vector3(CoolantTerminal.x, CoolantTerminal.y, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = model.transform.localScale;
        go.SetActive(true);

        ArcadeTerminal term = go.GetComponent<ArcadeTerminal>();
        term.terminalId = "Coolant";
        term.sceneToLoad = FindSceneNamed(SnakeKeyword, SnakeFallback);
        term.hintModeAvailable = model.hintModeAvailable;

        Debug.Log("Borne Coolant creee, elle lance la scene : " + term.sceneToLoad);
    }

    // Retrouve la scene du serpent dans les Build Settings, quel que soit son nom exact
    public static string FindSceneNamed(string keyword, string fallback)
    {
        int count = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(path);

            if (sceneName.ToLower().Contains(keyword)) return sceneName;
        }

        Debug.LogWarning("ShipExpansion : aucune scene contenant \"" + keyword +
                         "\" dans les Build Settings, on tentera \"" + fallback + "\"");
        return fallback;
    }
}
