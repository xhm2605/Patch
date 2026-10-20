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

        // Lance depuis le vaisseau : on suit la difficulte choisie dans le menu
        if (GameManager.Instance != null)
            currentDifficulty = (DifficultyLevel)Mathf.Clamp(GameSettings.difficulty, 0, 2);
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
        GameObject[] current = currentDifficulty == DifficultyLevel.Easy ? easyOnlyObjects
                             : currentDifficulty == DifficultyLevel.Hard ? hardOnlyObjects
                             : mediumOnlyObjects;

        // Un objet present dans plusieurs listes reste visible s'il appartient au niveau courant
        SetAll(easyOnlyObjects, false);
        SetAll(mediumOnlyObjects, false);
        SetAll(hardOnlyObjects, false);
        SetAll(current, true);
    }

    private static void SetAll(GameObject[] objects, bool active)
    {
        if (objects == null) return;

        foreach (GameObject obj in objects)
            if (obj != null) obj.SetActive(active);
    }

    public void GameOver()
    {
        if (ended) return;

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        StartCoroutine(EndMinigame(false));
    }

    public void Win()
    {
        if (ended) return;

        if (winPanel != null) winPanel.SetActive(true);
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