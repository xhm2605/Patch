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
    public TextMeshProUGUI gameOverText; // NOUVEAU : La case pour le texte de défaite
    
    private int score = 0;
    private int totalDots = 0;
    private int dotsEaten = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateScoreText();

        if (winText != null) winText.gameObject.SetActive(false);
        
        // NOUVEAU : On masque le Game Over au début
        if (gameOverText != null) gameOverText.gameObject.SetActive(false); 

        CountTotalDots();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        CheckAndEatDot();
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
                CheckWinCondition();
            }
        }
    }

    void CheckWinCondition()
    {
        if (dotsEaten >= totalDots)
        {
            if (winText != null) winText.gameObject.SetActive(true);
            speed = 0;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score : " + score;
        }
    }

    // NOUVELLE FONCTION : Détecter la collision avec un fantôme
   private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si l'objet qu'on touche a l'étiquette "Enemy"
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // On affiche le Game Over
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);
            }
            
            // On fige complètement le temps dans le jeu !
            Time.timeScale = 0f;
            
            // On fait disparaître Pac-Man
            gameObject.SetActive(false);
        }
    }
}