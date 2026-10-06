using UnityEngine;

public class BatteryItem : MonoBehaviour
{
    [Header("Pop-up Animation Settings")]
    public float popForce = 3f; // Force applied when spawning from chest

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // Apply upward burst force when spawned
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(Random.Range(-0.5f, 0.5f), popForce);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                player = other.GetComponentInParent<PlayerController>();
            }

            if (player != null)
            {
                player.AddAmmo(1); // Add 1 ammo to player
            }

            Destroy(gameObject); // Destroy battery after collection
        }
    }
}