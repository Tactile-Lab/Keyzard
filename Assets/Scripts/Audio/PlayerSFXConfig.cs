using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSFXConfig", menuName = "Audio/PlayerSFXConfig")]
public class PlayerSFXConfig : DomainSFXAudioConfig
{
    public override bool SupportsKey(SFXEventKey key)
    {
        switch (key)
        {
            case SFXEventKey.PlayerFootstep:
            case SFXEventKey.PlayerHurt:
            case SFXEventKey.PlayerDeath:
                return true;
            default:
                return false;
        }
    }
}