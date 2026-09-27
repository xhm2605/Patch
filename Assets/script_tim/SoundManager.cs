using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private bool musicPlaying = false;

    private AudioClip ambient;
    private AudioClip clickClip;
    private AudioClip repairClip;
    private AudioClip failClip;
    private AudioClip lockedClip;
    private AudioClip victoryClip;
    private AudioClip gameOverClip;
    private AudioClip tickClip;
    private AudioClip openClip;

    private const int Rate = 22050;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        GameObject go = new GameObject("SoundManager");
        go.AddComponent<SoundManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.85f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        BuildClips();

        GameSettings.ApplyAudio();

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyScene(scene.name);
        HookSceneButtons();
    }

    // La musique d'ambiance ne joue que dans le vaisseau et le menu :
    // les mini-jeux ont deja leur propre bande son.
    void ApplyScene(string sceneName)
    {
        bool wantsMusic = (sceneName == "Main" || sceneName == "MainMenu");

        if (wantsMusic)
        {
            if (!musicPlaying)
            {
                musicSource.Stop();
                musicSource.resource = ambient;
                musicSource.loop = true;
                musicSource.Play();
                musicPlaying = true;
            }
        }
        else if (musicPlaying)
        {
            musicSource.Stop();
            musicPlaying = false;
        }

        Debug.Log("AUDIO DIAG"
            + " | scene " + sceneName
            + " | playing " + musicSource.isPlaying
            + " | resource " + (musicSource.resource != null ? musicSource.resource.name : "null")
            + " | peak " + PeakOf(ambient).ToString("0.000")
            + " | srcVol " + musicSource.volume.ToString("0.00")
            + " | listener " + AudioListener.volume.ToString("0.00")
            + " | listenerPause " + AudioListener.pause
            + " | mute " + musicSource.mute
            + " | listeners " + FindObjectsByType<AudioListener>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length);
    }

    float PeakOf(AudioClip clip)
    {
        if (clip == null) return -1f;

        int n = Mathf.Min(8192, clip.samples);
        if (n <= 0) return -2f;

        float[] buffer = new float[n];
        clip.GetData(buffer, clip.samples / 3);

        float peak = 0f;
        for (int i = 0; i < n; i++) peak = Mathf.Max(peak, Mathf.Abs(buffer[i]));
        return peak;
    }

    // ---------- API ----------

    public static void PlayClick() { PlaySfx(Get(s => s.clickClip), 0.45f); }
    public static void PlayOpen() { PlaySfx(Get(s => s.openClip), 0.6f); }
    public static void PlayRepair() { PlaySfx(Get(s => s.repairClip), 0.7f); }
    public static void PlayFail() { PlaySfx(Get(s => s.failClip), 0.7f); }
    public static void PlayLocked() { PlaySfx(Get(s => s.lockedClip), 0.6f); }
    public static void PlayVictory() { PlaySfx(Get(s => s.victoryClip), 0.8f); }
    public static void PlayGameOver() { PlaySfx(Get(s => s.gameOverClip), 0.8f); }
    public static void PlayTick() { PlaySfx(Get(s => s.tickClip), 0.35f); }

    static AudioClip Get(Func<SoundManager, AudioClip> pick)
    {
        if (Instance == null) return null;
        return pick(Instance);
    }

    static void PlaySfx(AudioClip clip, float volume)
    {
        if (Instance == null || clip == null) return;
        Instance.sfxSource.PlayOneShot(clip, volume);
    }

    public static void AttachClick(Button button)
    {
        if (button == null) return;
        if (button.GetComponent<ClickSoundTag>() != null) return;

        button.gameObject.AddComponent<ClickSoundTag>();
        button.onClick.AddListener(PlayClick);
    }

    void HookSceneButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button b in buttons) AttachClick(b);
    }

    // ---------- Generation des sons ----------

    void BuildClips()
    {
        clickClip = Blip("Click", 900f, 0.07f, 26f);
        openClip = Arpeggio("Open", new float[] { 523.25f, 784f }, 0.09f, 18f);
        repairClip = Arpeggio("Repair", new float[] { 523.25f, 659.25f, 784f, 1046.5f }, 0.11f, 11f);
        failClip = Arpeggio("Fail", new float[] { 392f, 311.13f, 233.08f }, 0.14f, 9f);
        lockedClip = Buzz("Locked", 140f, 0.22f);
        victoryClip = Arpeggio("Victory", new float[] { 523.25f, 659.25f, 784f, 1046.5f, 1318.5f }, 0.13f, 7f);
        gameOverClip = Slide("GameOver", 440f, 110f, 1.1f);
        tickClip = Blip("Tick", 1400f, 0.05f, 40f);
        ambient = Ambient();
    }

    AudioClip Build(string clipName, Func<float, float> sample, float duration)
    {
        int count = Mathf.Max(1, Mathf.CeilToInt(duration * Rate));
        float[] data = new float[count];

        float fade = 0.02f;

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)Rate;
            float v = sample(t);

            // fondu aux extremites pour eviter les claquements
            if (t < fade) v *= t / fade;
            float remaining = duration - t;
            if (remaining < fade) v *= Mathf.Max(0f, remaining / fade);

            data[i] = Mathf.Clamp(v, -1f, 1f);
        }

        AudioClip clip = AudioClip.Create(clipName, count, 1, Rate, false);
        clip.SetData(data, 0);
        return clip;
    }

    AudioClip Blip(string clipName, float freq, float duration, float decay)
    {
        return Build(clipName, t =>
        {
            float env = Mathf.Exp(-t * decay);
            float wave = Mathf.Sin(2f * Mathf.PI * freq * t);
            wave += 0.25f * Mathf.Sin(4f * Mathf.PI * freq * t);
            return wave * 0.45f * env;
        }, duration);
    }

    AudioClip Arpeggio(string clipName, float[] notes, float noteDuration, float decay)
    {
        float total = notes.Length * noteDuration;

        return Build(clipName, t =>
        {
            int index = Mathf.Clamp((int)(t / noteDuration), 0, notes.Length - 1);
            float local = t - index * noteDuration;

            float env = Mathf.Exp(-local * decay) * Mathf.Min(1f, local * 120f);
            float wave = Mathf.Sin(2f * Mathf.PI * notes[index] * local);
            wave += 0.2f * Mathf.Sin(4f * Mathf.PI * notes[index] * local);
            return wave * 0.4f * env;
        }, total);
    }

    AudioClip Buzz(string clipName, float freq, float duration)
    {
        return Build(clipName, t =>
        {
            float env = Mathf.Exp(-t * 6f);
            float saw = Mathf.Repeat(freq * t, 1f) * 2f - 1f;
            return saw * 0.3f * env;
        }, duration);
    }

    AudioClip Slide(string clipName, float fromFreq, float toFreq, float duration)
    {
        return Build(clipName, t =>
        {
            float k = t / duration;
            float freq = Mathf.Lerp(fromFreq, toFreq, k * k);
            float env = Mathf.Exp(-t * 1.6f);
            return Mathf.Sin(2f * Mathf.PI * freq * t) * 0.4f * env;
        }, duration);
    }

    // Nappe spatiale en la mineur pentatonique, boucle de 4 mesures
    AudioClip Ambient()
    {
        float beat = 60f / 68f;
        float barLength = beat * 4f;
        int bars = 4;
        float duration = barLength * bars;

        float[] bassNotes = { 110.00f, 110.00f, 87.31f, 98.00f };
        float[] arpNotes = { 440.00f, 523.25f, 659.25f, 523.25f };
        float[] barShift = { 1f, 1f, 0.7937f, 0.8909f };

        float step = beat / 2f;

        return Build("Ambient", t =>
        {
            int bar = Mathf.Clamp((int)(t / barLength), 0, bars - 1);
            float barTime = t - bar * barLength;

            float bassEnv = Mathf.Exp(-barTime * 0.5f) * Mathf.Min(1f, barTime * 30f);
            float bass = Mathf.Sin(2f * Mathf.PI * bassNotes[bar] * barTime) * 0.38f * bassEnv;

            int n = (int)(t / step);
            float localTime = t - n * step;
            float arpEnv = Mathf.Exp(-localTime * 3.5f) * Mathf.Min(1f, localTime * 90f);
            float freq = arpNotes[n % arpNotes.Length] * barShift[bar];
            float arp = Mathf.Sin(2f * Mathf.PI * freq * localTime) * 0.20f * arpEnv;

            float shimmer = Mathf.Sin(2f * Mathf.PI * freq * 2f * localTime) * 0.06f * arpEnv;

            return bass + arp + shimmer;
        }, duration);
    }
}

public class ClickSoundTag : MonoBehaviour { }
