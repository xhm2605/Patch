using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Sans cela le personnage pivote sur lui-meme des qu'il frotte un mur
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.angularVelocity = 0f;
        }

        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        // Lecture des touches (ZQSD / WASD / flèches)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Évite d'aller plus vite en diagonale
        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        // Déplacement physique du personnage
        rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
    }
}