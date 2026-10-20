using UnityEngine;

// Raccourci de test : Shift+W (ou F8) gagne, Shift+L (ou F9) perd.
// Actif seulement dans l'editeur et les builds de developpement.
public class MinigameTester : MonoBehaviour
{
    void Update()
    {
        if (!Debug.isDebugBuild || GameManager.Instance == null) return;

        bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.F8) || (shift && Input.GetKeyDown(KeyCode.W)))
            GameManager.Instance.MinigameWon();

        if (Input.GetKeyDown(KeyCode.F9) || (shift && Input.GetKeyDown(KeyCode.L)))
            GameManager.Instance.MinigameLost();
    }
}
