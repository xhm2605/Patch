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
    
    private string texteGameOverOriginal;

    public GameObject bombPrefab; 
    private List<GameObject> bombesActives = new List<GameObject>();

    // Variable pour mémoriser la difficulté dans tout le script
    private int difficulteActuelle = 2;

    void Start()
    {
        texteGameOverOriginal = texteGameOver.GetComponent<TMP_Text>().text;

        difficulteActuelle = PlayerPrefs.GetInt("Difficulte", 2);

        if (GameManager.Instance != null)
            difficulteActuelle = GameSettings.difficulty + 1;

        // On adapte la VITESSE en fonction de la difficulté
        if (difficulteActuelle == 1) {
            tempsEntreMouvements = 0.4f; 
        } else if (difficulteActuelle == 2) {
            tempsEntreMouvements = 0.2f; 
        } else if (difficulteActuelle == 3) {
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
        
        transform.eulerAngles = new Vector3(0, 0, 0);
        
        GererBombes(); 

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
            transform.eulerAngles = new Vector3(0, 0, 90);
        } else if (Input.GetKeyDown(KeyCode.DownArrow) && direction != Vector2.up) {
            direction = Vector2.down;
            transform.eulerAngles = new Vector3(0, 0, -90); 
        } else if (Input.GetKeyDown(KeyCode.LeftArrow) && direction != Vector2.right) {
            direction = Vector2.left;
            transform.eulerAngles = new Vector3(0, 0, 180); 
        } else if (Input.GetKeyDown(KeyCode.RightArrow) && direction != Vector2.left) {
            direction = Vector2.right;
            transform.eulerAngles = new Vector3(0, 0, 0); 
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

            Vector2 directionWagon = ciblesCorps[i] - (Vector2)corps[i].position;
            if (directionWagon != Vector2.zero)
            {
                float angle = Mathf.Atan2(directionWagon.y, directionWagon.x) * Mathf.Rad2Deg;
                corps[i].eulerAngles = new Vector3(0, 0, angle);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!jeuEnCours) return;

        if (collision.gameObject.name == "Walls" || collision.gameObject.CompareTag("Body") || collision.gameObject.CompareTag("Bomb"))
        {
            FinDePartie(false); 
        }
        else if (collision.gameObject.CompareTag("Food"))
        {
            collision.transform.position = TrouverPositionLibre();
            
            Grandir();
            score += 1;
            texteScore.text = "Score: " + score;

            DeplacerBombesExistantes();
            GererBombes(); 

            if (score >= scorePourGagner)
            {
                FinDePartie(true); 
            }
        }
    }

    private void DeplacerBombesExistantes()
    {
        foreach (GameObject bombe in bombesActives)
        {
            if (bombe != null)
            {
                bombe.transform.position = new Vector2(-1000, -1000); 
                bombe.transform.position = TrouverPositionLibre();
            }
        }
    }

    private void GererBombes()
    {
        int nbBombesSouhaitees = 1; 

        // On adapte le NOMBRE DE BOMBES en fonction de la difficulté
        if (difficulteActuelle == 1) 
        {
            // Mode Facile : 2 bombes max
            if (score >= 5) nbBombesSouhaitees = 2; 
            else nbBombesSouhaitees = 1;
        }
        else if (difficulteActuelle == 2) 
        {
            // Mode Moyen : 4 bombes max (tous les 3 points)
            if (score >= 9) nbBombesSouhaitees = 4;
            else if (score >= 6) nbBombesSouhaitees = 3;
            else if (score >= 3) nbBombesSouhaitees = 2;
            else nbBombesSouhaitees = 1;
        }
        else if (difficulteActuelle == 3) 
        {
            // Mode Difficile : 5 bombes max (tous les 2 points, monte très vite !)
            if (score >= 8) nbBombesSouhaitees = 5;
            else if (score >= 6) nbBombesSouhaitees = 4;
            else if (score >= 4) nbBombesSouhaitees = 3;
            else if (score >= 2) nbBombesSouhaitees = 2;
            else nbBombesSouhaitees = 1;
        }

        while (bombesActives.Count < nbBombesSouhaitees)
        {
            Vector2 positionAleatoire = TrouverPositionLibre();
            GameObject nouvelleBombe = Instantiate(bombPrefab, positionAleatoire, Quaternion.identity);
            bombesActives.Add(nouvelleBombe);
        }
    }

    private Vector2 TrouverPositionLibre()
    {
        Vector2 positionTest = Vector2.zero;
        bool positionValide = false;
        int maxTentatives = 200; 
        int tentatives = 0;

        GameObject food = GameObject.FindGameObjectWithTag("Food");

        while (!positionValide && tentatives < maxTentatives)
        {
            positionTest = new Vector2(Mathf.Round(Random.Range(-8f, 8f)), Mathf.Round(Random.Range(-5f, 5f)));
            positionValide = true;
            tentatives++;

            if ((Vector2)transform.position == positionTest) positionValide = false;

            foreach (Transform morceau in corps) {
                if ((Vector2)morceau.position == positionTest) positionValide = false;
            }

            foreach (GameObject bombe in bombesActives) {
                if (bombe != null && (Vector2)bombe.transform.position == positionTest) positionValide = false;
            }

            if (food != null && (Vector2)food.transform.position == positionTest) positionValide = false;
        }

        return positionTest;
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
        direction = Vector2.zero;

        bool relieAuVaisseau = (GameManager.Instance != null);
        enAttenteDeTouche = !relieAuVaisseau;

        if (estVictoire)
        {
            texteVictoire.SetActive(true);
            texteGameOver.SetActive(false);
        }
        else
        {
            texteGameOver.SetActive(true);
            texteVictoire.SetActive(false);
            texteGameOver.GetComponent<TMP_Text>().text = texteGameOverOriginal;
        }

        if (relieAuVaisseau) StartCoroutine(RetourAuVaisseau(estVictoire));
    }

    IEnumerator RetourAuVaisseau(bool estVictoire)
    {
        yield return new WaitForSeconds(estVictoire ? 1.2f : 1.6f);

        if (GameManager.Instance == null) yield break;

        if (estVictoire) GameManager.Instance.MinigameWon();
        else GameManager.Instance.MinigameLost();
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

        foreach (GameObject bombe in bombesActives) {
            if (bombe != null) Destroy(bombe);
        }
        bombesActives.Clear();
        
        score = 0;
        texteScore.text = "Score: " + score;

        StartCoroutine(CompteARebours());
    }
}