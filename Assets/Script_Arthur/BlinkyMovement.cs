using UnityEngine;

public class BlinkyMovement : MonoBehaviour
{
    public float speed = 4f; 
    private Rigidbody2D rb;
    private Transform pacmanTransform; // Pour stocker la position de Pac-Man

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // On cherche automatiquement Pac-Man sur la carte grâce à son nom
        GameObject pacman = GameObject.Find("PacMan");
        if (pacman != null)
        {
            pacmanTransform = pacman.transform;
        }
    }

    void FixedUpdate()
    {
        if (pacmanTransform != null)
        {
            // Blinky calcule la direction pour aller tout droit vers Pac-Man
            Vector2 direction = (pacmanTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // S'il touche un mur, la physique gère son glissement le long du mur
    }
}