using UnityEngine;

public class PinkyMovement : MonoBehaviour
{
    public float speed = 4f;
    public Transform[] waypoints; 
    
    private int currentWaypointIndex = 0;
    private Rigidbody2D rb;
    private LevelManager levelManager;
    private Transform pacman;
    
    // NOUVEAU : Une petite mémoire pour savoir si elle fuyait déjà à la frame précédente
    private bool wasFleeing = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        levelManager = FindObjectOfType<LevelManager>();
        
        GameObject p = GameObject.Find("PacMan");
        if (p != null) pacman = p.transform;
    }

    void FixedUpdate()
    {
        if (waypoints.Length == 0) return;

        // On vérifie si le Super Pouvoir est actif
        bool isFleeing = levelManager != null && levelManager.pacmanEstInvincible;

        // --- DECLENCHEMENT DE LA PEUR ---
        // Si Pac-Man vient TOUT JUSTE de manger l'étoile, on force Pinky à faire demi-tour sur son circuit
        if (isFleeing && !wasFleeing)
        {
            // Formule mathématique pour reculer d'un cran dans la liste des waypoints de façon sécurisée
            currentWaypointIndex = (currentWaypointIndex - 1 + waypoints.Length) % waypoints.Length;
        }
        wasFleeing = isFleeing; // On mémorise l'état pour la boucle suivante

        // On adapte la vitesse : elle panique donc elle court moins vite (60% de sa vitesse)
        float vitesseActuelle = isFleeing ? (speed * 0.6f) : speed;

        // --- DEPLACEMENT (TOUJOURS SUR LES RAILS) ---
        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;

        // Déplacement fluide et sécurisé vers le waypoint
        Vector2 newPos = Vector2.MoveTowards(transform.position, target.position, vitesseActuelle * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Dès qu'elle arrive tout près du point, elle passe au suivant
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }
}