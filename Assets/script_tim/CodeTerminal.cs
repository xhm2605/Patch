using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeTerminal : MonoBehaviour
{
    [Header("Références UI")]
    public Button CodeButton;
    public GameObject CodePanel;
    public TMP_Text FeedbackText;

    private bool playerInRange = false;

    void Start()
    {
        if (CodeButton != null) CodeButton.gameObject.SetActive(false);
        if (CodePanel != null) CodePanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        CodeButton.gameObject.SetActive(true);
        CodeButton.onClick.RemoveAllListeners();
        CodeButton.onClick.AddListener(TryOpenCodePanel);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        CodeButton.onClick.RemoveAllListeners();
        CodeButton.gameObject.SetActive(false);
    }

    void TryOpenCodePanel()
    {
        if (!playerInRange) return;

        int repaired = GameManager.Instance.repairedTerminals.Count;

        if (repaired < 4)
        {
            // Story 8 : accès refusé tant que tout n'est pas réparé
            Debug.Log("Accès refusé : " + repaired + "/4 systèmes réparés");
            if (FeedbackText != null)
                FeedbackText.text = "REPAIRS INCOMPLETE : " + repaired + "/4";
            return;
        }

        CodeButton.gameObject.SetActive(false);
        CodePanel.SetActive(true);
    }
}