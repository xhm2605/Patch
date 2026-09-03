using UnityEngine;
using UnityEngine.SceneManagement; // Très important : permet de gérer le rechargement du niveau !

public class PacManManager : MonoBehaviour
{
    // C'est cette fonction que le bouton va déclencher
    public void RestartGame()
    {
        // 1. On remet le temps à la normale (sinon le jeu va recharger mais restera figé !)
        Time.timeScale = 1f;
        
        // 2. On recharge la scène (le niveau) actuelle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}