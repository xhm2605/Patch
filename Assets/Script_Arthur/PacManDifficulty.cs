using UnityEngine;

public class PacManDifficulty : MonoBehaviour
{
    [Header("Référence")]
    public LevelManager levelManager;

    void Start()
    {
        // La difficulté vient du menu principal (GameSettings.difficulty)
        int niveau = GameSettings.difficulty;

        // Une partie jouée pour gagner un indice est toujours en mode difficile
        if (GameManager.Instance != null && GameManager.Instance.IsHardMode()) niveau = 2;

        InitialiserDifficulte(niveau);
    }

    public void InitialiserDifficulte(int niveauDifficulte)
    {
        float pacSpeed = 5f;
        float ghostSpeed = 4f;
        float powerTime = 5f;

        switch (niveauDifficulte)
        {
            case 0: // Facile
                pacSpeed = 6f;
                ghostSpeed = 3f;
                powerTime = 7f;
                break;
            case 1: // Moyen
                pacSpeed = 5f;
                ghostSpeed = 4f;
                powerTime = 5f;
                break;
            case 2: // Difficile
                pacSpeed = 5f;
                ghostSpeed = 5.5f;
                powerTime = 3f;
                break;
        }

        PacManMovement pacman = FindObjectOfType<PacManMovement>();
        if (pacman != null) pacman.speed = pacSpeed;

        PinkyMovement[] fantomes = FindObjectsOfType<PinkyMovement>();
        foreach (PinkyMovement f in fantomes)
        {
            f.speed = ghostSpeed;
        }

        if (levelManager != null)
        {
            levelManager.dureeSuperPouvoir = powerTime;
            levelManager.LancerCompteARebours();
        }
    }
}
