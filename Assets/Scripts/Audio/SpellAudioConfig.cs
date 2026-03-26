using UnityEngine;

[CreateAssetMenu(fileName = "SpellAudio", menuName = "Audio/SpellAudioConfig")]
public class SpellAudioConfig : ScriptableObject
{
    public string spellName;
    
    [Header("Legacy Launch SFX (fallback release)")]
    public AudioClip launchSFX;
    [Range(0f, 1f)] public float launchVolume = 1f;

    [Header("Launch Start SFX")]
    public AudioClip launchStartSFX;
    [Range(0f, 1f)] public float launchStartVolume = 1f;

    [Header("Launch Release SFX")]
    public AudioClip launchReleaseSFX;
    [Range(0f, 1f)] public float launchReleaseVolume = 1f;
    
    [Header("Impact SFX")]
    public AudioClip impactSFX;
    [Range(0f, 1f)] public float impactVolume = 1f;
    
    [Header("Active Loop SFX")]
    public AudioClip activeLoopSFX;
    [Range(0f, 1f)] public float activeVolume = 1f;
    public bool hasActiveLoop = false;

    private static void EnsureClipLoaded(AudioClip clip)
    {
        if (clip != null && clip.loadState == AudioDataLoadState.Unloaded)
        {
            clip.LoadAudioData();
        }
    }

    // Warm up spell clips so first playback has less latency.
    public void Preload()
    {
        EnsureClipLoaded(launchSFX);
        EnsureClipLoaded(launchStartSFX);
        EnsureClipLoaded(launchReleaseSFX);
        EnsureClipLoaded(impactSFX);
        EnsureClipLoaded(activeLoopSFX);
    }

    public void PlayLaunchStartSFX()
    {
        if (launchStartSFX != null)
        {
            EnsureClipLoaded(launchStartSFX);
            AudioManager.Instance.PlaySFX(launchStartSFX, launchStartVolume);
        }
    }

    public void PlayLaunchReleaseSFX()
    {
        if (launchReleaseSFX != null)
        {
            EnsureClipLoaded(launchReleaseSFX);
            AudioManager.Instance.PlaySFX(launchReleaseSFX, launchReleaseVolume);
            return;
        }

        // Backward compatibility for existing spell configs.
        if (launchSFX != null)
        {
            EnsureClipLoaded(launchSFX);
            AudioManager.Instance.PlaySFX(launchSFX, launchVolume);
        }
    }
    
    public void PlayLaunchSFX()
    {
        // Legacy API maps to release timing.
        PlayLaunchReleaseSFX();
    }
    
    public void PlayImpactSFX()
    {
        if (impactSFX != null)
        {
            EnsureClipLoaded(impactSFX);
            AudioManager.Instance.PlaySFX(impactSFX, impactVolume);
        }
    }
    
    public AudioSource StartActiveLoop()
    {
        if (hasActiveLoop && activeLoopSFX != null)
        {
            EnsureClipLoaded(activeLoopSFX);
            return AudioManager.Instance.StartLoop(activeLoopSFX, activeVolume);
        }
        return null;
    }
}