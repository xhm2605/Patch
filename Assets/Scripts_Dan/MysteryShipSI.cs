using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(AudioSource))]
public class MysteryShipSI : MonoBehaviour
{
    // Mystery Ship movement, spawn timing and score settings
    public float speed = 5f;
    public float cycleTime = 30f;
    public int score = 300;
    public AudioClip flyingSound;

    [Header("Audio Fade")]
    public float fadeDuration = 1.5f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    // Positions just outside each side of the screen
    private Vector2 leftDestination;
    private Vector2 rightDestination;

    private int direction = -1;
    private bool spawned;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;
    }

    private void Start()
    {
        // Set destinations slightly outside the camera view
        Vector3 leftEdge = Camera.main.ViewportToWorldPoint(Vector3.zero);
        Vector3 rightEdge = Camera.main.ViewportToWorldPoint(Vector3.right);

        leftDestination = new Vector2(leftEdge.x - 1f, transform.position.y);
        rightDestination = new Vector2(rightEdge.x + 1f, transform.position.y);

        Despawn();
    }

    private void Update()
    {
        if (!spawned)
        {
            return;
        }

        // Move the ship in its current direction
        if (direction == 1)
        {
            MoveRight();
        }
        else
        {
            MoveLeft();
        }
    }

    private void MoveRight()
    {
        transform.position += speed * Time.deltaTime * Vector3.right;

        if (transform.position.x >= rightDestination.x)
        {
            Despawn();
        }
    }

    private void MoveLeft()
    {
        transform.position += speed * Time.deltaTime * Vector3.left;

        if (transform.position.x <= leftDestination.x)
        {
            Despawn();
        }
    }

    private void Spawn()
    {
        // Alternate the direction each time the ship appears
        direction *= -1;

        if (direction == 1)
        {
            transform.position = leftDestination;
        }
        else
        {
            transform.position = rightDestination;
        }

        spawned = true;

        // Start the flying sound quietly and fade it in
        if (flyingSound != null)
        {
            audioSource.clip = flyingSound;
            audioSource.volume = 0f;
            audioSource.Play();

            StartFade(1f);
        }
    }

    private void Despawn()
    {
        spawned = false;

        // Fade out the sound before stopping playback
        if (audioSource.isPlaying)
        {
            StartFade(0f, true);
        }

        // Move the ship outside the visible camera area
        if (direction == 1)
        {
            transform.position = rightDestination;
        }
        else
        {
            transform.position = leftDestination;
        }

        Invoke(nameof(Spawn), cycleTime);
    }

    private void StartFade(float targetVolume, bool stopAfterFade = false)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeAudio(targetVolume, stopAfterFade));
    }

    private IEnumerator FadeAudio(float targetVolume, bool stopAfterFade)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            audioSource.volume = Mathf.Lerp(
                startVolume,
                targetVolume,
                elapsed / fadeDuration
            );

            yield return null;
        }

        audioSource.volume = targetVolume;

        if (stopAfterFade)
        {
            audioSource.Stop();
        }

        fadeCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Award points and despawn when hit by the player's laser
        if (other.gameObject.layer == LayerMask.NameToLayer("LaserSI"))
        {
            Despawn();
            GameManagerSI.Instance.OnMysteryShipKilled(this);
        }
    }
}