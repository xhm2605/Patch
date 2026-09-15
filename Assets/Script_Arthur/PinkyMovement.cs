using UnityEngine;

public class PinkyMovement : MonoBehaviour
{
    public float speed = 4f;
    public Transform[] waypoints; 
    
    private int currentWaypointIndex = 0;
    private Rigidbody2D rb;
    private LevelManager levelManager;
    private Transform pacman;
    
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

        bool isFleeing = levelManager != null && levelManager.pacmanEstInvincible;

        if (isFleeing && !wasFleeing)
        {
            currentWaypointIndex = (currentWaypointIndex - 1 + waypoints.Length) % waypoints.Length;
        }
        wasFleeing = isFleeing; 

        float vitesseActuelle = isFleeing ? (speed * 0.6f) : speed;

        Transform target = waypoints[currentWaypointIndex];
        if (target == null) return;

        Vector2 newPos = Vector2.MoveTowards(transform.position, target.position, vitesseActuelle * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
    }

    // LA NOUVELLE FONCTION EST ICI
    public void ResetGhost()
    {
        currentWaypointIndex = 0; 
        wasFleeing = false; 
        
        // On la remet à son point de départ (le premier waypoint)
        if (waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
        }
    }
}