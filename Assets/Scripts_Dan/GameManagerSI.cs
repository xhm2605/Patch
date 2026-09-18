using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManagerSI : MonoBehaviour
{
    public static GameManagerSI Instance { get; private set; }

    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject youWinUI;
    [SerializeField] private Text scoreText;
    [SerializeField] private Text livesText;

    private PlayerSI player;
    private InvadersSI invaders;
    private MysteryShipSI mysteryShip;
    private BunkerSI[] bunkers;

    private bool ended = false;

    public int score { get; private set; } = 0;
    public int lives { get; private set; } = 3;

    private void Awake()
    {
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
        player = FindAnyObjectByType<PlayerSI>();
        invaders = FindAnyObjectByType<InvadersSI>();
        mysteryShip = FindAnyObjectByType<MysteryShipSI>();
        bunkers = FindObjectsByType<BunkerSI>();

        NewGame();
    }

    private void Update()
    {
        if (ended) return;

        if (lives <= 0 && Input.GetKeyDown(KeyCode.Return))
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        gameOverUI.SetActive(false);
        youWinUI.SetActive(false);

        SetScore(0);
        SetLives(3);
        NewRound();
    }

    private void NewRound()
    {
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
        Vector3 position = player.transform.position;
        position.x = 0f;

        player.transform.position = position;
        player.gameObject.SetActive(true);
    }

    private void GameOver()
    {
        if (ended) return;

        gameOverUI.SetActive(true);
        invaders.gameObject.SetActive(false);

        StartCoroutine(EndMinigame(false));
    }

    private void Win()
    {
        if (ended) return;

        youWinUI.SetActive(true);
        invaders.gameObject.SetActive(false);

        StartCoroutine(EndMinigame(true));
    }

    private System.Collections.IEnumerator EndMinigame(bool won)
    {
        ended = true;

        if (player != null)
            player.enabled = false;

        yield return new WaitForSecondsRealtime(1.5f);

        Time.timeScale = 1f;

        if (GameManager.Instance == null) yield break;

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
        this.lives = Mathf.Max(lives, 0);
        livesText.text = this.lives.ToString();
    }

    public void OnPlayerKilled(PlayerSI player)
    {
        if (ended) return;

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

        invader.gameObject.SetActive(false);

        SetScore(score + invader.score);

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

        if (invaders.gameObject.activeSelf)
        {
            invaders.gameObject.SetActive(false);
            OnPlayerKilled(player);
        }
    }
}