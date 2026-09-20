using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class InvaderSI : MonoBehaviour
{
    // Invader animation and score settings
    public Sprite[] animationSprites = new Sprite[0];
    public float animationTime = 1f;
    public int score = 10;

    private SpriteRenderer spriteRenderer;
    private int animationFrame;

    private void Awake()
    {
        // Initialise the invader with the first animation sprite
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = animationSprites[0];
    }

    private void Start()
    {
        // Repeatedly change the sprite to animate the invader
        InvokeRepeating(
            nameof(AnimateSprite),
            animationTime,
            animationTime
        );
    }

    private void AnimateSprite()
    {
        animationFrame++;

        // Return to the first frame after reaching the end
        if (animationFrame >= animationSprites.Length)
        {
            animationFrame = 0;
        }

        spriteRenderer.sprite = animationSprites[animationFrame];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy the invader when hit by the player's laser
        if (other.gameObject.layer == LayerMask.NameToLayer("LaserSI"))
        {
            GameManagerSI.Instance.OnInvaderKilled(this);
        }
        // Notify the Game Manager if an invader reaches the boundary
        else if (other.gameObject.layer == LayerMask.NameToLayer("BoundarySI"))
        {
            GameManagerSI.Instance.OnBoundaryReached();
        }
    }
}