using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MysteryShipSI : MonoBehaviour
{
    // Mystery Ship movement, spawn timing and score settings
    public float speed = 5f;
    public float cycleTime = 30f;
    public int score = 300;

    // Positions just outside each side of the screen
    private Vector2 leftDestination;
    private Vector2 rightDestination;

    private int direction = -1;
    private bool spawned;

    private void Start()
    {
        // Set destinations slightly outside the camera view
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        leftDestination = new Vector2(leftEdge.x - 1f, transform.position.y);
        rightDestination = new Vector2(rightEdge.x + 1f, transform.position.y);

        Despawn();
    }

    private void Update()
    {
        if (!spawned)
        {
            return;
        }

        // Move the ship in its current direction
        if (direction == 1)
        {
            MoveRight();
        }
        else
        {
            MoveLeft();
        }
    }

    private void MoveRight()
    {
        transform.position += speed * Time.deltaTime * Vector3.right;

        // Despawn after leaving the right side of the screen
        if (transform.position.x >= rightDestination.x)
        {
            Despawn();
        }
    }

    private void MoveLeft()
    {
        transform.position += speed * Time.deltaTime * Vector3.left;

        // Despawn after leaving the left side of the screen
        if (transform.position.x <= leftDestination.x)
        {
            Despawn();
        }
    }

    private void Spawn()
    {
        // Alternate the direction each time the ship appears
        direction *= -1;

        if (direction == 1)
        {
            transform.position = leftDestination;
        }
        else
        {
            transform.position = rightDestination;
        }

        spawned = true;
    }

    private void Despawn()
    {
        spawned = false;

        // Move the ship outside the visible camera area
        if (direction == 1)
        {
            transform.position = rightDestination;
        }
        else
        {
            transform.position = leftDestination;
        }

        // Spawn again after the selected cycle time
        Invoke(nameof(Spawn), cycleTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Award points and despawn when hit by the player's laser
        if (other.gameObject.layer == LayerMask.NameToLayer("LaserSI"))
        {
            Despawn();
            GameManagerSI.Instance.OnMysteryShipKilled(this);
        }
    }
}