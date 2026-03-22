using UnityEngine;

[CreateAssetMenu(fileName = "SpellAudio", menuName = "Audio/SpellAudioConfig")]
public class SpellAudioConfig : ScriptableObject
{
    public string spellName;
    
    [Header("Launch SFX")]
    public AudioClip launchSFX;
    [Range(0f, 1f)] public float launchVolume = 1f;
    
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
        EnsureClipLoaded(impactSFX);
        EnsureClipLoaded(activeLoopSFX);
    }
    
    public void PlayLaunchSFX()
    {
        if (launchSFX != null)
        {
            EnsureClipLoaded(launchSFX);
            AudioManager.Instance.PlaySFX(launchSFX, launchVolume);
        }
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