using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Progression de la partie")]
    public List<string> repairedTerminals = new List<string>();
    public List<string> collectedFragments = new List<string>();
    public bool hintUnlocked = false;
    public bool introShown = false;

    [Header("Code final")]
    public List<string> wordPool = new List<string>
    {
        "STARSHIP", "ASTEROID", "GRAVITON", "SPACEMAN", "ROCKETRY", "MOONBASE"
    };
    public string currentWord;
    public List<string> solutionOrder = new List<string>();

    [Header("Chronometre")]
    public float totalTime = 480f;
    public bool timerRunning = false;

    [Header("Verrouillage apres echec")]
    public float lockoutDuration = 20f;

    private Dictionary<string, float> lockoutUntil = new Dictionary<string, float>();
    private Dictionary<string, string> fragmentByTerminal = new Dictionary<string, string>();

    private List<string> terminalIds = new List<string>
    {
        "Engine", "Shield", "Oxygen", "Comms", "Coolant"
    };

    // Pac-Man et Snake ne rendent qu'une lettre, les trois autres en rendent deux
    private Dictionary<string, int> fragmentLength = new Dictionary<string, int>
    {
        { "Engine", 2 }, { "Shield", 1 }, { "Oxygen", 2 }, { "Comms", 2 }, { "Coolant", 1 }
    };

    private float timeLeft;
    private string currentTerminalId;
    private string currentFragment;
    private bool isHardMode = false;

    private bool pendingEnd = false;
    private bool pendingWin = false;
    private string pendingReason = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        GameSettings.ApplyAudio();

        StartNewGame();
    }

    void Update()
    {
        if (!timerRunning) return;

        float before = timeLeft;
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 30f && timeLeft > 0f &&
            Mathf.FloorToInt(before) != Mathf.FloorToInt(timeLeft))
        {
            SoundManager.PlayTick();
        }

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameOver("TIME OUT");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (pendingEnd && scene.name == "Main")
        {
            pendingEnd = false;
            EndScreen.Show(pendingWin, pendingReason);
        }
    }

    public int TerminalCount()
    {
        return terminalIds.Count;
    }

    public List<string> TerminalIds()
    {
        return terminalIds;
    }

    public void StartNewGame()
    {
        repairedTerminals.Clear();
        collectedFragments.Clear();
        fragmentByTerminal.Clear();
        lockoutUntil.Clear();
        hintUnlocked = false;

        currentWord = PickWord();

        // Le code final se compose lettre par lettre
        solutionOrder.Clear();
        for (int i = 0; i < currentWord.Length; i++)
            solutionOrder.Add(currentWord.Substring(i, 1));

        // On tire l'ordre des bornes, puis on decoupe le mot selon leur capacite
        List<string> ids = new List<string>(terminalIds);
        for (int i = 0; i < ids.Count; i++)
        {
            int r = Random.Range(i, ids.Count);
            string tmp = ids[i];
            ids[i] = ids[r];
            ids[r] = tmp;
        }

        int cursor = 0;
        foreach (string id in ids)
        {
            int length = fragmentLength.ContainsKey(id) ? fragmentLength[id] : 2;
            length = Mathf.Clamp(length, 0, currentWord.Length - cursor);

            fragmentByTerminal[id] = length > 0 ? currentWord.Substring(cursor, length) : "?";
            cursor += length;
        }

        totalTime = GameSettings.TotalTime();
        StartTimer();

        Debug.Log("Difficulte : " + GameSettings.DifficultyName() + " | Mot : " + currentWord);
    }

    string PickWord()
    {
        List<string> valid = new List<string>();
        foreach (string w in wordPool)
        {
            if (!string.IsNullOrEmpty(w) && w.Length == 8) valid.Add(w);
            else Debug.LogWarning("Mot ignore, il doit faire 8 lettres : " + w);
        }

        if (valid.Count == 0) return "STARSHIP";
        return valid[Random.Range(0, valid.Count)];
    }

    // Toutes les lettres ramassees, separees une a une
    public List<string> CollectedLetters()
    {
        List<string> letters = new List<string>();

        foreach (string frag in collectedFragments)
            for (int i = 0; i < frag.Length; i++)
                letters.Add(frag.Substring(i, 1));

        return letters;
    }

    public string GetFragmentFor(string terminalId)
    {
        return fragmentByTerminal.ContainsKey(terminalId) ? fragmentByTerminal[terminalId] : "??";
    }

    public void StartTimer()
    {
        timeLeft = totalTime;
        timerRunning = true;
    }

    public float GetTimeLeft()
    {
        return timeLeft;
    }

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
            Debug.Log("Indice debloque");
            SoundManager.PlayVictory();
        }
        else if (!repairedTerminals.Contains(currentTerminalId))
        {
            repairedTerminals.Add(currentTerminalId);
            collectedFragments.Add(currentFragment);
            Debug.Log("Panne reparee : " + currentTerminalId + " | Fragment : " + currentFragment);
            SoundManager.PlayRepair();
        }

        SceneManager.LoadScene("Main");
    }

    public void MinigameLost()
    {
        if (!isHardMode && !string.IsNullOrEmpty(currentTerminalId))
            lockoutUntil[currentTerminalId] = Time.unscaledTime + lockoutDuration;

        SoundManager.PlayFail();

        SceneManager.LoadScene("Main");
    }

    public bool IsLocked(string terminalId)
    {
        if (!lockoutUntil.ContainsKey(terminalId)) return false;
        return Time.unscaledTime < lockoutUntil[terminalId];
    }

    public float GetLockRemaining(string terminalId)
    {
        if (!lockoutUntil.ContainsKey(terminalId)) return 0f;
        return Mathf.Max(0f, lockoutUntil[terminalId] - Time.unscaledTime);
    }

    public bool IsHardMode()
    {
        return isHardMode;
    }

    public bool IsRepaired(string terminalId)
    {
        return repairedTerminals.Contains(terminalId);
    }

    public void GameOver(string reason)
    {
        timerRunning = false;
        Debug.Log("GAME OVER : " + reason);
        SoundManager.PlayGameOver();
        ShowEnd(false, reason);
    }

    public void Victory()
    {
        timerRunning = false;
        Debug.Log("VICTOIRE");
        SoundManager.PlayVictory();
        ShowEnd(true, "ACCESS GRANTED");
    }

    void ShowEnd(bool win, string reason)
    {
        if (SceneManager.GetActiveScene().name == "Main")
        {
            EndScreen.Show(win, reason);
        }
        else
        {
            pendingEnd = true;
            pendingWin = win;
            pendingReason = reason;
            SceneManager.LoadScene("Main");
        }
    }
}
