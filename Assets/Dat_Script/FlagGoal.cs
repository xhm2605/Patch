using UnityEngine;

public class FlagGoal : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource bgmAudioSource; 
    public AudioClip victorySound;      
    [Range(0f, 1f)] public float victoryVolume = 0.3f; 

    private bool isTriggered = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;

            if (bgmAudioSource != null)
            {
                bgmAudioSource.Stop();
            }

            if (victorySound != null)
            {
                AudioSource.PlayClipAtPoint(victorySound, Camera.main.transform.position, victoryVolume);
            }

            GameManager_Dat.Instance.Win();
        }
    }
}