using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    private AudioSource ambienceSource;   // l'espace
    private AudioSource beepSource;       // les bips de console
    private AudioSource breathSource;     // la respiration
    private AudioSource engineSource;     // les reacteurs
    private AudioSource sfxSource;

    private bool playing = false;

    private AudioClip ambienceClip;
    private AudioClip breathClip;
    private AudioClip engineClip;
    private AudioClip beepClip;

    private AudioClip clickClip;
    private AudioClip repairClip;
    private AudioClip failClip;
    private AudioClip lockedClip;
    private AudioClip victoryClip;
    private AudioClip gameOverClip;
    private AudioClip tickClip;
    private AudioClip openClip;

    private const int Rate = 22050;
    private const float BeepLoop = 48f;

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

        ambienceClip = Load("SpaceAmbience");
        breathClip = Load("Breathing");
        engineClip = Load("RocketEngine");

        ambienceSource = NewSource(true, 0.85f);
        beepSource = NewSource(true, 0.95f);
        breathSource = NewSource(true, 0.25f);
        engineSource = NewSource(true, 0.10f);
        sfxSource = NewSource(false, 1f);

        BuildEffects();
        beepClip = BuildBeepTrack();

        GameSettings.ApplyAudio();

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyScene(SceneManager.GetActiveScene().name);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    AudioClip Load(string clipName)
    {
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip == null) Debug.LogWarning("SoundManager : " + clipName + " introuvable dans Assets/Resources");
        return clip;
    }

    AudioSource NewSource(bool loop, float volume)
    {
        AudioSource src = gameObject.AddComponent<AudioSource>();
        src.loop = loop;
        src.playOnAwake = false;
        src.volume = volume;
        return src;
    }

    void Update()
    {
        if (!playing) return;

        float breathVolume = 0.25f;
        float breathPitch = 1f;
        float tension = 0f;

        if (GameManager.Instance != null && GameManager.Instance.timerRunning)
        {
            float left = GameManager.Instance.GetTimeLeft();

            if (left <= 12f)
            {
                breathVolume = 0f;          // on etouffe
                breathPitch = 1.30f;
                tension = 1f;
            }
            else if (left <= 30f)
            {
                breathVolume = 0.42f;
                breathPitch = 1.22f;
                tension = 0.9f;
            }
            else if (left <= 60f)
            {
                breathVolume = 0.34f;
                breathPitch = 1.12f;
                tension = 0.6f;
            }
            else if (left <= 120f)
            {
                breathVolume = 0.29f;
                breathPitch = 1.05f;
                tension = 0.3f;
            }
        }

        // pas de respiration tant que le texte d'introduction defile
        if (IntroCrawl.IsPlaying) breathVolume = 0f;

        float speed = (breathVolume < breathSource.volume) ? 0.45f : 0.09f;
        breathSource.volume = Mathf.MoveTowards(breathSource.volume, breathVolume, Time.unscaledDeltaTime * speed);
        breathSource.pitch = Mathf.MoveTowards(breathSource.pitch, breathPitch, Time.unscaledDeltaTime * 0.12f);

        // les reacteurs montent en regime quand le temps presse
        engineSource.pitch = Mathf.MoveTowards(engineSource.pitch,
            Mathf.Lerp(1f, 1.10f, tension), Time.unscaledDeltaTime * 0.08f);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyScene(scene.name);
        HookSceneButtons();
    }

    // Le paysage sonore du vaisseau. Les mini-jeux gardent leur propre bande son.
    void ApplyScene(string sceneName)
    {
        bool wantsAmbience = (sceneName == "Main" || sceneName == "MainMenu");
        bool wantsBreath = (sceneName == "Main");

        if (wantsAmbience && !playing)
        {
            Start(ambienceSource, ambienceClip);
            Start(beepSource, beepClip);
            Start(engineSource, engineClip);
            playing = true;
        }
        else if (!wantsAmbience && playing)
        {
            ambienceSource.Stop();
            beepSource.Stop();
            engineSource.Stop();
            breathSource.Stop();
            playing = false;
        }

        if (playing)
        {
            if (wantsBreath && !breathSource.isPlaying)
            {
                breathSource.volume = 0f;
                Start(breathSource, breathClip);
            }
            else if (!wantsBreath && breathSource.isPlaying)
            {
                breathSource.Stop();
            }
        }
    }

    void Start(AudioSource src, AudioClip clip)
    {
        if (src == null || clip == null) return;

        src.Stop();
        src.resource = clip;
        src.loop = true;
        src.pitch = 1f;
        src.Play();
    }

    // ---------- API ----------

    public static void PlayClick() { PlaySfx(Get(s => s.clickClip), 0.45f); }
    public static void PlayOpen() { PlaySfx(Get(s => s.openClip), 0.6f); }
    public static void PlayRepair() { PlaySfx(Get(s => s.repairClip), 0.7f); }
    public static void PlayFail() { PlaySfx(Get(s => s.failClip), 0.7f); }
    public static void PlayLocked() { PlaySfx(Get(s => s.lockedClip), 0.6f); }
    public static void PlayVictory() { PlaySfx(Get(s => s.victoryClip), 0.8f); }
    public static void PlayGameOver() { PlaySfx(Get(s => s.gameOverClip), 0.8f); }
    public static void PlayTick() { PlaySfx(Get(s => s.tickClip), 0.4f); }

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

    // ---------- Effets courts ----------

    void BuildEffects()
    {
        clickClip = Blip("Click", 900f, 0.07f, 26f);
        openClip = Arpeggio("Open", new float[] { 523.25f, 784f }, 0.09f, 18f);
        repairClip = Arpeggio("Repair", new float[] { 523.25f, 659.25f, 784f, 1046.5f }, 0.11f, 11f);
        failClip = Arpeggio("Fail", new float[] { 392f, 311.13f, 233.08f }, 0.14f, 9f);
        lockedClip = Buzz("Locked", 140f, 0.22f);
        victoryClip = Arpeggio("Victory", new float[] { 523.25f, 659.25f, 784f, 1046.5f, 1318.5f }, 0.13f, 7f);
        gameOverClip = Slide("GameOver", 440f, 110f, 1.1f);
        tickClip = Blip("Tick", 1400f, 0.05f, 40f);
    }

    AudioClip Build(string clipName, Func<float, float> sample, float duration, float fade = 0.02f)
    {
        int count = Mathf.Max(1, Mathf.CeilToInt(duration * Rate));
        float[] data = new float[count];

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)Rate;
            float v = sample(t);

            if (fade > 0f)
            {
                if (t < fade) v *= t / fade;
                float remaining = duration - t;
                if (remaining < fade) v *= Mathf.Max(0f, remaining / fade);
            }

            data[i] = Mathf.Clamp(v, -1f, 1f);
        }

        return FromData(clipName, data);
    }

    AudioClip FromData(string clipName, float[] data)
    {
        AudioClip clip = AudioClip.Create(clipName, data.Length, 1, Rate, false);
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

    // ---------- Les bips de console, avec leur echo ----------

    AudioClip BuildBeepTrack()
    {
        int count = Mathf.CeilToInt(BeepLoop * Rate);
        float[] data = new float[count];

        AddBeep(data, 3.40f, 1220f, 0.090f, 0.38f);
        AddBeep(data, 9.80f, 880f, 0.130f, 0.42f);
        AddBeep(data, 16.30f, 1500f, 0.070f, 0.35f);
        AddBeep(data, 22.75f, 700f, 0.160f, 0.45f);
        AddBeep(data, 29.10f, 1050f, 0.100f, 0.39f);
        AddBeep(data, 35.60f, 1800f, 0.055f, 0.34f);
        AddBeep(data, 41.05f, 960f, 0.120f, 0.41f);
        AddBeep(data, 45.90f, 700f, 0.150f, 0.44f);

        AddEcho(data, 0.31f, 0.52f, 5);

        return FromData("Beeps", data);
    }

    void AddBeep(float[] data, float startSec, float freq, float duration, float volume)
    {
        int a = Mathf.Clamp(Mathf.RoundToInt(startSec * Rate), 0, data.Length - 1);
        int b = Mathf.Clamp(Mathf.RoundToInt((startSec + duration) * Rate), 0, data.Length);

        for (int i = a; i < b; i++)
        {
            float local = (i - a) / (float)Rate;
            float env = Mathf.Min(1f, local * 400f) * Mathf.Exp(-local * 14f);
            data[i] = Mathf.Clamp(data[i] + Mathf.Sin(2f * Mathf.PI * freq * local) * volume * env, -1f, 1f);
        }
    }

    // Echo par repetitions decroissantes. On enroule sur la fin de la boucle
    // pour que la traine ne soit pas coupee au raccord.
    void AddEcho(float[] data, float delaySec, float feedback, int repeats)
    {
        int len = data.Length;
        int delay = Mathf.Max(1, Mathf.RoundToInt(delaySec * Rate));

        float[] dry = (float[])data.Clone();

        for (int r = 1; r <= repeats; r++)
        {
            float gain = Mathf.Pow(feedback, r);
            int offset = delay * r;

            for (int i = 0; i < len; i++)
            {
                int j = (i + offset) % len;
                data[j] = Mathf.Clamp(data[j] + dry[i] * gain, -1f, 1f);
            }
        }
    }
}

public class ClickSoundTag : MonoBehaviour { }
