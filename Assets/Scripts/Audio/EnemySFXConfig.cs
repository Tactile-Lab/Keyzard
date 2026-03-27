using UnityEngine;

[CreateAssetMenu(fileName = "EnemySFXConfig", menuName = "Audio/EnemySFXConfig")]
public class EnemySFXConfig : DomainSFXAudioConfig
{
    public override bool SupportsKey(SFXEventKey key)
    {
        switch (key)
        {
            case SFXEventKey.EnemyAttack:
            case SFXEventKey.EnemyHurt:
            case SFXEventKey.EnemyDeath:
            case SFXEventKey.EnemyFootstep:
            case SFXEventKey.EnemyRapideAttack:
            case SFXEventKey.EnemyRapideHurt:
            case SFXEventKey.EnemyRapideDeath:
            case SFXEventKey.EnemyRapideFootstep:
            case SFXEventKey.EnemyLourdAttack:
            case SFXEventKey.EnemyLourdHurt:
            case SFXEventKey.EnemyLourdDeath:
            case SFXEventKey.EnemyLourdFootstep:
            case SFXEventKey.EnemyDistantAttack:
            case SFXEventKey.EnemyDistantHurt:
            case SFXEventKey.EnemyDistantDeath:
                return true;
            default:
                return false;
        }
    }
}