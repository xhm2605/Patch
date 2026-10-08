using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeTerminal : MonoBehaviour
{
    [Header("References UI")]
    public Button CodeButton;
    public GameObject CodePanel;
    public TMP_Text FeedbackText;

    public float interactRange = 4f;

    private Transform player;
    private bool wired = false;

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

    // Portee mesuree a chaque image, pour survivre aux teleportations
    void Update()
    {
        if (CodeButton == null || CodePanel == null) return;

        if (player == null)
        {
            PlayerMovement pm = FindAnyObjectByType<PlayerMovement>();
            if (pm != null) player = pm.transform;
        }

        bool near = player != null
                    && Vector2.Distance(player.position, transform.position) <= interactRange;

        bool show = near && !CodePanel.activeSelf;

        if (show && !wired)
        {
            wired = true;
            CodeButton.onClick.RemoveAllListeners();
            CodeButton.onClick.AddListener(OpenCodePanel);
        }
        else if (!show && wired)
        {
            wired = false;
            CodeButton.onClick.RemoveAllListeners();
        }

        if (CodeButton.gameObject.activeSelf != show) CodeButton.gameObject.SetActive(show);

        if (show && Input.GetKeyDown(KeyCode.Space)) OpenCodePanel();
    }

    // La console s'ouvre quand on veut : elle n'affiche que les lettres deja trouvees
    void OpenCodePanel()
    {
        if (CodePanel == null || CodePanel.activeSelf) return;

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
