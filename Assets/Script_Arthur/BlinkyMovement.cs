using UnityEngine;

public class BlinkyMovement : MonoBehaviour
{
    public float speed = 4f;
    public LayerMask obstacleLayer;
    
    private Rigidbody2D rb;
    private Transform pacman;
    private Vector2 currentDirection = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.Find("PacMan");
        if (p != null) pacman = p.transform;
    }

    void FixedUpdate()
    {
        if (pacman == null) return;

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 bestDirection = currentDirection;
        float shortestDistance = Mathf.Infinity;
        bool pathFound = false;

        foreach (Vector2 dir in directions)
        {
            if (dir == -currentDirection) continue; 

            // Le Radar : on vérifie un point situé juste devant lui avec un petit cercle
            Vector2 futurePos = (Vector2)transform.position + (dir * 0.6f);
            Collider2D hit = Physics2D.OverlapCircle(futurePos, 0.1f, obstacleLayer);

            // Si le radar ne touche aucun mur
            if (hit == null) 
            {
                pathFound = true;
                
                float distanceToPacman = Vector2.Distance(futurePos, pacman.position);
                if (distanceToPacman < shortestDistance)
                {
                    shortestDistance = distanceToPacman;
                    bestDirection = dir;
                }
            }
        }

        if (!pathFound)
        {
            bestDirection = -currentDirection;
        }

        currentDirection = bestDirection;
        rb.linearVelocity = currentDirection * speed; 
    }
}