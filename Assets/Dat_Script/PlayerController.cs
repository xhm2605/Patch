using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public bool canMove = true;
    


    private Rigidbody2D rb;
    private bool isGrounded;
    private SpriteRenderer sr;
    private Animator animator;
    [SerializeField]private Animator _animator;
    
    

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

        if (moveInput != 0)
        {
            _animator.SetBool("isRunning", true);
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }
        
            sr.flipX = moveInput < 0;

        animator.SetFloat("Speed", Mathf.Abs(moveInput));

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        

    }
}