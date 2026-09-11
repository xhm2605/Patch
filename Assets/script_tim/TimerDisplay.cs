using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public TMP_Text timerText;

    void Update()
    {
        if (GameManager.Instance == null) return;

        float t = GameManager.Instance.GetTimeLeft();
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

        // Passe en rouge sous 30 secondes
        timerText.color = (t <= 3f) ? Color.red : Color.white;
    }
}