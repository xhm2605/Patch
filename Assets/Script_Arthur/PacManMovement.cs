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

    // NOUVEAU : On garde le LevelManager en mémoire pour surveiller l'invincibilité
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

        // 👉 NOUVEAU : Le système qui surveille la fin du pouvoir
        if (levelManager != null)
        {
            bool isInvincible = levelManager.pacmanEstInvincible;
            
            // Si Pac-Man n'est plus invincible MAIS qu'il l'était juste avant
            if (!isInvincible && wasInvincible)
            {
                // On remet la musique de fond normale
                if (bgmSource != null && backgroundMusic != null)
                {
                    bgmSource.clip = backgroundMusic;
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
                UpdateScoreText();
                
                if (meteorBeepSound != null) audioSource.PlayOneShot(meteorBeepSound);

                CheckWinCondition();
            }
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
            if (levelManager.pacmanEstInvincible)
            {
                if (eatGhostSound != null) audioSource.PlayOneShot(eatGhostSound);

                levelManager.MangerFantome(collision.gameObject);
                collision.gameObject.GetComponent<PinkyMovement>().ResetGhost();
            }
            else
            {
                if (deathSound != null) AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position, 1f);

                levelManager.PerdreUneVie();

                PinkyMovement[] tousLesFantomes = FindObjectsOfType<PinkyMovement>();
                foreach(PinkyMovement fantome in tousLesFantomes)
                {
                    fantome.ResetGhost();
                }

                if (levelManager.vies <= 0)
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
            // 👉 MODIFIÉ : On remplace la musique du lecteur au lieu de jouer un simple effet
            if (bgmSource != null && powerUpSound != null)
            {
                bgmSource.clip = powerUpSound;
                bgmSource.Play();
            }

            if (levelManager != null) levelManager.ActiverSuperPouvoir();
            
            Tilemap carteEtoiles = collision.GetComponent<Tilemap>();
            Vector3Int casePosition = carteEtoiles.WorldToCell(transform.position);
            carteEtoiles.SetTile(casePosition, null);
        }
    }
}