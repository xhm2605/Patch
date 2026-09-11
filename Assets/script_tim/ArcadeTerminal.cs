using UnityEngine;
using UnityEngine.UI;

public class ArcadeTerminal : MonoBehaviour
{
    [Header("Configuration")]
    public string terminalId = "Engine";     // identifiant unique de la panne
    public string fragment = "ST";           // les 2 lettres données par cette panne
    public string sceneToLoad = "Pacman";
    public Button repairButton;

    [Header("Etat")]
    public bool hintModeAvailable = false;   // activé plus tard par l'écran du code final

    private bool playerInRange = false;
    private bool isRepaired = false;
    [Header("Sprites")]
    public Sprite brokenSprite;
    public Sprite fixedSprite;

    void Start()
    {
        if (repairButton != null)
            repairButton.gameObject.SetActive(false);

        if (brokenSprite != null)
        GetComponent<SpriteRenderer>().sprite = brokenSprite;

        if (GameManager.Instance != null && GameManager.Instance.IsRepaired(terminalId))
        {
            isRepaired = true;
            if (fixedSprite != null)
                GetComponent<SpriteRenderer>().sprite = fixedSprite;
        }
    }

    // La borne reste utilisable si elle est réparée ET que le joueur cherche un indice
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
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        repairButton.onClick.RemoveAllListeners();
        repairButton.gameObject.SetActive(false);
    }

    void StartMinigame()
    {
        if (!playerInRange || !IsInteractable()) return;

        repairButton.gameObject.SetActive(false);

        // Si la panne est déjà réparée, c'est une partie d'indice en mode difficile
        bool hardMode = isRepaired;

        GameManager.Instance.LaunchMinigame(terminalId, fragment, sceneToLoad, hardMode);
    }
}