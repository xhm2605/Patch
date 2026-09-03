using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeTerminal : MonoBehaviour
{
    [Header("Références UI")]
    public Button codeButton;
    public GameObject codePanel;
    public TMP_Text feedbackText;

    private bool playerInRange = false;

    void Start()
    {
        if (codeButton != null) codeButton.gameObject.SetActive(false);
        if (codePanel != null) codePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        codeButton.gameObject.SetActive(true);
        codeButton.onClick.RemoveAllListeners();
        codeButton.onClick.AddListener(TryOpenCodePanel);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        codeButton.onClick.RemoveAllListeners();
        codeButton.gameObject.SetActive(false);
    }

    void TryOpenCodePanel()
    {
        if (!playerInRange) return;

        int repaired = GameManager.Instance.repairedTerminals.Count;

        if (repaired < 4)
        {
            // Story 8 : accès refusé tant que tout n'est pas réparé
            Debug.Log("Accès refusé : " + repaired + "/4 systèmes réparés");
            if (feedbackText != null)
                feedbackText.text = "REPAIRS INCOMPLETE : " + repaired + "/4";
            return;
        }

        codeButton.gameObject.SetActive(false);
        codePanel.SetActive(true);
    }
}