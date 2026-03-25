using System.Collections.Generic;
using UnityEngine;

public class GG : Sort
{
    private const float FatalDamage = 999999f;

    private void Reset()
    {
        if (string.IsNullOrWhiteSpace(nomSort))
        {
            nomSort = "GG";
        }
    }

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(nomSort))
        {
            nomSort = "GG";
        }
    }

    public override void LancerSort()
    {
        CastDebugWipe();
    }

    public override void LancerSortCible(GameObject cibleRef)
    {
        CastDebugWipe();
    }

    private void CastDebugWipe()
    {
        if (audioConfig != null)
        {
            audioConfig.Preload();
            audioConfig.PlayLaunchSFX();
        }

        if (GameManager.Instance == null)
        {
            DestroySort(gameObject);
            return;
        }

        List<GameManager.EnemyEntry> ennemis = GameManager.Instance.list_enemies;
        if (ennemis == null || ennemis.Count == 0)
        {
            DestroySort(gameObject);
            return;
        }

        for (int i = ennemis.Count - 1; i >= 0; i--)
        {
            GameManager.EnemyEntry entry = ennemis[i];
            if (entry == null || entry.enemy == null) continue;

            Enemy enemy = entry.enemy.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(FatalDamage);
            }
        }

        DestroySort(gameObject);
    }
}