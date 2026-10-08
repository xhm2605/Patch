using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public bool canMove = true;

    [Header("Shooting & Battery Ammo")]
    public int currentAmmo = 0;          // Current battery ammo count
    public int maxAmmo = 3;              // Maximum capacity = 3
    public GameObject bulletPrefab;      // Bullet prefab reference
    public Transform firePoint;          // Spawn position for bullets
    public KeyCode shootKey = KeyCode.Q; // Shoot input key
    [Header("Audio SFX")]
    public AudioClip shootSound;

    private Rigidbody2D rb;
    private bool isGrounded;
    private SpriteRenderer sr;
    private Animator animator;

    // Track the player's facing direction (1 = Right, -1 = Left)
    private float facingDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        if (!canMove) return;

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Update facing direction and flip sprite
        if (moveInput > 0)
        {
            facingDirection = 1f;
            sr.flipX = false;
        }
        else if (moveInput < 0)
        {
            facingDirection = -1f;
            sr.flipX = true;
        }

        // Handle movement animations
        if (animator != null)
        {
            animator.SetBool("isRunning", moveInput != 0);
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
        }

        // Ground check & jump
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Check for shooting input
        HandleShooting();
    }

    void HandleShooting()
    {
        if (currentAmmo > 0 && Input.GetKeyDown(KeyCode.Q))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        currentAmmo--; // Consume 1 ammo
        Debug.Log($"Shot fired! Remaining ammo: {currentAmmo}/{maxAmmo}");
        
        if (shootSound != null)
        {
           AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, 2.0f);
        }

        // Adjust spawn position relative to facing direction
        float offsetX = Mathf.Abs(firePoint.localPosition.x) * facingDirection;
        Vector3 spawnPosition = new Vector3(transform.position.x + offsetX, firePoint.position.y, firePoint.position.z);

        // Instantiate bullet ONLY when pressing the shoot button
        GameObject bullet = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        if (bulletScript != null)
        {
            bulletScript.SetupDirection(new Vector2(facingDirection, 0));
        }
    }

    // Called when collecting a battery
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        Debug.Log($"Collected Battery! Current Ammo: {currentAmmo}/{maxAmmo}");
    }


    


}