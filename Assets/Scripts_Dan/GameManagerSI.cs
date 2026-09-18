using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManagerSI : MonoBehaviour
{
    // Shared instance of the Space Invaders Game Manager
    public static GameManagerSI Instance { get; private set; }

    // UI elements
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject youWinUI;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;

    // Main Space Invaders game objects
    private PlayerSI player;
    private InvadersSI invaders;
    private MysteryShipSI mysteryShip;
    private BunkerSI[] bunkers;

    // Prevents game events from running after the minigame ends
    private bool ended = false;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private void Awake()
    {
        // Ensure only one GameManagerSI instance exists
        if (Instance != null)
        {
            DestroyImmediate(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        // Find the main objects needed to manage the minigame
        player = FindAnyObjectByType<PlayerSI>();
        invaders = FindAnyObjectByType<InvadersSI>();
        mysteryShip = FindAnyObjectByType<MysteryShipSI>();
        bunkers = FindObjectsByType<BunkerSI>();

        NewGame();
    }

    private void Update()
    {
        if (ended) return;

        // Allow a new game to start after all lives are lost
        if (lives <= 0 && Input.GetKeyDown(KeyCode.Return))
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        // Reset the UI, score and lives
        gameOverUI.SetActive(false);
        youWinUI.SetActive(false);

        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
        // Reset the invaders and bunkers for a new round
        invaders.ResetInvaders();
        invaders.gameObject.SetActive(true);

        for (int i = 0; i < bunkers.Length; i++)
        {
            bunkers[i].ResetBunker();
        }

        Respawn();
    }

    private void Respawn()
    {
        // Centre and reactivate the player
        Vector3 position = player.transform.position;
        position.x = 0f;

        player.transform.position = position;
        player.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        if (ended) return;

        // Stop the invaders and display the game over screen
        gameOverUI.SetActive(true);
        invaders.gameObject.SetActive(false);

        StartCoroutine(EndMinigame(false));
    }

    private void Win()
    {
        if (ended) return;

        // Stop the invaders and display the win screen
        youWinUI.SetActive(true);
        invaders.gameObject.SetActive(false);

        StartCoroutine(EndMinigame(true));
    }

    private System.Collections.IEnumerator EndMinigame(bool won)
    {
        ended = true;

        // Disable player controls when the minigame finishes
        if (player != null)
            player.enabled = false;

        // Give the player time to see the result screen
        yield return new WaitForSecondsRealtime(1.5f);

        Time.timeScale = 1f;

        if (GameManager.Instance == null) yield break;

        // Report the result to the main game's Game Manager
        if (won) GameManager.Instance.MinigameWon();
        else GameManager.Instance.MinigameLost();
    }

    private void SetScore(int score)
    {
        this.score = score;
        scoreText.text = score.ToString().PadLeft(4, '0');
    }

    private void SetLives(int lives)
    {
        // Prevent the number of lives from dropping below zero
        this.lives = Mathf.Max(lives, 0);
        livesText.text = this.lives.ToString();
    }

    public void OnPlayerKilled(PlayerSI player)
    {
        if (ended) return;

        // Remove a life and temporarily disable the player
        SetLives(lives - 1);
        player.gameObject.SetActive(false);

        if (lives > 0)
        {
            Invoke(nameof(NewRound), 1f);
        }
        else
        {
            GameOver();
        }
    }

    public void OnInvaderKilled(InvaderSI invader)
    {
        if (ended) return;

        // Remove the invader and add its points to the score
        invader.gameObject.SetActive(false);
        SetScore(score + invader.score);

        // Win when all invaders have been destroyed
        if (invaders.GetAliveCount() == 0)
        {
            Win();
        }
    }

    public void OnMysteryShipKilled(MysteryShipSI mysteryShip)
    {
        if (ended) return;

        SetScore(score + mysteryShip.score);
    }

    public void OnBoundaryReached()
    {
        if (ended) return;

        // Invaders reaching the boundary causes the player to lose a life
        if (invaders.gameObject.activeSelf)
        {
            invaders.gameObject.SetActive(false);
            OnPlayerKilled(player);
        }
    }
}