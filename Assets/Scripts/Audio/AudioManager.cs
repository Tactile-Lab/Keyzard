using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Mixer Groups")]
    public AudioMixerGroup masterGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    
    [Header("Music Config")]
    public MusicAudioConfig musicConfig;

    [Header("Music (Legacy)")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Header("Music Transitions")]
    [SerializeField] private float defaultMusicSwitchFade = 0.2f;
    [SerializeField] private float sceneTransitionMusicFade = 0.3f;
    [Header("Music Muffle")]
    [Tooltip("Cutoff normal de la musique (Hz).")]
    [Min(10f)] public float normalMusicCutoff = 22000f;
    [Tooltip("Cutoff quand le glossaire est ouvert (Hz).")]
    [Min(10f)] public float muffledMusicCutoff = 900f;
    
    private AudioSource musicSource;
    private AudioLowPassFilter musicLowPass;
    private Coroutine muffleRoutine;
    private Coroutine musicSwitchRoutine;
    private Coroutine transitionMusicFadeRoutine;
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private List<AudioSource> loopPool = new List<AudioSource>();
    private const int INITIAL_POOL_SIZE = 5;

    private GameMusicState _currentMusicState;
    private float _currentStateVolume = 1f;
    private readonly Dictionary<GameMusicState, float> _musicTimestamps = new Dictionary<GameMusicState, float>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        PlayBackgroundMusic();
    }
    
    private void InitializeAudioSources()
    {
        // Créer la source de musique
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        // La musique continue même si AudioListener.pause = true.
        musicSource.ignoreListenerPause = true;

        musicLowPass = gameObject.GetComponent<AudioLowPassFilter>();
        if (musicLowPass == null)
        {
            musicLowPass = gameObject.AddComponent<AudioLowPassFilter>();
        }
        musicLowPass.cutoffFrequency = normalMusicCutoff;
        
        // Initialiser le pool SFX
        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            CreateSFXSource();
            CreateLoopSource();
        }
    }
    
    private AudioSource CreateSFXSource()
    {
        var sourceObject = new GameObject($"SFXSource_{sfxPool.Count}");
        sourceObject.transform.SetParent(transform, false);
        // Disable object so AddComponent doesn't trigger PlayOnAwake before configuration.
        sourceObject.SetActive(false);

        var source = sourceObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxGroup;
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;

        sourceObject.SetActive(true);
        sfxPool.Add(source);
        return source;
    }
    
    private AudioSource CreateLoopSource()
    {
        var sourceObject = new GameObject($"LoopSource_{loopPool.Count}");
        sourceObject.transform.SetParent(transform, false);
        // Disable object so AddComponent doesn't trigger PlayOnAwake before configuration.
        sourceObject.SetActive(false);

        var source = sourceObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxGroup;
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.dopplerLevel = 0f;

        sourceObject.SetActive(true);
        loopPool.Add(source);
        return source;
    }
    
    private AudioSource GetAvailableSFXSource()
    {
        foreach (var source in sfxPool)
        {
            if (!source.isPlaying) return source;
        }
        
        // Si toutes les sources sont utilisées, en créer une nouvelle
        return CreateSFXSource();
    }
    
    private AudioSource GetAvailableLoopSource()
    {
        foreach (var source in loopPool)
        {
            if (!source.isPlaying) return source;
        }
        
        // Si toutes les sources sont utilisées, en créer une nouvelle
        return CreateLoopSource();
    }
    
    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }
    }

    public void PlayMusic(GameMusicState state)
    {
        PlayMusic(state, defaultMusicSwitchFade);
    }

    public void PlayMusic(GameMusicState state, float fadeDuration)
    {
        if (musicSource == null || musicConfig == null)
        {
            return;
        }

        MusicAudioEntry nextEntry = musicConfig.GetEntry(state);
        if (nextEntry == null || nextEntry.clip == null)
        {
            Debug.LogWarning($"[AudioManager] Aucun clip configure pour l'etat {state}.");
            return;
        }

        if (musicSource.clip == nextEntry.clip && _currentMusicState == state && musicSource.isPlaying)
        {
            _currentStateVolume = Mathf.Clamp01(nextEntry.volume);
            musicSource.volume = GetEffectiveMusicVolume();
            return;
        }

        if (musicSwitchRoutine != null)
        {
            StopCoroutine(musicSwitchRoutine);
        }

        if (transitionMusicFadeRoutine != null)
        {
            StopCoroutine(transitionMusicFadeRoutine);
            transitionMusicFadeRoutine = null;
        }

        musicSwitchRoutine = StartCoroutine(SwitchMusicRoutine(state, nextEntry, Mathf.Max(0f, fadeDuration)));
    }

    public void BeginSceneTransitionAudioFadeOut(float duration = -1f)
    {
        if (musicSource == null || !musicSource.isPlaying)
        {
            return;
        }

        if (musicSwitchRoutine != null)
        {
            StopCoroutine(musicSwitchRoutine);
            musicSwitchRoutine = null;
        }

        if (transitionMusicFadeRoutine != null)
        {
            StopCoroutine(transitionMusicFadeRoutine);
        }

        float fade = duration >= 0f ? duration : sceneTransitionMusicFade;
        transitionMusicFadeRoutine = StartCoroutine(FadeMusicVolumeCoroutine(0f, Mathf.Max(0f, fade), () => transitionMusicFadeRoutine = null));
    }

    public void EndSceneTransitionAudioFadeIn(float duration = -1f)
    {
        if (musicSource == null || musicSource.clip == null)
        {
            return;
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

        if (transitionMusicFadeRoutine != null)
        {
            StopCoroutine(transitionMusicFadeRoutine);
        }

        float fade = duration >= 0f ? duration : sceneTransitionMusicFade;
        transitionMusicFadeRoutine = StartCoroutine(FadeMusicVolumeCoroutine(GetEffectiveMusicVolume(), Mathf.Max(0f, fade), () => transitionMusicFadeRoutine = null));
    }

    public void ResetMusicRuntime(bool stopMusic = true)
    {
        _musicTimestamps.Clear();
        _currentStateVolume = 1f;

        if (musicSwitchRoutine != null)
        {
            StopCoroutine(musicSwitchRoutine);
            musicSwitchRoutine = null;
        }

        if (transitionMusicFadeRoutine != null)
        {
            StopCoroutine(transitionMusicFadeRoutine);
            transitionMusicFadeRoutine = null;
        }

        if (musicSource == null)
        {
            return;
        }

        if (stopMusic)
        {
            musicSource.Stop();
            musicSource.time = 0f;
            musicSource.clip = null;
        }

        musicSource.volume = GetEffectiveMusicVolume();
    }

    private IEnumerator SwitchMusicRoutine(GameMusicState state, MusicAudioEntry entry, float fadeDuration)
    {
        PrewarmClip(entry.clip);

        if (musicSource.isPlaying)
        {
            MusicAudioEntry currentEntry = musicConfig.GetEntry(_currentMusicState);
            if (currentEntry != null && currentEntry.persistInBackground)
            {
                _musicTimestamps[_currentMusicState] = musicSource.time;
            }
        }

        float halfFade = fadeDuration * 0.5f;
        if (musicSource.isPlaying && halfFade > 0f)
        {
            yield return StartCoroutine(FadeMusicVolumeCoroutine(0f, halfFade));
        }

        musicSource.Stop();
        musicSource.clip = entry.clip;
        _currentMusicState = state;
        _currentStateVolume = Mathf.Clamp01(entry.volume);

        if (entry.persistInBackground && _musicTimestamps.TryGetValue(state, out float savedTime))
        {
            musicSource.time = Mathf.Min(savedTime, Mathf.Max(0f, entry.clip.length - 0.01f));
        }
        else
        {
            musicSource.time = 0f;
        }

        musicSource.volume = halfFade > 0f ? 0f : GetEffectiveMusicVolume();
        musicSource.Play();

        if (halfFade > 0f)
        {
            yield return StartCoroutine(FadeMusicVolumeCoroutine(GetEffectiveMusicVolume(), halfFade));
        }

        musicSwitchRoutine = null;
    }

    private void PrewarmClip(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (clip.loadState == AudioDataLoadState.Unloaded)
        {
            clip.LoadAudioData();
        }
    }

    private float GetEffectiveMusicVolume()
    {
        return Mathf.Clamp01(musicVolume) * Mathf.Clamp01(_currentStateVolume);
    }

    private IEnumerator FadeMusicVolumeCoroutine(float targetVolume, float duration, System.Action onComplete = null)
    {
        if (musicSource == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        targetVolume = Mathf.Clamp01(targetVolume);
        duration = Mathf.Max(0f, duration);

        if (duration <= 0f)
        {
            musicSource.volume = targetVolume;
            onComplete?.Invoke();
            yield break;
        }

        float start = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicSource.volume = Mathf.Lerp(start, targetVolume, t);
            yield return null;
        }

        musicSource.volume = targetVolume;
        onComplete?.Invoke();
    }
    
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        
        var source = GetAvailableSFXSource();
        source.volume = 1f;
        source.PlayOneShot(clip, volume);
    }
    
    public AudioSource StartLoop(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return null;
        
        var source = GetAvailableLoopSource();
        source.clip = clip;
        source.volume = volume;
        source.Play();
        return source;
    }
    
    public void StopLoop(AudioSource loopSource)
    {
        if (loopSource != null && loopSource.isPlaying)
        {
            loopSource.Stop();
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        if (masterGroup != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            masterGroup.audioMixer.SetFloat("MasterVolume", dB);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicGroup != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            musicGroup.audioMixer.SetFloat("MusicVolume", dB);
        }

        if (musicSource != null && musicSwitchRoutine == null && transitionMusicFadeRoutine == null)
        {
            musicSource.volume = GetEffectiveMusicVolume();
        }
    }
    
    public void SetSFXVolume(float volume)
    {
        if (sfxGroup != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            sfxGroup.audioMixer.SetFloat("SFXVolume", dB);
        }
    }

    public void SetGameplayAudioPaused(bool paused)
    {
        AudioListener.pause = paused;
    }

    public void SetMusicMuffled(bool isMuffled, float transitionDuration = 0.2f)
    {
        if (musicLowPass == null) return;

        float targetCutoff = isMuffled ? muffledMusicCutoff : normalMusicCutoff;
        transitionDuration = Mathf.Max(0f, transitionDuration);

        if (muffleRoutine != null)
        {
            StopCoroutine(muffleRoutine);
        }

        if (transitionDuration <= 0f)
        {
            musicLowPass.cutoffFrequency = targetCutoff;
            return;
        }

        muffleRoutine = StartCoroutine(AnimateMusicCutoff(targetCutoff, transitionDuration));
    }

    private IEnumerator AnimateMusicCutoff(float targetCutoff, float duration)
    {
        float startCutoff = musicLowPass.cutoffFrequency;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            musicLowPass.cutoffFrequency = Mathf.Lerp(startCutoff, targetCutoff, t);
            yield return null;
        }

        musicLowPass.cutoffFrequency = targetCutoff;
        muffleRoutine = null;
    }
}