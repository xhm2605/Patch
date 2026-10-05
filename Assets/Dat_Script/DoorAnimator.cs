using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    private Animator anim;
    private bool playerIsNearby = false;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = false;
        }
    }

    void Update()
    {
        if (!playerIsNearby) return;

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (anim != null)
            {
                anim.SetTrigger("Open");
            }
        }
    }
}