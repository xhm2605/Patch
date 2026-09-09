using UnityEngine;

public class PinkyMovement : MonoBehaviour
{
    public float speed = 4f;
    public Transform[] waypoints; // La liste de tes 7 points (Point_1, Point_2, etc.)
    
    private int currentWaypointIndex = 0;
    private Rigidbody2D rb;
    private LevelManager levelManager;
    private Transform pacman;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        levelManager = FindObjectOfType<LevelManager>();
        
        GameObject p = GameObject.Find("PacMan");
        if (p != null) pacman = p.transform;
    }

    void FixedUpdate()
    {
        // On vérifie si le Super Pouvoir est actif
        bool isFleeing = levelManager != null && levelManager.pacmanEstInvincible;

        if (isFleeing && pacman != null)
        {
            // --- MODE PANIQUE (SUPER GOMME ACTIVE) ---
            // Il abandonne sa ronde et fuit en courant pour s'éloigner de Pac-Man
            Vector2 directionFuite = (transform.position - pacman.position).normalized;
            Vector2 newPos = (Vector2)transform.position + directionFuite * (speed * 0.7f) * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
        }
        else
        {
            // --- MODE NORMAL (PATROUILLE DES 7 POINTS) ---
            if (waypoints.Length == 0) return;

            // On cible le point actuel de la liste
            Transform target = waypoints[currentWaypointIndex];
            if (target == null) return;

            // Déplacement fluide vers le point
            Vector2 newPos = Vector2.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            // Dès qu'il arrive tout près du point, il passe au suivant dans l'ordre
            if (Vector2.Distance(transform.position, target.position) < 0.1f)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
    }
}