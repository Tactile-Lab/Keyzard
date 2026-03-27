using System.Collections.Generic;
using UnityEngine;

public enum SFXEventKey
{
    None,

    // UI
    UIMenuMove,
    UIMenuConfirm,
    UIOpen,
    UIClose,
    UISceneTransition,
    UIGlossaryOpen,
    UIGlossaryClose,

    // Typing
    TypingHit,
    TypingMiss,
    TypingWordFail,
    TypingTargetLock,
    TypingSpellReady,
    TypingSpellCleared,

    // Player
    PlayerFootstep,
    PlayerHurt,
    PlayerDeath,

    // Enemy generic
    EnemyAttack,
    EnemyHurt,
    EnemyDeath,
    EnemyFootstep,

    // Enemy by type
    EnemyRapideAttack,
    EnemyRapideHurt,
    EnemyRapideDeath,
    EnemyRapideFootstep,
    EnemyLourdAttack,
    EnemyLourdHurt,
    EnemyLourdDeath,
    EnemyLourdFootstep,
    EnemyDistantAttack,
    EnemyDistantHurt,
    EnemyDistantDeath,

    // Progress / misc
    RoomClear,
    NewSpellUnlocked,
    BookPageTurn,
    EndDemoOpen,
    EndDemoConfirm
}

[System.Serializable]
public class SFXEventAudioEntry
{
    public SFXEventKey key;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(-3f, 3f)] public float pitch = 1f;
    [Range(0f, 0.5f)] public float randomPitchVariance = 0f;
}

[CreateAssetMenu(fileName = "SFXEventAudioConfig", menuName = "Audio/SFXEventAudioConfig")]
public class SFXEventAudioConfig : ScriptableObject
{
    public List<SFXEventAudioEntry> entries = new List<SFXEventAudioEntry>();

    public SFXEventAudioEntry GetEntry(SFXEventKey key)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            SFXEventAudioEntry entry = entries[i];
            if (entry != null && entry.key == key)
            {
                return entry;
            }
        }

        return null;
    }
}