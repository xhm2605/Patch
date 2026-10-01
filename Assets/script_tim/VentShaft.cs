using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VentShaft : MonoBehaviour
{
    public VentShaft linked;
    public string destinationName = "";
    public KeyCode useKey = KeyCode.E;

    private static float blockedUntil = 0f;
    private static GameObject button;
    private static TMP_Text buttonLabel;
    private static VentShaft active;

    private bool playerInside = false;
    private Transform player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        player = other.transform;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || !other.CompareTag("Player")) return;

        playerInside = false;
        if (active == this) Hide();
    }

    void OnDisable()
    {
        if (active == this) Hide();
    }

    void Update()
    {
        bool ready = playerInside && linked != null && Time.time >= blockedUntil;

        if (ready)
        {
            Show();
            if (Input.GetKeyDown(useKey)) Travel();
        }
        else if (active == this)
        {
            Hide();
        }
    }

    // ---------- Bouton a l'ecran, partage par toutes les trappes ----------

    void Show()
    {
        EnsureButton();
        if (button == null) return;

        active = this;

        if (buttonLabel != null)
        {
            buttonLabel.text = string.IsNullOrEmpty(destinationName)
                ? "ENTER THE VENT"
                : "VENT TO " + destinationName.ToUpper();
        }

        if (!button.activeSelf) button.SetActive(true);
    }

    static void Hide()
    {
        active = null;
        if (button != null) button.SetActive(false);
    }

    static void EnsureButton()
    {
        if (button != null) return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        button = new GameObject("VentButton", typeof(RectTransform));
        button.layer = LayerMask.NameToLayer("UI");
        button.transform.SetParent(canvas.transform, false);

        RectTransform rt = button.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.anchoredPosition = new Vector2(0f, 120f);
        rt.sizeDelta = new Vector2(460f, 76f);

        Image img = button.AddComponent<Image>();
        img.sprite = MenuStyler.RoundedRect(96, 96, 16f, 2.5f,
            new Color(0.09f, 0.20f, 0.32f, 0.92f),
            new Color(0.40f, 0.82f, 1f, 1f));
        img.type = Image.Type.Sliced;

        Button btn = button.AddComponent<Button>();
        btn.targetGraphic = img;

        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.86f, 0.92f, 1f);
        cb.highlightedColor = Color.white;
        cb.pressedColor = new Color(0.55f, 0.72f, 0.86f);
        cb.fadeDuration = 0.1f;
        btn.colors = cb;

        btn.onClick.AddListener(() => { if (active != null) active.Travel(); });

        GameObject textGo = new GameObject("Label", typeof(RectTransform));
        textGo.layer = button.layer;
        textGo.transform.SetParent(button.transform, false);

        RectTransform trt = textGo.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;

        buttonLabel = textGo.AddComponent<TextMeshProUGUI>();
        buttonLabel.fontSize = 26f;
        buttonLabel.characterSpacing = 5f;
        buttonLabel.fontStyle = FontStyles.Bold | FontStyles.UpperCase;
        buttonLabel.color = new Color(0.88f, 0.96f, 1f);
        buttonLabel.alignment = TextAlignmentOptions.Center;
        buttonLabel.raycastTarget = false;

        button.SetActive(false);
    }

    // ---------- Trajet ----------

    public void Travel()
    {
        if (player == null || linked == null) return;
        if (Time.time < blockedUntil) return;

        Vector3 destination = linked.transform.position;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.position = destination;
        player.position = destination;

        SnapCamera(destination);

        // Evite de repartir aussitot par la trappe d'arrivee
        blockedUntil = Time.time + 1.2f;

        playerInside = false;
        Hide();

        SoundManager.PlayOpen();
    }

    void SnapCamera(Vector3 destination)
    {
        CameraFollow cam = FindAnyObjectByType<CameraFollow>();
        if (cam == null) return;

        cam.transform.position = destination + cam.offset;
    }
}
