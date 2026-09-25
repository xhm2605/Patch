using System.Collections;
using UnityEngine;

public class GhostSpawner : MonoBehaviour
{
    [Header("Paramètres d'apparition")]
    public GameObject[] ghosts; 
    public float timeBetweenSpawns = 2f; 

    void Start()
    {
        // On gèle les scripts de mouvement de tous les fantômes au démarrage
        foreach (GameObject ghost in ghosts)
        {
            ToggleMovement(ghost, false);
        }

        // On lance la séquence d'apparition
        StartCoroutine(SpawnSequence());
    }

    IEnumerator SpawnSequence()
    {
        foreach (GameObject ghost in ghosts)
        {
            // On réactive le script de mouvement (le fantôme se met à chasser)
            ToggleMovement(ghost, true);
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    // Fonction qui cherche et modifie uniquement les scripts de mouvement
    void ToggleMovement(GameObject ghost, bool state)
    {
        MonoBehaviour[] scripts = ghost.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            // Si le nom du script contient "Move" (ex: BlinkyMovement, ClydeMove...)
            if (script.GetType().Name.Contains("Move"))
            {
                script.enabled = state;
            }
        }
    }
}