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

    private bool playerInRange = false;
    private bool isRepaired = false;
    private SpriteRenderer sr;
    private TMP_Text buttonLabel;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        ArcadeTerminal[] all = FindObjectsByType<ArcadeTerminal>(
            FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (ArcadeTerminal t in all)
        {
            if (t != this && t.terminalId == terminalId)
                Debug.LogWarning("Deux bornes ont le meme Terminal Id : " + terminalId);
        }

        if (repairButton != null)
        {
            buttonLabel = repairButton.GetComponentInChildren<TMP_Text>();
            repairButton.gameObject.SetActive(false);
        }

        if (GameManager.Instance != null && GameManager.Instance.IsRepaired(terminalId))
        {
            isRepaired = true;
            if (fixedSprite != null) sr.sprite = fixedSprite;
        }
        else if (brokenSprite != null)
        {
            sr.sprite = brokenSprite;
        }
    }

    private bool IsInteractable()
    {
        return !isRepaired || hintModeAvailable;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || !IsInteractable()) return;

        playerInRange = true;
        repairButton.gameObject.SetActive(true);
        repairButton.onClick.RemoveAllListeners();
        repairButton.onClick.AddListener(StartMinigame);
        RefreshButton();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        repairButton.onClick.RemoveAllListeners();
        repairButton.gameObject.SetActive(false);
    }

    void Update()
    {
        if (playerInRange) RefreshButton();
    }

    void RefreshButton()
    {
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
            buttonLabel.text = isRepaired ? "HINT" : "REPAIR";
        }
    }

    void StartMinigame()
    {
        if (!playerInRange || !IsInteractable()) return;
        if (GameManager.Instance.IsLocked(terminalId)) return;

        repairButton.gameObject.SetActive(false);

        string frag = GameManager.Instance.GetFragmentFor(terminalId);
        GameManager.Instance.LaunchMinigame(terminalId, frag, sceneToLoad, isRepaired);
    }
}
