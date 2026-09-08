using UnityEngine;
using TMPro; // Pour le texte des vies et du chrono
using System.Collections; // Indispensable pour le chronomètre (Coroutine)

public class LevelManager : MonoBehaviour
{
    public int vies = 3;
    
    [Header("Affichage")]
    public TMP_Text texteVies;
    public TMP_Text texteChrono; // Pour afficher le compte à rebours du pouvoir
    
    [Header("Personnages à replacer")]
    public Transform pacman;
    public Transform blinky;
    public Transform pinky;
    public Transform inky;
    public Transform clyde;

    [Header("Super Pouvoir")]
    public bool pacmanEstInvincible = false;
    public SpriteRenderer[] fantomesSprites; 
    private Color[] couleursOriginales;

    // Mémoire des positions de départ
    private Vector2 startPacman, startBlinky, startPinky, startInky, startClyde;

    void Start()
    {
        // 1. On mémorise les positions
        if (pacman != null) startPacman = pacman.position;
        if (blinky != null) startBlinky = blinky.position;
        if (pinky != null) startPinky = pinky.position;
        if (inky != null) startInky = inky.position;
        if (clyde != null) startClyde = clyde.position;

        // 2. On affiche le texte des vies et on cache le chrono au départ
        MettreAJourTexte(); 
        if (texteChrono != null) texteChrono.text = "";

        // 3. On mémorise les couleurs normales des fantômes
        if (fantomesSprites.Length > 0)
        {
            couleursOriginales = new Color[fantomesSprites.Length];
            for (int i = 0; i < fantomesSprites.Length; i++)
            {
                if (fantomesSprites[i] != null)
                {
                    couleursOriginales[i] = fantomesSprites[i].color;
                }
            }
        }
    }

    public void PerdreUneVie()
    {
        vies--; 
        MettreAJourTexte(); 

        if (vies > 0)
        {
            // On replace tout le monde
            if (pacman != null) pacman.position = startPacman;
            if (blinky != null) blinky.position = startBlinky;
            if (pinky != null) pinky.position = startPinky;
            if (inky != null) inky.position = startInky;
            if (clyde != null) clyde.position = startClyde;
        }
    }

    private void MettreAJourTexte()
    {
        if (texteVies != null)
        {
            texteVies.text = "Lives : " + vies;
        }
    }

    // --- GESTION DU SUPER POUVOIR ---

    public void ActiverSuperPouvoir()
    {
        // Si Pac-Man mange une nouvelle gomme pendant que le pouvoir est actif,
        // on relance le chrono proprement à 5 secondes.
        StopAllCoroutines(); 
        StartCoroutine(RoutineSuperPouvoir());
    }

    private IEnumerator RoutineSuperPouvoir()
    {
        pacmanEstInvincible = true;
        
        // Tous les fantômes deviennent bleus
        for (int i = 0; i < fantomesSprites.Length; i++)
        {
            if (fantomesSprites[i] != null) fantomesSprites[i].color = Color.blue;
        }

        // Compte à rebours de 5 secondes
        float tempsRestant = 5f;
        while (tempsRestant > 0f)
        {
            if (texteChrono != null)
            {
                texteChrono.text = "Power : " + Mathf.Ceil(tempsRestant).ToString() + "s";
            }

            tempsRestant -= Time.deltaTime;
            yield return null; 
        }

        // On efface le texte du chrono à la fin
        if (texteChrono != null)
        {
            texteChrono.text = "";
        }

        // Fin du pouvoir, ils reprennent leur couleur d'origine
        for (int i = 0; i < fantomesSprites.Length; i++)
        {
            if (fantomesSprites[i] != null) fantomesSprites[i].color = couleursOriginales[i];
        }
        pacmanEstInvincible = false;
    }

    public void MangerFantome(GameObject fantome)
    {
        // On téléporte le fantôme mangé à sa base
        if (fantome.name == "Blinky") fantome.transform.position = startBlinky;
        if (fantome.name == "Pinky") fantome.transform.position = startPinky;
        if (fantome.name == "Inky") fantome.transform.position = startInky;
        if (fantome.name == "Clyde") fantome.transform.position = startClyde;
    }
}