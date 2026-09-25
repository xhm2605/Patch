using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        // On mémorise la position de base de la caméra
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // On génère des secousses aléatoires sur X et Y
            float x = originalPos.x + Random.Range(-1f, 1f) * magnitude;
            float y = originalPos.y + Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null; 
        }

        // On remet la caméra parfaitement au centre à la fin
        transform.localPosition = originalPos;
    }
}
