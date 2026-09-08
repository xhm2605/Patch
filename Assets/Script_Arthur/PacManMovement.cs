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
    public GameObject replayButton; // NOUVEAU : La variable pour le bouton
    
    private int score = 0;
    private int totalDots = 0;
    private int dotsEaten = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateScoreText();

        if (winText != null) winText.gameObject.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false); 
        
        // NOUVEAU : On masque le bouton au début du jeu
        if (replayButton != null) replayButton.SetActive(false); 

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
            
            // NOUVEAU : On affiche le bouton quand on gagne
            if (replayButton != null) replayButton.SetActive(true);
            
            // On fige le temps pour la victoire
            Time.timeScale = 0f; 
        }
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
        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (gameOverText != null)
            {
                gameOverText.gameObject.SetActive(true);
            }
            
            // NOUVEAU : On affiche le bouton quand on perd
            if (replayButton != null) replayButton.SetActive(true);
            
            Time.timeScale = 0f;
            gameObject.SetActive(false);
        }
    }
}