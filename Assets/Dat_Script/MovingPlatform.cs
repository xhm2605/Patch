using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    public Vector2 moveDirection = Vector2.right; 
    public float distance = 5f;                   
    public float speed = 2f;                     

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float movement = Mathf.PingPong(Time.time * speed, distance);
        
        transform.position = startPosition + (Vector3)(moveDirection.normalized * movement);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 start = Application.isPlaying ? startPosition : transform.position;
        Vector3 end = start + (Vector3)(moveDirection.normalized * distance);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireCube(start, transform.localScale);
        Gizmos.DrawWireCube(end, transform.localScale);
    }
}