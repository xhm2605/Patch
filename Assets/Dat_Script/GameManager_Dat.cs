using UnityEngine;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public class GameManager_Dat : MonoBehaviour
{
    public static GameManager_Dat Instance;

    [Header("UI Panels")]
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Difficulty Settings")]
    public DifficultyLevel currentDifficulty = DifficultyLevel.Medium;

    [Header("Difficulty Map Objects")]
    [Tooltip("Objects that ONLY exist on Easy")]
    public GameObject[] easyOnlyObjects;

    [Tooltip("Objects that ONLY exist on Medium")]
    public GameObject[] mediumOnlyObjects;

    [Tooltip("Objects that ONLY exist on Hard")]
    public GameObject[] hardOnlyObjects;

    private bool ended = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ApplyDifficultyToMap();
    }

    public void SetDifficulty(int difficultyIndex)
    {
        currentDifficulty = (DifficultyLevel)difficultyIndex;
        ApplyDifficultyToMap();
    }

    private void ApplyDifficultyToMap()
    {
        bool isEasy = (currentDifficulty == DifficultyLevel.Easy);
        bool isMedium = (currentDifficulty == DifficultyLevel.Medium);
        bool isHard = (currentDifficulty == DifficultyLevel.Hard);

        // Active ONLY on Easy
        if (easyOnlyObjects != null)
        {
            foreach (GameObject obj in easyOnlyObjects)
            {
                if (obj != null) obj.SetActive(isEasy);
            }
        }

        // Active ONLY on Medium
        if (mediumOnlyObjects != null)
        {
            foreach (GameObject obj in mediumOnlyObjects)
            {
                if (obj != null) obj.SetActive(isMedium);
            }
        }

        // Active ONLY on Hard
        if (hardOnlyObjects != null)
        {
            foreach (GameObject obj in hardOnlyObjects)
            {
                if (obj != null) obj.SetActive(isHard);
            }
        }
    }

    public void GameOver()
    {
        if (ended) return;

        gameOverPanel.SetActive(true);
        StartCoroutine(EndMinigame(false));
    }

    public void Win()
    {
        if (ended) return;

        winPanel.SetActive(true);
        StartCoroutine(EndMinigame(true));
    }

    private System.Collections.IEnumerator EndMinigame(bool won)
    {
        ended = true;
        Time.timeScale = 1f;

        yield return new WaitForSeconds(1.5f);

        if (GameManager.Instance == null) yield break;

        if (won) GameManager.Instance.MinigameWon();
        else GameManager.Instance.MinigameLost();
    }
}