using System.Collections;
using UnityEngine;

// Plays a Present and a Past version of the music in sync and crossfades
// between them when the player warps. Also plays a warp sound effect and
// briefly muffles the music during the transition.
//
// Setup: put this on an empty GameObject (e.g. "AudioManager") in the scene and
// drag in the clips. It listens to TimeWarpManager.TimeSwappedEvent, so warps
// trigger it automatically. Set "Start In Past" to match TimeWarpManager.
public class WarpAudio : MonoBehaviour
{
    public static WarpAudio Instance { get; private set; }

    [Header("Music (both clips should be the same length and tempo)")]
    public AudioClip presentMusic;
    public AudioClip pastMusic;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    public float crossfadeTime = 0.6f;

    [Header("Warp sound effects")]
    public AudioClip warpToPastSfx;
    public AudioClip warpToPresentSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Tooltip("Random pitch change per warp so a repeated sound doesn't get tiring. 0.03 = +/-3%.")]
    [Range(0f, 0.1f)] public float pitchVariation = 0.03f;

    [Header("Death sound")]
    public AudioClip deathSfx;
    [Range(0f, 1f)] public float deathVolume = 0.9f;

    [Header("Muffle the music during the warp")]
    public bool muffleDuringWarp = true;
    public float muffledCutoff = 800f; // lower = more muffled

    [Header("Start state")]
    public bool startInPast = false;

    [Header("Debug")]
    [Tooltip("Print music volumes to the Console after every warp.")]
    public bool logVolumes = true;

    const float OpenCutoff = 22000f;

    AudioSource presentSource, pastSource, sfxSource;
    AudioLowPassFilter presentFilter, pastFilter;
    Coroutine fadeRoutine;
    bool inPast;

    public bool InPast => inPast;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        presentSource = MakeMusicSource("PresentMusic", presentMusic, out presentFilter);
        pastSource = MakeMusicSource("PastMusic", pastMusic, out pastFilter);

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
    }

    void Start()
    {
        inPast = startInPast;
        presentSource.volume = inPast ? 0f : musicVolume;
        pastSource.volume = inPast ? musicVolume : 0f;

        // Start both tracks on the exact same audio tick so they stay in sync.
        double startTime = AudioSettings.dspTime + 0.1;
        if (presentSource.clip != null) presentSource.PlayScheduled(startTime);
        if (pastSource.clip != null) pastSource.PlayScheduled(startTime);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Listen to TimeWarpManager: it fires TimeSwappedEvent(inPast) after every successful swap,
    // so the music and warp sound follow the game without editing TimeWarpManager.
    void OnEnable()
    {
        TimeWarpManager.TimeSwappedEvent += Warp;
        PlayerRespawn.PlayerDiedEvent += PlayDeath;
    }

    void OnDisable()
    {
        TimeWarpManager.TimeSwappedEvent -= Warp;
        PlayerRespawn.PlayerDiedEvent -= PlayDeath;
        OnDisable_FinishFade();
    }

    // Called by PlayerRespawn every time the player dies (hit or fell off the level).
    public void PlayDeath()
    {
        if (deathSfx == null) return;
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(deathSfx, deathVolume);
    }

    // Call with true to go to the Past, false to go to the Present.
    public void Warp(bool toPast)
    {
        if (toPast == inPast) return;
        inPast = toPast;

        AudioClip sfx = toPast ? warpToPastSfx : warpToPresentSfx;
        if (sfx != null)
        {
            sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            sfxSource.PlayOneShot(sfx, sfxVolume);
        }

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(Crossfade());
    }

    public void ToggleWarp()
    {
        Warp(!inPast);
    }

    IEnumerator Crossfade()
    {
        float startPresent = presentSource.volume;
        float startPast = pastSource.volume;
        float targetPresent = inPast ? 0f : musicVolume;
        float targetPast = inPast ? musicVolume : 0f;

        // Filters are only switched on while warping, so they can never leave the music muffled.
        presentFilter.enabled = muffleDuringWarp;
        pastFilter.enabled = muffleDuringWarp;

        for (float t = 0f; t < crossfadeTime; t += Time.unscaledDeltaTime)
        {
            float k = t / crossfadeTime;
            // Equal-power crossfade: keeps the overall loudness steady through the middle.
            float fadeIn = Mathf.Sin(k * Mathf.PI * 0.5f);
            float fadeOut = Mathf.Cos(k * Mathf.PI * 0.5f);
            presentSource.volume = inPast ? startPresent * fadeOut : Mathf.Lerp(startPresent, targetPresent, fadeIn);
            pastSource.volume = inPast ? Mathf.Lerp(startPast, targetPast, fadeIn) : startPast * fadeOut;

            if (muffleDuringWarp)
            {
                // Dips down to muffledCutoff halfway through, then opens back up.
                float dip = Mathf.Sin(k * Mathf.PI);
                float cutoff = Mathf.Lerp(OpenCutoff, muffledCutoff, dip);
                presentFilter.cutoffFrequency = cutoff;
                pastFilter.cutoffFrequency = cutoff;
            }
            yield return null;
        }

        presentSource.volume = targetPresent;
        pastSource.volume = targetPast;
        presentFilter.cutoffFrequency = OpenCutoff;
        pastFilter.cutoffFrequency = OpenCutoff;
        presentFilter.enabled = false;
        pastFilter.enabled = false;
        fadeRoutine = null;

        if (logVolumes)
            Debug.Log($"[WarpAudio] Now in {(inPast ? "Past" : "Present")}. " +
                      $"Present vol {presentSource.volume:0.00} ({(presentSource.clip ? presentSource.clip.name : "NO CLIP")}), " +
                      $"Past vol {pastSource.volume:0.00} ({(pastSource.clip ? pastSource.clip.name : "NO CLIP")}), " +
                      $"playing: {presentSource.isPlaying}/{pastSource.isPlaying}");
    }

    void OnDisable_FinishFade()
    {
        // If this object gets switched off mid-warp, snap to the end state instead of
        // leaving the music half-faded and muffled.
        if (presentSource == null || pastSource == null || presentFilter == null || pastFilter == null) return;
        presentSource.volume = inPast ? 0f : musicVolume;
        pastSource.volume = inPast ? musicVolume : 0f;
        presentFilter.enabled = false;
        pastFilter.enabled = false;
        fadeRoutine = null;
    }

    AudioSource MakeMusicSource(string name, AudioClip clip, out AudioLowPassFilter filter)
    {
        var child = new GameObject(name);
        child.transform.SetParent(transform, false);

        var source = child.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
        source.spatialBlend = 0f;          // plain 2D music: distance to the camera never matters
        source.dopplerLevel = 0f;
        source.priority = 0;               // music is never cut for other sounds

        filter = child.AddComponent<AudioLowPassFilter>();
        filter.cutoffFrequency = OpenCutoff;
        filter.enabled = false;            // only switched on during a warp
        return source;
    }
}
