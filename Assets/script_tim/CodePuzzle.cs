using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodePuzzle : MonoBehaviour
{
    [Header("Références UI")]
    public TMP_Text slotText;
    public TMP_Text feedbackText;
    public TMP_Text attemptsText;
    public Button[] fragButtons;      // FragBtn0 à FragBtn3
    public Button submitButton;
    public Button resetButton;

    [Header("Réglages")]
    public int maxAttempts = 4;

    private List<string> pool = new List<string>();   // fragments pas encore posés
    private string[] slots = new string[4];           // les 4 emplacements
    private bool[] locked = new bool[4];              // emplacements verrouillés
    private int attemptsLeft;
    private bool finished = false;

    void OnEnable()
    {
        SetupPuzzle();
    }

    void SetupPuzzle()
    {
        finished = false;
        attemptsLeft = maxAttempts;

        for (int i = 0; i < 4; i++)
        {
            slots[i] = "";
            locked[i] = false;
        }

        // Récupère les fragments collectés et les mélange
        pool = new List<string>(GameManager.Instance.collectedFragments);
        Shuffle(pool);

        // Branche les boutons
        for (int i = 0; i < fragButtons.Length; i++)
        {
            int index = i;   // copie locale obligatoire pour le listener
            fragButtons[i].onClick.RemoveAllListeners();
            fragButtons[i].onClick.AddListener(() => PlaceFragment(index));
        }

        submitButton.onClick.RemoveAllListeners();
        submitButton.onClick.AddListener(SubmitAttempt);

        resetButton.onClick.RemoveAllListeners();
        resetButton.onClick.AddListener(ResetSlots);

        // Applique l'indice s'il a été débloqué
        if (GameManager.Instance.hintUnlocked)
            ApplyHint();

        feedbackText.text = "";
        RefreshUI();
    }

    void Shuffle(List<string> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            string tmp = list[i];
            list[i] = list[r];
            list[r] = tmp;
        }
    }

    // Pose le fragment choisi dans le premier emplacement libre
    void PlaceFragment(int buttonIndex)
    {
        if (finished || buttonIndex >= pool.Count) return;

        for (int i = 0; i < 4; i++)
        {
            if (!locked[i] && slots[i] == "")
            {
                slots[i] = pool[buttonIndex];
                pool.RemoveAt(buttonIndex);
                RefreshUI();
                return;
            }
        }
    }

    // Renvoie tous les fragments non verrouillés dans la réserve
    void ResetSlots()
    {
        if (finished) return;

        for (int i = 0; i < 4; i++)
        {
            if (!locked[i] && slots[i] != "")
            {
                pool.Add(slots[i]);
                slots[i] = "";
            }
        }

        feedbackText.text = "";
        RefreshUI();
    }

    void SubmitAttempt()
    {
        if (finished) return;

        // Vérifie que les 4 emplacements sont remplis
        for (int i = 0; i < 4; i++)
        {
            if (slots[i] == "")
            {
                feedbackText.text = "PLACE ALL FOUR FRAGMENTS FIRST";
                return;
            }
        }

        List<string> solution = GameManager.Instance.solutionOrder;
        int correct = 0;

        // Verrouille les fragments bien placés
        for (int i = 0; i < 4; i++)
        {
            if (slots[i] == solution[i])
            {
                locked[i] = true;
                correct++;
            }
        }

        if (correct == 4)
        {
            finished = true;
            feedbackText.text = "ACCESS GRANTED — MISSION COMPLETE";
            Debug.Log("VICTOIRE ! Code trouvé.");
            RefreshUI();
            return;
        }

        attemptsLeft--;

        if (attemptsLeft <= 0)
        {
            finished = true;
            feedbackText.text = "ACCESS DENIED — SYSTEM LOCKED";
            Debug.Log("DÉFAITE : plus d'essais.");
            RefreshUI();
            return;
        }

        // Renvoie les fragments mal placés dans la réserve
        for (int i = 0; i < 4; i++)
        {
            if (!locked[i] && slots[i] != "")
            {
                pool.Add(slots[i]);
                slots[i] = "";
            }
        }

        feedbackText.text = correct + " FRAGMENT(S) CORRECTLY PLACED";
        RefreshUI();
    }

    // L'indice verrouille directement le premier fragment
    void ApplyHint()
    {
        string first = GameManager.Instance.solutionOrder[0];

        if (slots[0] != "" && slots[0] != first)
        {
            pool.Add(slots[0]);
        }

        pool.Remove(first);
        slots[0] = first;
        locked[0] = true;
    }

    void RefreshUI()
    {
        // Affichage du mot en cours de composition
        string display = "";
        for (int i = 0; i < 4; i++)
        {
            if (slots[i] == "")
                display += "__ ";
            else if (locked[i])
                display += "[" + slots[i] + "] ";
            else
                display += slots[i] + " ";
        }
        slotText.text = display;

        attemptsText.text = "ATTEMPTS LEFT : " + attemptsLeft;

        // Met à jour les boutons de fragments
        for (int i = 0; i < fragButtons.Length; i++)
        {
            bool active = (i < pool.Count) && !finished;
            fragButtons[i].gameObject.SetActive(active);

            if (active)
                fragButtons[i].GetComponentInChildren<TMP_Text>().text = pool[i];
        }

        submitButton.interactable = !finished;
        resetButton.interactable = !finished;
    }
}