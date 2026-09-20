using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class ProjectileSI : MonoBehaviour
{
    private BoxCollider2D boxCollider;

    // Projectile movement settings
    public Vector3 direction = Vector3.up;
    public float speed = 20f;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        // Move the projectile in its assigned direction
        transform.position += speed * Time.deltaTime * direction;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckCollision(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CheckCollision(other);
    }

    private void CheckCollision(Collider2D other)
    {
        // Check whether the projectile has collided with a bunker
        BunkerSI bunker = other.gameObject.GetComponent<BunkerSI>();

        // Bunkers use pixel-level collision to determine whether a visible section was hit
        if (bunker == null || bunker.CheckCollision(boxCollider, transform.position))
        {
            Destroy(gameObject);
        }
    }
}