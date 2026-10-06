using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float leftLimit = -2f;
    public float rightLimit = 2f;

    [Header("Desynchronization Settings")]
    [Tooltip("Randomizes initial direction so cloned enemies don't move in sync.")]
    public bool randomizeStartDirection = true;
    [Tooltip("Adds slight speed variance between individual enemies.")]
    public bool randomizeSpeed = true;
    public float speedVariation = 0.5f;

    [Header("Death Settings")]
    public float deathAnimDuration = 0.6f;
    [Header("Sound")]
    public AudioClip deathSound;

    // Private state variables
    private Vector3 startPos;
    private int direction = 1;
    private Animator animator;
    private SpriteRenderer sr;
    private bool isDying = false;

    void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();

        // Ensure transform rotation is reset to prevent vertical movement bugs
        transform.rotation = Quaternion.identity;

        // 1. Randomize initial movement direction (Left or Right)
        if (randomizeStartDirection)
        {
            direction = Random.value > 0.5f ? 1 : -1;
        }

        // 2. Apply slight speed variation
        if (randomizeSpeed)
        {
            moveSpeed += Random.Range(-speedVariation, speedVariation);
        }

        // 3. Offset walk animation frame so enemies don't march in sync
        if (animator != null)
        {
            animator.Play("EnemyP1_Walk", 0, Random.Range(0f, 1f));
        }
    }

    void Update()
    {
        // Stop movement logic if enemy is dying
        if (isDying) return;

        // Move horizontally in World Space to avoid local rotation conflicts
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);

        // Patrol boundary check
        if (transform.position.x > startPos.x + rightLimit)
            direction = -1;
        else if (transform.position.x < startPos.x + leftLimit)
            direction = 1;

        // Handle sprite flipping based on movement direction
        if (sr != null)
            sr.flipX = (direction > 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying) return;

        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponentInParent<Rigidbody2D>();

            // Player stomps enemy from above
            if (playerRb != null && playerRb.linearVelocity.y < 0 && other.transform.position.y > transform.position.y + 0.2f)
            {
                Die(playerRb);
            }
            // Player gets hit by enemy
            else
            {
                GameManager_Dat.Instance.GameOver();
            }
        }
    }

    public void Die(Rigidbody2D playerRb = null)
    {
        isDying = true;

        if (playerRb != null)
        {
            playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 5f);
        }

        if (animator != null)
        {
            animator.SetTrigger("Death");
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        Destroy(gameObject, deathAnimDuration);
    }
}