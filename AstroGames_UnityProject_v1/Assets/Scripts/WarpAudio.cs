using System.Collections;
using UnityEngine;

// Plays a Present and a Past version of the music in sync and crossfades
// between them when the player warps. Also plays a warp sound effect and
// briefly muffles the music during the transition.
//
// Setup: put this on an empty GameObject (e.g. "AudioManager") in the scene and
// drag in the clips. It listens to TimeWarpManager.TimeSwappedEvent, so warps
// trigger it automatically. Set "Start In Past" to match TimeWarpManager.
/// <summary>
/// Crossfades past/present music and plays effects in response to warp and death events.
/// </summary>
/// <remarks>
/// Listens to TimeWarpManager.TimeSwappedEvent and PlayerRespawn.PlayerDiedEvent. Creates audio sources and filters for music and effects.
/// </remarks>
/// <seealso cref="TimeWarpManager"/>
/// <seealso cref="PlayerRespawn"/>
public class WarpAudio : MonoBehaviour
{
    /// <summary>
    /// Current instance, assigned in Awake and cleared when that instance is destroyed.
    /// </summary>
    public static WarpAudio Instance { get; private set; }

    /// <summary>
    /// Music clip for the present timeline; matching the past track length and tempo is recommended.
    /// </summary>
    [Header("Music (both clips should be the same length and tempo)")]
    public AudioClip presentMusic;
    /// <summary>
    /// Music clip for the past timeline, started in sync with presentMusic.
    /// </summary>
    public AudioClip pastMusic;
    /// <summary>
    /// Target volume of the currently active timeline music.
    /// </summary>
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    /// <summary>
    /// Duration of a timeline music transition in unscaled seconds.
    /// </summary>
    public float crossfadeTime = 0.6f;

    /// <summary>
    /// One-shot effect played when changing to the past.
    /// </summary>
    [Header("Warp sound effects")]
    public AudioClip warpToPastSfx;
    /// <summary>
    /// One-shot effect played when changing to the present.
    /// </summary>
    public AudioClip warpToPresentSfx;
    /// <summary>
    /// Volume multiplier for warp sound effects.
    /// </summary>
    [Range(0f, 1f)] public float sfxVolume = 1f;
    /// <summary>
    /// Maximum random pitch offset applied to warp and death effects.
    /// </summary>
    [Tooltip("Random pitch change per warp so a repeated sound doesn't get tiring. 0.03 = +/-3%.")]
    [Range(0f, 0.1f)] public float pitchVariation = 0.03f;

    /// <summary>
    /// Optional one-shot effect played when PlayerRespawn reports a death.
    /// </summary>
    [Header("Death sound")]
    public AudioClip deathSfx;
    /// <summary>
    /// Volume multiplier for the death sound effect.
    /// </summary>
    [Range(0f, 1f)] public float deathVolume = 0.9f;

    /// <summary>
    /// Whether low-pass filters temporarily muffle the music during transitions.
    /// </summary>
    [Header("Muffle the music during the warp")]
    public bool muffleDuringWarp = true;
    /// <summary>
    /// Low-pass cutoff frequency in hertz at the midpoint of a muffled transition.
    /// </summary>
    public float muffledCutoff = 800f; // lower = more muffled

    /// <summary>
    /// Initial audio timeline; set to match TimeWarpManager.startInPast.
    /// </summary>
    [Header("Start state")]
    public bool startInPast = false;

    /// <summary>
    /// Whether completed crossfades write timeline and audio-source details to the Console.
    /// </summary>
    [Header("Debug")]
    [Tooltip("Print music volumes to the Console after every warp.")]
    public bool logVolumes = true;

    /// <summary>
    /// Unmuffled filter cutoff frequency in hertz.
    /// </summary>
    const float OpenCutoff = 22000f;

    /// <summary>
    /// Audio sources for present music, past music, and one-shot effects.
    /// </summary>
    AudioSource presentSource, pastSource, sfxSource;
    /// <summary>
    /// Filters applied temporarily to the two timeline music sources.
    /// </summary>
    AudioLowPassFilter presentFilter, pastFilter;
    /// <summary>
    /// Handle for the active music crossfade coroutine.
    /// </summary>
    Coroutine fadeRoutine;
    /// <summary>
    /// Current audio timeline, updated by Warp.
    /// </summary>
    bool inPast;

    /// <summary>
    /// True when the audio state targets the past timeline.
    /// </summary>
    public bool InPast => inPast;

    /// <summary>
    /// Enforces a single current instance and creates music sources, filters, and the effect source.
    /// </summary>
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

    /// <summary>
    /// Sets initial music volumes and schedules both assigned music clips on the same audio clock tick.
    /// </summary>
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

    /// <summary>
    /// Clears Instance when this component owns the current instance reference.
    /// </summary>
    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // Listen to TimeWarpManager: it fires TimeSwappedEvent(inPast) after every successful swap,
    // so the music and warp sound follow the game without editing TimeWarpManager.
    /// <summary>
    /// Subscribes to successful timeline swaps and player death notifications.
    /// </summary>
    void OnEnable()
    {
        TimeWarpManager.TimeSwappedEvent += Warp;
        PlayerRespawn.PlayerDiedEvent += PlayDeath;
    }

    /// <summary>
    /// Unsubscribes from gameplay events and sets final music volumes and filter states.
    /// </summary>
    void OnDisable()
    {
        TimeWarpManager.TimeSwappedEvent -= Warp;
        PlayerRespawn.PlayerDiedEvent -= PlayDeath;
        OnDisable_FinishFade();
    }

    // Called by PlayerRespawn every time the player dies (hit or fell off the level).
    /// <summary>
    /// Plays the configured death effect with pitch variation, if a clip is assigned.
    /// </summary>
    /// <seealso cref="PlayerRespawn.PlayerDiedEvent"/>
    public void PlayDeath()
    {
        if (deathSfx == null) return;
        sfxSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        sfxSource.PlayOneShot(deathSfx, deathVolume);
    }

    // Call with true to go to the Past, false to go to the Present.
    /// <summary>
    /// Changes the audio timeline, plays its warp effect, and crossfades music without switching gameplay objects.
    /// </summary>
    /// <param name="toPast">True to target past music; false to target present music.</param>
    /// <seealso cref="TimeWarpManager.TimeSwappedEvent"/>
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

    /// <summary>
    /// Toggles the audio timeline only by calling Warp with the opposite state.
    /// </summary>
    public void ToggleWarp()
    {
        Warp(!inPast);
    }

    /// <summary>
    /// Blends music volumes and optional filter cutoffs over crossfadeTime using unscaled frame time.
    /// </summary>
    /// <returns>Coroutine iterator yielding once per transition frame.</returns>
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

    /// <summary>
    /// Sets music volumes to the current audio timeline and disables filters if sources are initialized.
    /// </summary>
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

    /// <summary>
    /// Creates a child object with a looping 2D music source and a disabled low-pass filter.
    /// </summary>
    /// <param name="name">Name assigned to the child GameObject.</param>
    /// <param name="clip">Music clip assigned to the new source.</param>
    /// <param name="filter">Receives the low-pass filter created on the child.</param>
    /// <returns>The newly created, non-autoplay AudioSource.</returns>
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

/*! \var AudioSource WarpAudio::pastSource
 *  \brief Audio source for past music.
 */

/*! \var AudioSource WarpAudio::sfxSource
 *  \brief Audio source for warp and death effects.
 */

/*! \var AudioLowPassFilter WarpAudio::pastFilter
 *  \brief Filter used to muffle past music during a warp.
 */
