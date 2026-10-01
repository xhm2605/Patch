using UnityEngine;

public static class GameSettings
{
    public static int difficulty = 1;
    public static bool soundOn = true;
    public static float volume = 0.7f;

    public static string DifficultyName()
    {
        if (difficulty == 0) return "EASY";
        if (difficulty == 2) return "HARD";
        return "MEDIUM";
    }

    public static float TotalTime()
    {
        if (difficulty == 0) return 720f;
        if (difficulty == 2) return 450f;
        return 570f;
    }

    // Volume global : s'applique aussi aux mini-jeux
    public static void ApplyAudio()
    {
        AudioListener.volume = soundOn ? Mathf.Clamp01(volume) : 0f;
    }

    public static void ToggleSound()
    {
        soundOn = !soundOn;
        ApplyAudio();
    }

    public static void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
        if (volume > 0f) soundOn = true;
        ApplyAudio();
    }

    public static int VolumePercent()
    {
        return Mathf.RoundToInt(Mathf.Clamp01(volume) * 100f);
    }
}
