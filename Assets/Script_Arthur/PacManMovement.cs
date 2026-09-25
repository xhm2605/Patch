using UnityEngine;
using UnityEngine.Tilemaps;
using TMPro; 

public class PacManMovement : MonoBehaviour
{
    public float speed = 5f;
    public int pointsPerDot = 10;
    
    private Rigidbody2D rb;
    private Vector2 movement;

    public Tilemap dotTilemap;

    [Header("UI")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI gameOverText; 
    
    [Header("Audio")]
    public AudioClip powerUpSound;    
    public AudioClip eatGhostSound;   
    public AudioClip meteorBeepSound; 
    public AudioClip deathSound;      
    public AudioClip victoryMusic;    
    public AudioClip gameOverMusic;   
    public AudioClip backgroundMusic;
    
    private AudioSource audioSource;
    private AudioSource bgmSource;      

    private int score = 0;
    private int totalDots = 0;
    private int dotsEaten = 0;

    private LevelManager levelManager;
    private bool wasInvincible = false;
    private bool gameEnded = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>(); 
        levelManager = FindObjectOfType<LevelManager>();

        bgmSource = gameObject.AddComponent<AudioSource>();
        if (backgroundMusic != null)
        {
            bgmSource.clip = backgroundMusic;
            bgmSource.loop = true; 
            bgmSource.volume = 0.4f; 
            bgmSource.Play();
        }

        UpdateScoreText();

        if (winText != null) winText.gameObject.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false); 

        CountTotalDots();
    }

    void Update()
    {
        if (gameEnded) return;

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        CheckAndEatDot();

        if (levelManager != null)
        {
            bool isInvincible = levelManager.pacmanEstInvincible;
            
            if (!isInvincible && wasInvincible)
            {
                if (bgmSource != null && backgroundMusic != null)
                {
                    bgmSource.clip = backgroundMusic;
                    bgmSource.pitch = 1.0f; // Vitesse normale au retour
                    bgmSource.Play();
                }
            }
            
            wasInvincible = isInvincible;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * speed;
    }

    void CountTotalDots()
    {
        if (dotTilemap != null)
        {
            BoundsInt bounds = dotTilemap.cellBounds;
            TileBase[] allTiles = dotTilemap.GetTilesBlock(bounds);

            foreach (TileBase tile in allTiles)
            {
                if (tile != null) totalDots++;
            }
        }
    }

    void CheckAndEatDot()
    {
        if (dotTilemap != null)
        {
            Vector3Int cellPosition = dotTilemap.WorldToCell(transform.position);

            if (dotTilemap.HasTile(cellPosition))
            {
                dotTilemap.SetTile(cellPosition, null);
                score += pointsPerDot;
                
                dotsEaten++;
                UpdateMusicPitch(); // Mise à jour de la musique
                
                UpdateScoreText();
                
                PacManShield shieldScript = GetComponent<PacManShield>();
                if (shieldScript != null)
                {
                    shieldScript.CheckShieldActivation(score);
                }

                if (meteorBeepSound != null) audioSource.PlayOneShot(meteorBeepSound);

                CheckWinCondition();
            }
        }
    }

    private void UpdateMusicPitch()
    {
        if (bgmSource == null || totalDots <= 0 || (levelManager != null && levelManager.pacmanEstInvincible)) return;

        float progress = (float)dotsEaten / (float)totalDots;

        if (progress < 0.5f)
        {
            bgmSource.pitch = 1.0f; 
        }
        else if (progress >= 0.5f && progress < 0.8f) 
        {
            bgmSource.pitch = 1.1f; 
        }
        else 
        {
            bgmSource.pitch = 1.2f; 
        }
    }

    void CheckWinCondition()
    {
        if (dotsEaten >= totalDots)
        {
            if (winText != null) winText.gameObject.SetActive(true);

            if (bgmSource != null) bgmSource.Stop();
            if (victoryMusic != null) audioSource.PlayOneShot(victoryMusic);

            StartCoroutine(EndMinigame(true));
        }
    }

    private System.Collections.IEnumerator EndMinigame(bool won)
    {
        gameEnded = true;
        Time.timeScale = 1f;

        movement = Vector2.zero;
        speed = 0f;
        rb.linearVelocity = Vector2.zero;

        foreach (PinkyMovement f in FindObjectsOfType<PinkyMovement>())
            f.enabled = false;

        yield return new WaitForSeconds(1.5f);

        if (GameManager.Instance == null) yield break;

        if (won) GameManager.Instance.MinigameWon();
        else GameManager.Instance.MinigameLost();
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (gameEnded) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            PacManShield shieldScript = GetComponent<PacManShield>();

            if (levelManager != null && levelManager.pacmanEstInvincible)
            {
                if (eatGhostSound != null) audioSource.PlayOneShot(eatGhostSound);

                // Calcul des points de combo
                int pointsGagnes = 200 * (int)Mathf.Pow(2, levelManager.comboFantomes);
                score += pointsGagnes;
                levelManager.comboFantomes++; 
                
                UpdateScoreText(); 
                
                if (shieldScript != null)
                {
                    shieldScript.CheckShieldActivation(score);
                }

                levelManager.MangerFantome(collision.gameObject);
                collision.gameObject.GetComponent<PinkyMovement>().ResetGhost();
            }
            else if (shieldScript != null && shieldScript.isShieldActive)
            {
                Debug.Log("Bouclier actif : Collision annulée, Pac-Man survit !");
            }
            else
            {
                if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position, 1f);

                CameraShake cameraShake = Camera.main.GetComponent<CameraShake>();
                if (cameraShake != null) cameraShake.TriggerShake(0.2f, 0.3f); 

                if (levelManager != null) levelManager.PerdreUneVie();

                PinkyMovement[] tousLesFantomes = FindObjectsByType<PinkyMovement>(FindObjectsSortMode.None);
                foreach(PinkyMovement fantome in tousLesFantomes)
                {
                    fantome.ResetGhost();
                }

                if (levelManager != null && levelManager.vies <= 0)
                {
                    if (gameOverText != null) gameOverText.gameObject.SetActive(true);

                    if (bgmSource != null) bgmSource.Stop();
                    if (gameOverMusic != null) AudioSource.PlayClipAtPoint(gameOverMusic, Camera.main.transform.position, 1f);

                    StartCoroutine(EndMinigame(false));
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gameEnded) return;

        if (collision.gameObject.CompareTag("SuperGomme"))
        {
            if (bgmSource != null && powerUpSound != null)
            {
                bgmSource.clip = powerUpSound;
                bgmSource.pitch = 1.0f; 
                bgmSource.Play();
            }

            if (levelManager != null) levelManager.ActiverSuperPouvoir();
            
            Tilemap carteEtoiles = collision.GetComponent<Tilemap>();
            Vector3Int casePosition = carteEtoiles.WorldToCell(transform.position);
            carteEtoiles.SetTile(casePosition, null);
        }
    }
}