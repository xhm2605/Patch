using UnityEngine;

public class InkyMovement : MonoBehaviour
{
    public float speed = 4f;
    public LayerMask obstacleLayer;
    
    private Rigidbody2D rb;
    private Transform pacman;
    private Vector2 currentDirection = Vector2.right;
    private float decisionTimer = 0f;
    private LevelManager levelManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.Find("PacMan");
        if (p != null) pacman = p.transform;
        
        levelManager = FindObjectOfType<LevelManager>();
    }

    void FixedUpdate()
    {
        if (pacman == null) return;

        decisionTimer -= Time.fixedDeltaTime;

        Vector2 forwardPos = (Vector2)transform.position + (currentDirection * 0.6f);
        bool isBlocked = Physics2D.OverlapCircle(forwardPos, 0.1f, obstacleLayer) != null;

        if (isBlocked || decisionTimer <= 0f)
        {
            Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
            Vector2 bestDirection = currentDirection;
            
            bool isFleeing = levelManager != null && levelManager.pacmanEstInvincible;
            float recordDistance = isFleeing ? -1f : Mathf.Infinity;
            bool pathFound = false;

            foreach (Vector2 dir in directions)
            {
                if (dir == -currentDirection) continue; 

                Vector2 futurePos = (Vector2)transform.position + (dir * 0.6f);
                if (Physics2D.OverlapCircle(futurePos, 0.1f, obstacleLayer) == null) 
                {
                    pathFound = true;
                    
                    Vector2 virtualPos = (Vector2)transform.position + dir;
                    float distance = Vector2.Distance(virtualPos, pacman.position);

                    if (isFleeing)
                    {
                        if (distance > recordDistance)
                        {
                            recordDistance = distance;
                            bestDirection = dir;
                        }
                    }
                    else
                    {
                        if (distance < recordDistance)
                        {
                            recordDistance = distance;
                            bestDirection = dir;
                        }
                    }
                }
            }

            if (!pathFound)
            {
                bestDirection = -currentDirection;
            }

            currentDirection = bestDirection;
            decisionTimer = 0.2f; 
        }

        float currentSpeed = (levelManager != null && levelManager.pacmanEstInvincible) ? speed * 0.7f : speed;
        rb.linearVelocity = currentDirection * currentSpeed; 
    }
}