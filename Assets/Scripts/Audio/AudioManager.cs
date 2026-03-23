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
    
    [Header("Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Header("Music Muffle")]
    [Tooltip("Cutoff normal de la musique (Hz).")]
    [Min(10f)] public float normalMusicCutoff = 22000f;
    [Tooltip("Cutoff quand le glossaire est ouvert (Hz).")]
    [Min(10f)] public float muffledMusicCutoff = 900f;
    
    private AudioSource musicSource;
    private AudioLowPassFilter musicLowPass;
    private Coroutine muffleRoutine;
    private List<AudioSource> sfxPool = new List<AudioSource>();
    private List<AudioSource> loopPool = new List<AudioSource>();
    private const int INITIAL_POOL_SIZE = 5;
    
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
        if (musicGroup != null)
        {
            float dB = volume > 0 ? 20f * Mathf.Log10(volume) : -80f;
            musicGroup.audioMixer.SetFloat("MusicVolume", dB);
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