using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class Snake : MonoBehaviour
{
    private Vector2 direction = Vector2.zero; 
    private float tempsEntreMouvements = 0.5f; 
    private float chrono = 0f;
    private bool jeuEnCours = false; 
    private bool enAttenteDeTouche = false; 

    private Vector2 positionCible;
    private List<Vector2> ciblesCorps = new List<Vector2>();

    public Transform bodyPrefab; 
    private List<Transform> corps = new List<Transform>(); 

    public TMP_Text texteScore;
    private int score = 0;
    
    public int scorePourGagner = 10; 

    public GameObject texteGameOver;
    public GameObject texteVictoire; 
    
    // Mémorise ton texte original écrit dans l'Inspector
    private string texteGameOverOriginal;

    void Start()
    {
        // On sauvegarde ton texte personnalisé au tout début
        texteGameOverOriginal = texteGameOver.GetComponent<TMP_Text>().text;

        int niveauDifficulte = PlayerPrefs.GetInt("Difficulte", 2); 

        if (niveauDifficulte == 1) {
            tempsEntreMouvements = 0.4f; 
        } else if (niveauDifficulte == 2) {
            tempsEntreMouvements = 0.2f; 
        } else if (niveauDifficulte == 3) {
            tempsEntreMouvements = 0.08f; 
        }

        positionCible = transform.position; 
        StartCoroutine(CompteARebours());
    }

    IEnumerator CompteARebours()
    {
        enAttenteDeTouche = false;
        jeuEnCours = false; 
        direction = Vector2.zero; 
        
        texteVictoire.SetActive(false); 
        texteGameOver.SetActive(true);
        TMP_Text texteUI = texteGameOver.GetComponent<TMP_Text>();

        // Le décompte utilise l'objet texte du Game Over
        texteUI.text = "3";
        yield return new WaitForSeconds(1f);
        
        texteUI.text = "2";
        yield return new WaitForSeconds(1f);
        
        texteUI.text = "1";
        yield return new WaitForSeconds(1f);
        
        texteUI.text = "GO !";
        yield return new WaitForSeconds(0.5f);

        texteGameOver.SetActive(false);
        direction = Vector2.right; 
        jeuEnCours = true; 
    }

    void Update()
    {
        if (enAttenteDeTouche)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || 
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                ReinitialiserEtRelancer();
            }
            return;
        }

        if (!jeuEnCours) return;

        if (Input.GetKeyDown(KeyCode.UpArrow) && direction != Vector2.down) {
            direction = Vector2.up;
        } else if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Vector2.up) {
            direction = Vector2.down;
        } else if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Vector2.right) {
            direction = Vector2.left;
        } else if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Vector2.left) {
            direction = Vector2.right;
        }

        chrono += Time.deltaTime;
        if (chrono >= tempsEntreMouvements)
        {
            for (int i = corps.Count - 1; i > 0; i--) {
                ciblesCorps[i] = ciblesCorps[i - 1];
            }
            if (corps.Count > 0) {
                ciblesCorps[0] = positionCible;
            }

            positionCible += direction;
            chrono = 0f; 
        }

        float vitesseVisuelle = 1f / tempsEntreMouvements; 
        
        transform.position = Vector2.MoveTowards(transform.position, positionCible, vitesseVisuelle * Time.deltaTime);
        
        for (int i = 0; i < corps.Count; i++) {
            corps[i].position = Vector2.MoveTowards(corps[i].position, ciblesCorps[i], vitesseVisuelle * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!jeuEnCours) return;

        if (collision.gameObject.name == "Walls" || collision.gameObject.CompareTag("Body"))
        {
            FinDePartie(false); 
        }
        else if (collision.gameObject.CompareTag("Food"))
        {
            float randomX = Mathf.Round(Random.Range(-8f, 8f));
            float randomY = Mathf.Round(Random.Range(-5f, 5f));
            collision.transform.position = new Vector2(randomX, randomY);
            
            Grandir();

            score += 1;
            texteScore.text = "Score: " + score;

            if (score >= scorePourGagner)
            {
                FinDePartie(true); 
            }
        }
    }

    private void Grandir()
    {
        Transform nouveauMorceau = Instantiate(bodyPrefab, new Vector3(-1000, -1000, 0), Quaternion.identity);
        
        Collider2D col = nouveauMorceau.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Vector2 positionInitiale = (corps.Count > 0) ? (Vector2)corps[corps.Count - 1].position : (Vector2)transform.position;
        nouveauMorceau.position = positionInitiale; 
        
        corps.Add(nouveauMorceau);
        ciblesCorps.Add(positionInitiale); 
        
        if (col != null)
        {
            StartCoroutine(ActiverCollider(col)); 
        }
    }

    IEnumerator ActiverCollider(Collider2D col)
    {
        yield return new WaitForSeconds(tempsEntreMouvements);
        if (col != null) 
        {
            col.enabled = true;
        }
    }

    private void FinDePartie(bool estVictoire)
    {
        jeuEnCours = false;
        enAttenteDeTouche = true; 
        direction = Vector2.zero; 

        if (estVictoire)
        {
            texteVictoire.SetActive(true);
            texteGameOver.SetActive(false);
            // On ne modifie pas le texte de victoire, il garde ton réglage Unity
        }
        else
        {
            texteGameOver.SetActive(true);
            texteVictoire.SetActive(false);
            
            // On remet ton texte original pour effacer le "GO !"
            texteGameOver.GetComponent<TMP_Text>().text = texteGameOverOriginal; 
        }
    }

    private void ReinitialiserEtRelancer()
    {
        transform.position = Vector2.zero;
        positionCible = Vector2.zero; 
        
        for (int i = 0; i < corps.Count; i++) {
            Destroy(corps[i].gameObject);
        }
        corps.Clear(); 
        ciblesCorps.Clear(); 
        
        score = 0;
        texteScore.text = "Score: " + score;

        StartCoroutine(CompteARebours());
    }
}