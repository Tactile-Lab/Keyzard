using UnityEngine;

[CreateAssetMenu(fileName = "TypingSFXConfig", menuName = "Audio/TypingSFXConfig")]
public class TypingSFXConfig : DomainSFXAudioConfig
{
    public override bool SupportsKey(SFXEventKey key)
    {
        switch (key)
        {
            case SFXEventKey.TypingHit:
            case SFXEventKey.TypingMiss:
            case SFXEventKey.TypingWordFail:
            case SFXEventKey.TypingTargetLock:
            case SFXEventKey.TypingSpellReady:
            case SFXEventKey.TypingSpellCleared:
                return true;
            default:
                return false;
        }
    }
}