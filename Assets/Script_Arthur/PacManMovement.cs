using UnityEngine;
using UnityEngine.Tilemaps;

public class PacManMovement : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;

    // On crée une case pour glisser notre calque de points depuis Unity
    public Tilemap dotTilemap;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // À chaque image, on vérifie si Pac-Man est sur un point
        CheckAndEatDot();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = movement.normalized * speed;
    }

    void CheckAndEatDot()
    {
        // Si on a bien relié le calque des points dans l'Inspector
        if (dotTilemap != null)
        {
            // On trouve la position exacte de la case sous Pac-Man
            Vector3Int cellPosition = dotTilemap.WorldToCell(transform.position);

            // S'il y a un point (tile) sur cette case...
            if (dotTilemap.HasTile(cellPosition))
            {
                // ...on le supprime !
                dotTilemap.SetTile(cellPosition, null);
            }
        }
    }
}
