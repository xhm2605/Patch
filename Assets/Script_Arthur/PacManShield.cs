using System.Collections;
using UnityEngine;

public class PacManShield : MonoBehaviour
{
    [Header("Visuel")]
    public GameObject shieldVisual; // Glisse ton ShieldVisual ici dans l'Inspector

    [Header("Paramètres")]
    public bool isShieldActive = false;
    private int nextShieldThreshold = 600; // Le palier d'activation

    void Start()
    {
        // On s'assure que le bouclier est éteint au lancement
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(false);
        }
    }

    // Cette fonction sera appelée quand le joueur gagne des points
    public void CheckShieldActivation(int currentTotalScore)
    {
        // Si le score dépasse le palier (ex: 600, puis 1200, etc.)
        if (currentTotalScore >= nextShieldThreshold)
        {
            StartCoroutine(ActivateShieldRoutine());
            nextShieldThreshold += 600; // Prépare le prochain palier
        }
    }

    // La minuterie du bouclier
    private IEnumerator ActivateShieldRoutine()
    {
        isShieldActive = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);

        // Le bouclier est solide pendant 2 secondes
        yield return new WaitForSeconds(2f);

        // Clignotement pendant la dernière seconde pour alerter le joueur
        for (int i = 0; i < 5; i++)
        {
            if (shieldVisual != null) shieldVisual.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            if (shieldVisual != null) shieldVisual.SetActive(true);
            yield return new WaitForSeconds(0.1f);
        }

        isShieldActive = false;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    // Gestion de la survie
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Vérifie si l'objet touché a le tag "Enemy" (comme défini sur ton image précédente pour Pinky)
        if (collision.CompareTag("Enemy")) 
        {
            if (isShieldActive)
            {
                Debug.Log("Bouclier actif : Collision annulée, Pac-Man survit !");
                // Le bouclier le protège, il ne se passe rien.
            }
            else
            {
                Debug.Log("Pac-Man est mort !");
                // INSÈRE ICI TON CODE DE MORT (ex: levelManager.GameOver(); ou recharger la scène)
            }
        }
    }
}
