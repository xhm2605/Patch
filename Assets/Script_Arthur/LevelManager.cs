using UnityEngine;
using TMPro; 
using System.Collections; 

public class LevelManager : MonoBehaviour
{
    public int vies = 3;
    
    [Header("Affichage")]
    public TMP_Text texteVies;
    public TMP_Text texteChrono; 
    public TMP_Text texteDemarrage; 
    
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

    public float dureeSuperPouvoir = 5f;
    public int comboFantomes = 0; // Le compteur de combo

    private Vector2 startPacman, startBlinky, startPinky, startInky, startClyde;

    void Start()
    {
        if (pacman != null) startPacman = pacman.position;
        if (blinky != null) startBlinky = blinky.position;
        if (pinky != null) startPinky = pinky.position;
        if (inky != null) startInky = inky.position;
        if (clyde != null) startClyde = clyde.position;

        MettreAJourTexte(); 
        if (texteChrono != null) texteChrono.text = "";

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

    public void LancerCompteARebours()
    {
        StartCoroutine(RoutineDemarrage());
    }

    private IEnumerator RoutineDemarrage()
    {
        Time.timeScale = 0f;

        if (texteDemarrage != null)
        {
            texteDemarrage.gameObject.SetActive(true);
            
            texteDemarrage.text = "3";
            yield return new WaitForSecondsRealtime(1f);
            
            texteDemarrage.text = "2";
            yield return new WaitForSecondsRealtime(1f);
            
            texteDemarrage.text = "1";
            yield return new WaitForSecondsRealtime(1f);
            
            texteDemarrage.text = "GO !";
            yield return new WaitForSecondsRealtime(1f);

            texteDemarrage.gameObject.SetActive(false);
        }
        else 
        {
            yield return new WaitForSecondsRealtime(3f);
        }

        Time.timeScale = 1f;
    }

    public void PerdreUneVie()
    {
        vies--; 
        MettreAJourTexte(); 

        if (vies > 0)
        {
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

    public void ActiverSuperPouvoir()
    {
        StopAllCoroutines(); 
        StartCoroutine(RoutineSuperPouvoir());
    }

    private IEnumerator RoutineSuperPouvoir()
    {
        pacmanEstInvincible = true;
        comboFantomes = 0; // On réinitialise le combo à 0
        
        for (int i = 0; i < fantomesSprites.Length; i++)
        {
            if (fantomesSprites[i] != null) fantomesSprites[i].color = Color.blue;
        }

        float tempsRestant = dureeSuperPouvoir; 
        while (tempsRestant > 0f)
        {
            if (texteChrono != null)
            {
                texteChrono.text = "Power : " + Mathf.Ceil(tempsRestant).ToString() + "s";
            }

            tempsRestant -= Time.deltaTime;
            yield return null; 
        }

        if (texteChrono != null)
        {
            texteChrono.text = "";
        }

        for (int i = 0; i < fantomesSprites.Length; i++)
        {
            if (fantomesSprites[i] != null) fantomesSprites[i].color = couleursOriginales[i];
        }
        pacmanEstInvincible = false;
    }

    public void MangerFantome(GameObject fantome)
    {
        if (fantome.name == "Blinky") fantome.transform.position = startBlinky;
        if (fantome.name == "Pinky") fantome.transform.position = startPinky;
        if (fantome.name == "Inky") fantome.transform.position = startInky;
        if (fantome.name == "Clyde") fantome.transform.position = startClyde;
    }
}