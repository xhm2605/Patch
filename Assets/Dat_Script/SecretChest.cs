using UnityEngine;

public class SecretChest : MonoBehaviour
{
    [SerializeField] private Animator chestAnimator;

    [Header("Item Pop-up Settings")]
    public GameObject itemPrefab; 
    public Transform spawnPoint;      
    public float popForce = 4f;
    public AudioClip openChestSound;      

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
        bool shiftPressed = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);

        if (isPlayerNearby && !isOpened && shiftPressed)
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

        if (openChestSound != null)
        {
            AudioSource.PlayClipAtPoint(openChestSound, Camera.main.transform.position, 0.1f);
        }

        SpawnItem();
    }

    private void SpawnItem()
    {
        if (itemPrefab == null) return;

        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        GameObject spawnedItem = Instantiate(itemPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D itemRb = spawnedItem.GetComponent<Rigidbody2D>();
        if (itemRb != null)
        {
            itemRb.linearVelocity = new Vector2(Random.Range(-0.5f, 0.5f), popForce);
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