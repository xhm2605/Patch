using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArcadeTerminal : MonoBehaviour
{
    [Header("Configuration")]
    public string terminalId = "Engine";
    public string sceneToLoad = "Pacman";
    public Button repairButton;

    [Header("Sprites")]
    public Sprite brokenSprite;
    public Sprite fixedSprite;

    [Header("Etat")]
    public bool hintModeAvailable = false;
    public float interactRange = 3.4f;

    private bool isRepaired = false;
    private SpriteRenderer sr;
    private TMP_Text buttonLabel;

    // Les cinq bornes partagent le meme bouton : une seule le tient a la fois
    private static ArcadeTerminal holder;
    private static Transform player;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        AddSolidBody();

        ArcadeTerminal[] all = FindObjectsByType<ArcadeTerminal>(FindObjectsInactive.Exclude);
        foreach (ArcadeTerminal t in all)
        {
            if (t != this && t.terminalId == terminalId)
                Debug.LogWarning("Deux bornes ont le meme Terminal Id : " + terminalId);
        }

        if (repairButton != null)
        {
            buttonLabel = repairButton.GetComponentInChildren<TMP_Text>(true);
            repairButton.gameObject.SetActive(false);
        }

        if (GameManager.Instance != null && GameManager.Instance.IsRepaired(terminalId))
        {
            isRepaired = true;
            if (fixedSprite != null && sr != null) sr.sprite = fixedSprite;
        }
        else if (brokenSprite != null && sr != null)
        {
            sr.sprite = brokenSprite;
        }
    }

    void OnDisable()
    {
        if (holder == this) holder = null;
    }

    // La borne est un meuble : seule sa base bloque le passage
    void AddSolidBody()
    {
        foreach (Collider2D c in GetComponents<Collider2D>())
            if (!c.isTrigger) return;

        BoxCollider2D body = gameObject.AddComponent<BoxCollider2D>();

        if (sr != null && sr.sprite != null)
        {
            Bounds b = sr.sprite.bounds;
            body.size = new Vector2(b.size.x * 0.80f, b.size.y * 0.24f);
            body.offset = new Vector2(b.center.x, b.min.y + b.size.y * 0.12f);
        }
        else
        {
            body.size = new Vector2(1.6f, 1.2f);
        }

        body.isTrigger = false;
    }

    static Transform Player()
    {
        if (player == null)
        {
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
        }

        return player;
    }

    private bool IsInteractable()
    {
        return !isRepaired || hintModeAvailable;
    }

    // La portee est mesuree a chaque image : un trigger se perd quand le joueur
    // est teleporte par une trappe ou au retour d'un mini-jeu.
    void Update()
    {
        if (repairButton == null || GameManager.Instance == null) return;

        Transform p = Player();

        if (p == null || !IsInteractable())
        {
            Release();
            return;
        }

        float distance = Vector2.Distance(p.position, transform.position);

        if (distance > interactRange)
        {
            Release();
            return;
        }

        // Si une autre borne est plus proche, on lui laisse le bouton
        if (holder != null && holder != this)
        {
            float other = Vector2.Distance(p.position, holder.transform.position);
            if (other <= distance) return;
        }

        if (holder != this)
        {
            holder = this;
            repairButton.onClick.RemoveAllListeners();
            repairButton.onClick.AddListener(StartMinigame);
            repairButton.gameObject.SetActive(true);
        }

        RefreshButton();

        if (Input.GetKeyDown(KeyCode.Space)) StartMinigame();
    }

    void Release()
    {
        if (holder != this) return;

        holder = null;

        if (repairButton == null) return;
        repairButton.onClick.RemoveAllListeners();
        repairButton.gameObject.SetActive(false);
    }

    void RefreshButton()
    {
        if (repairButton == null || GameManager.Instance == null) return;

        bool locked = GameManager.Instance.IsLocked(terminalId);
        repairButton.interactable = !locked;

        if (buttonLabel == null) return;

        if (locked)
        {
            int s = Mathf.CeilToInt(GameManager.Instance.GetLockRemaining(terminalId));
            buttonLabel.text = "RESETTING " + s + "s";
        }
        else
        {
            buttonLabel.text = (isRepaired ? "HINT" : "REPAIR") + "   [SPACE]";
        }
    }

    void StartMinigame()
    {
        if (holder != this || !IsInteractable()) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.IsLocked(terminalId)) return;

        Release();

        string frag = GameManager.Instance.GetFragmentFor(terminalId);
        GameManager.Instance.LaunchMinigame(terminalId, frag, sceneToLoad, isRepaired);
    }
}
