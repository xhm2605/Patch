using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Accessible depuis n'importe quel script, n'importe quelle scène
    public static GameManager Instance;

    // ---------- VARIABLES ----------

    [Header("Progression de la partie")]
    public List<string> repairedTerminals = new List<string>();
    public List<string> collectedFragments = new List<string>();
    public bool hintUnlocked = false;

    [Header("Code final")]
    public List<string> solutionOrder = new List<string> { "ST", "AR", "SH", "IP" };

    [Header("Chronomètre")]
    public float totalTime = 30f;          // 5 minutes
    public bool timerRunning = false;

    private float timeLeft;
    private string currentTerminalId;
    private string currentFragment;
    private bool isHardMode = false;

    // Retient qu'une fin de partie doit s'afficher dès le retour dans Main
    private bool pendingEnd = false;
    private bool pendingWin = false;
    private string pendingReason = "";

    // ---------- CYCLE DE VIE ----------

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

        SceneManager.sceneLoaded += OnSceneLoaded;

        StartTimer();   // provisoire : sera déclenché par le menu plus tard
    }

    void Update()
    {
        if (!timerRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameOver("TIME OUT");
        }
    }

    // Si la partie s'est terminée pendant un mini-jeu, on affiche l'écran
    // de fin seulement une fois revenu dans la scène Main
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingEnd && scene.name == "Main")
        {
            pendingEnd = false;
            EndScreen.Show(pendingWin, pendingReason);
        }
    }

    // ---------- CHRONOMÈTRE ----------

    public void StartTimer()
    {
        timeLeft = totalTime;
        timerRunning = true;
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

    // ---------- MINI-JEUX ----------

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

    public void MinigameLost()
    {
        Debug.Log("Mini-jeu raté, retour au vaisseau");
        SceneManager.LoadScene("Main");
    }

    public bool IsHardMode()
    {
        return isHardMode;
    }

    public bool IsRepaired(string terminalId)
    {
        return repairedTerminals.Contains(terminalId);
    }

    // ---------- FIN DE PARTIE ----------

    public void GameOver(string reason)
    {
        timerRunning = false;
        Debug.Log("GAME OVER : " + reason);
        ShowEnd(false, reason);
    }

    public void Victory()
    {
        timerRunning = false;
        Debug.Log("VICTOIRE !");
        ShowEnd(true, "ACCESS GRANTED");
    }

    void ShowEnd(bool win, string reason)
    {
        // L'écran de fin vit dans la scène Main
        if (SceneManager.GetActiveScene().name == "Main")
        {
            EndScreen.Show(win, reason);
        }
        else
        {
            // On est dans un mini-jeu : on note et on rentre au vaisseau
            pendingEnd = true;
            pendingWin = win;
            pendingReason = reason;
            SceneManager.LoadScene("Main");
        }
    }
}