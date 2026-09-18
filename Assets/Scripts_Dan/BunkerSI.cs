using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class BunkerSI : MonoBehaviour
{
    public Texture2D splat;

    private Texture2D originalTexture;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        originalTexture = spriteRenderer.sprite.texture;

        ResetBunker();
    }

    public void ResetBunker()
    {
        CopyTexture(originalTexture);

        gameObject.SetActive(true);
    }

    private void CopyTexture(Texture2D source)
    {
        Texture2D copy = new Texture2D(
            source.width,
            source.height,
            source.format,
            false
        )
        {
            filterMode = source.filterMode,
            anisoLevel = source.anisoLevel,
            wrapMode = source.wrapMode
        };

        copy.SetPixels32(source.GetPixels32());
        copy.Apply();

        Sprite sprite = Sprite.Create(
            copy,
            spriteRenderer.sprite.rect,
            new Vector2(0.5f, 0.5f),
            spriteRenderer.sprite.pixelsPerUnit
        );

        spriteRenderer.sprite = sprite;
    }

    public bool CheckCollision(BoxCollider2D other, Vector3 hitPoint)
    {
        Vector2 offset = other.size / 2;

        return Splat(hitPoint) ||
               Splat(hitPoint + (Vector3.down * offset.y)) ||
               Splat(hitPoint + (Vector3.up * offset.y)) ||
               Splat(hitPoint + (Vector3.left * offset.x)) ||
               Splat(hitPoint + (Vector3.right * offset.x));
    }

    private bool Splat(Vector3 hitPoint)
    {
        if (!CheckPoint(hitPoint, out int px, out int py))
        {
            return false;
        }

        Texture2D texture = spriteRenderer.sprite.texture;

        px -= splat.width / 2;
        py -= splat.height / 2;

        int startX = px;

        for (int y = 0; y < splat.height; y++)
        {
            px = startX;

            for (int x = 0; x < splat.width; x++)
            {
                Color pixel = texture.GetPixel(px, py);

                pixel.a *= splat.GetPixel(x, y).a;

                texture.SetPixel(px, py, pixel);

                px++;
            }

            py++;
        }

        texture.Apply();

        return true;
    }

    private bool CheckPoint(Vector3 hitPoint, out int px, out int py)
    {
        Vector3 localPoint = transform.InverseTransformPoint(hitPoint);

        localPoint.x += boxCollider.size.x / 2;
        localPoint.y += boxCollider.size.y / 2;

        Texture2D texture = spriteRenderer.sprite.texture;

        px = (int)(
            localPoint.x / boxCollider.size.x * texture.width
        );

        py = (int)(
            localPoint.y / boxCollider.size.y * texture.height
        );

        return texture.GetPixel(px, py).a != 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("InvaderSI"))
        {
            gameObject.SetActive(false);
        }
    }
}