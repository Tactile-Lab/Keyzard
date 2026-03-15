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
    
    public void PlayLaunchSFX()
    {
        if (launchSFX != null)
        {
            AudioManager.Instance.PlaySFX(launchSFX, launchVolume);
        }
    }
    
    public void PlayImpactSFX()
    {
        if (impactSFX != null)
        {
            AudioManager.Instance.PlaySFX(impactSFX, impactVolume);
        }
    }
    
    public AudioSource StartActiveLoop()
    {
        if (hasActiveLoop && activeLoopSFX != null)
        {
            return AudioManager.Instance.StartLoop(activeLoopSFX, activeVolume);
        }
        return null;
    }
}