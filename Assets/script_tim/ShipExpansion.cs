using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipExpansion : MonoBehaviour
{
    public static ShipExpansion Instance;

    // Salle du circuit de refroidissement, au nord-est
    public static Rect CoolantRoom = Rect.MinMaxRect(18f, 16f, 34f, 28f);
    public static Rect CoolantCorridor = Rect.MinMaxRect(24f, 6f, 28f, 16f);
    public static Vector2 CoolantTerminal = new Vector2(26f, 25f);

    private const string SnakeKeyword = "snake";
    private const string SnakeFallback = "Snake";

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

        GameObject model = FindWallModel();
        if (model == null)
        {
            Debug.LogWarning("ShipExpansion : aucun mur modele trouve, la salle Coolant n'est pas construite");
            return;
        }

        Transform root = new GameObject("ShipExpansionRoot").transform;

        OpenOxygenCeiling(model, root);

        Wall(model, root, new Vector2(26f, 28f), new Vector2(16.5f, 0.5f));
        Wall(model, root, new Vector2(18f, 22f), new Vector2(0.5f, 12.5f));
        Wall(model, root, new Vector2(34f, 22f), new Vector2(0.5f, 12.5f));
        Wall(model, root, new Vector2(21f, 16f), new Vector2(6.5f, 0.5f));
        Wall(model, root, new Vector2(31f, 16f), new Vector2(6.5f, 0.5f));

        Wall(model, root, new Vector2(24f, 11f), new Vector2(0.5f, 10f));
        Wall(model, root, new Vector2(28f, 11f), new Vector2(0.5f, 10f));

        SpawnTerminal(root);
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

    GameObject Wall(GameObject model, Transform parent, Vector2 pos, Vector2 size)
    {
        GameObject go = Instantiate(model, parent);
        go.name = "ExpansionWall";
        go.SetActive(true);

        go.transform.position = new Vector3(pos.x, pos.y, 0f);
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);

        if (go.GetComponent<BoxCollider2D>() == null)
            go.AddComponent<BoxCollider2D>();

        return go;
    }

    // Le plafond d'Oxygen barre le nouveau couloir : on le remplace par deux tronçons
    void OpenOxygenCeiling(GameObject model, Transform root)
    {
        GameObject walls = GameObject.Find("Walls");
        if (walls == null) return;

        Transform target = null;
        float best = 0.8f;

        foreach (SpriteRenderer sr in walls.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (sr.GetComponent<ArcadeTerminal>() != null) continue;

            float d = Vector2.Distance(sr.transform.position, new Vector2(26f, 6f));
            if (d < best)
            {
                best = d;
                target = sr.transform;
            }
        }

        if (target != null) target.gameObject.SetActive(false);

        Wall(model, root, new Vector2(21f, 6f), new Vector2(6.5f, 0.5f));
        Wall(model, root, new Vector2(31f, 6f), new Vector2(6.5f, 0.5f));
    }

    void SpawnTerminal(Transform root)
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
