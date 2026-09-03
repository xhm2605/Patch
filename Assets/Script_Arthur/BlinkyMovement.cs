using UnityEngine;

public class BlinkyMovement : MonoBehaviour
{
    public float speed = 4f; 
    private Rigidbody2D rb;
    private Vector2 direction;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ChooseRandomDirection();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ChooseRandomDirection();
    }

    // NOUVELLE FONCTION : Vérifie si Blinky est coincé
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si Blinky touche un mur et que sa vitesse réelle est presque nulle (il est bloqué)
        if (rb.linearVelocity.magnitude < 0.1f)
        {
            ChooseRandomDirection();
        }
    }

    void ChooseRandomDirection()
    {
        int randomDir = Random.Range(0, 4);

        if (randomDir == 0) direction = Vector2.up;         
        else if (randomDir == 1) direction = Vector2.down;  
        else if (randomDir == 2) direction = Vector2.left;  
        else if (randomDir == 3) direction = Vector2.right; 
    }
}
