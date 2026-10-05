using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float leftLimit = -2f;
    public float rightLimit = 2f;

    private Vector3 startPos;
    private int direction = 1;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Translate(Vector2.right * direction * moveSpeed * Time.deltaTime);

        if (transform.position.x > startPos.x + rightLimit)
            direction = -1;
        else if (transform.position.x < startPos.x + leftLimit)
            direction = 1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody2D playerRb = other.GetComponentInParent<Rigidbody2D>();
            // Nếu Player đang rơi xuống (nhảy lên đầu) thì Enemy chết
            if (playerRb.linearVelocity.y < 0 && other.transform.position.y > transform.position.y + 0.2f)
            {
                Destroy(gameObject);
                playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 5f); // nảy nhẹ lên
            }
            else
            {
                
                GameManager_Dat.Instance.GameOver();
            }
        }
    }
}