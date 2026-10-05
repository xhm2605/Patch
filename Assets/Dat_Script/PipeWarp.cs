using UnityEngine;
using System.Collections;

public class PipeWarp : MonoBehaviour
{
    public enum Direction { Down, Up, Left, Right }
    public Direction entryDirection = Direction.Down;

    public Transform exitPoint;
    public float slideDuration = 1f;

    private bool playerOnTop = false;
    private Transform playerTransform;
    private PlayerController playerController;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTop = true;
            playerTransform = other.transform;
            playerController = other.GetComponent<PlayerController>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerOnTop = false;
        }
    }

    void Update()
    {
        if (!playerOnTop) return;

        bool keyPressed = entryDirection switch
        {
            Direction.Down => Input.GetKeyDown(KeyCode.DownArrow),
            Direction.Up => Input.GetKeyDown(KeyCode.UpArrow),
            Direction.Left => Input.GetKeyDown(KeyCode.LeftArrow),
            Direction.Right => Input.GetKeyDown(KeyCode.RightArrow),
            _ => false
        };

        if (keyPressed)
        {
            StartCoroutine(WarpTo(exitPoint));
        }
    }

    IEnumerator WarpTo(Transform target)
    {
        playerController.canMove = false;

        Vector3 startPos = playerTransform.position;
        Vector3 moveDir = entryDirection switch
        {
            Direction.Down => Vector3.down,
            Direction.Up => Vector3.up,
            Direction.Left => Vector3.left,
            Direction.Right => Vector3.right,
            _ => Vector3.zero
        };
        Vector3 hidePos = startPos + moveDir * 1f;

        float t = 0f;
        while (t < slideDuration)
        {
            playerTransform.position = Vector3.Lerp(startPos, hidePos, t / slideDuration);
            t += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = target.position;
        playerController.canMove = true;
    }
}