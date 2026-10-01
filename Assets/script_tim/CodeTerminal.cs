using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeTerminal : MonoBehaviour
{
    [Header("References UI")]
    public Button CodeButton;
    public GameObject CodePanel;
    public TMP_Text FeedbackText;

    private bool playerInRange = false;

    void Start()
    {
        AddSolidBody();

        if (CodeButton != null) CodeButton.gameObject.SetActive(false);
        if (CodePanel != null) CodePanel.SetActive(false);
    }

    // La console est un meuble : on ne la traverse pas
    void AddSolidBody()
    {
        foreach (Collider2D c in GetComponents<Collider2D>())
            if (!c.isTrigger) return;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        BoxCollider2D body = gameObject.AddComponent<BoxCollider2D>();

        if (sr != null && sr.sprite != null)
        {
            Bounds b = sr.sprite.bounds;
            body.size = new Vector2(b.size.x * 0.88f, b.size.y * 0.80f);
            body.offset = new Vector2(b.center.x, b.center.y);
        }
        else
        {
            body.size = new Vector2(3f, 1.4f);
        }

        body.isTrigger = false;
    }

    // Si on referme la console en restant devant, le bouton d'acces revient
    void Update()
    {
        if (!playerInRange || CodeButton == null || CodePanel == null) return;

        bool shouldShow = !CodePanel.activeSelf;
        if (CodeButton.gameObject.activeSelf != shouldShow)
            CodeButton.gameObject.SetActive(shouldShow);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (CodeButton == null) return;

        playerInRange = true;
        CodeButton.gameObject.SetActive(true);
        CodeButton.onClick.RemoveAllListeners();
        CodeButton.onClick.AddListener(OpenCodePanel);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || !other.CompareTag("Player")) return;

        playerInRange = false;
        if (CodeButton == null) return;

        CodeButton.onClick.RemoveAllListeners();
        CodeButton.gameObject.SetActive(false);
    }

    // La console s'ouvre quand on veut : elle n'affiche que les lettres deja trouvees
    void OpenCodePanel()
    {
        if (!playerInRange || CodePanel == null) return;

        if (FeedbackText != null && GameManager.Instance != null)
        {
            int done = GameManager.Instance.repairedTerminals.Count;
            int total = GameManager.Instance.TerminalCount();
            FeedbackText.text = done < total ? "SYSTEMS REPAIRED : " + done + "/" + total : "";
        }

        SoundManager.PlayOpen();
        CodeButton.gameObject.SetActive(false);
        CodePanel.SetActive(true);
    }
}
