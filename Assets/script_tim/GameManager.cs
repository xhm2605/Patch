using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Accessible depuis n'importe quel script, n'importe quelle scène
    public static GameManager Instance;

    [Header("Progression de la partie")]
    public List<string> repairedTerminals = new List<string>();
    public List<string> collectedFragments = new List<string>();

    private string currentTerminalId;
    private string currentFragment;

    void Awake()
    {
        // On garantit qu'il n'existe qu'un seul GameManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);   // survit au changement de scène
    }

    private bool isHardMode = false;
    public bool hintUnlocked = false;

    public void LaunchMinigame(string terminalId, string fragment, string sceneName, bool hardMode = false)
    {
        currentTerminalId = terminalId;
        currentFragment = fragment;
        isHardMode = hardMode;
        SceneManager.LoadScene(sceneName);
    }

    public void MinigameWon()
    {
        if (isHardMode)
        {
            hintUnlocked = true;
            Debug.Log("Indice débloqué !");
        }
        else if (!repairedTerminals.Contains(currentTerminalId))
        {
            repairedTerminals.Add(currentTerminalId);
            collectedFragments.Add(currentFragment);
            Debug.Log("Panne réparée : " + currentTerminalId + " | Fragment : " + currentFragment);
        }

        SceneManager.LoadScene("Main");
    }

    public bool IsHardMode()
    {
        return isHardMode;
    }

    public void MinigameLost()
    {
        Debug.Log("Mini-jeu raté, retour au vaisseau");
        SceneManager.LoadScene("Main");
    }

    public bool IsRepaired(string terminalId)
    {
        return repairedTerminals.Contains(terminalId);
    }
}