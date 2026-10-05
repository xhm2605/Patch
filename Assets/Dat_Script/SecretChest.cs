using UnityEngine;

public class SecretChest : MonoBehaviour
{
    [SerializeField] private Animator chestAnimator;
    public KeyCode triggerKey = KeyCode.LeftShift;

    private bool isPlayerNearby = false;
    private bool isOpened = false;

    void Start()
    {
        if (chestAnimator == null)
        {
            chestAnimator = GetComponentInChildren<Animator>();
        }
    }

    void Update()
    {
        if (isPlayerNearby && !isOpened && Input.GetKeyDown(triggerKey))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        isOpened = true;

        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger("Open");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}