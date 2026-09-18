using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerSI : MonoBehaviour
{
    // Player movement and laser settings
    public float speed = 10f;
    public ProjectileSI laserPrefab;
    private ProjectileSI laser;

    private void Update()
    {
        Vector3 position = transform.position;

        // Move the player horizontally using keyboard controls
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            position.x -= speed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            position.x += speed * Time.deltaTime;
        }

        // Keep the player within the camera boundaries
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        position.x = Mathf.Clamp(position.x, leftEdge.x, rightEdge.x);
        transform.position = position;

        // Only allow one laser to be active at a time
        if (laser == null &&
            (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            laser = Instantiate(
                laserPrefab,
                transform.position,
                Quaternion.identity
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Player loses a life when hit by a missile or invader
        if (other.gameObject.layer == LayerMask.NameToLayer("MissileSI") ||
            other.gameObject.layer == LayerMask.NameToLayer("InvaderSI"))
        {
            GameManagerSI.Instance.OnPlayerKilled(this);
        }
    }
}