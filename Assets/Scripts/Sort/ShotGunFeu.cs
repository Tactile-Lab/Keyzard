using UnityEngine;
using System.Collections.Generic;

public class ShotGunFeu : Sort
{
    public int nombreProjectiles = 13;
    public float angleTotal = 60f;

    [Header("Dispersion")]
    public float angleJitter = 2f;

    [Header("Dommages pellets centraux")]
    public float centralDamageMultiplier = 1.25f;
    public int centralProjectileCount = 3;

    public float delayVie = 0.4f;

    [Header("Impact projectile")]
    public string triggerImpact = "Impact";
    public float fallbackDestroyDelay = 1.5f;

    public override void LancerSortCible(GameObject cible)
    {
        if (cible == null)
        {
            DestroySort(cible);
            return;
        }

        // Le shotgun bypass la logique de base de Sort, donc on rejoue explicitement le launch SFX ici.
        if (audioConfig != null)
        {
            audioConfig.Preload();
            audioConfig.PlayLaunchReleaseSFX();
        }

        int baseDamage = damage;

        Vector2 directionBase = (cible.transform.position - transform.position).normalized;

        float angleBase = Mathf.Atan2(directionBase.y, directionBase.x) * Mathf.Rad2Deg;

        float angleDepart = angleBase - angleTotal / 2f;

        float pas = 0;

        if (nombreProjectiles > 1)
            pas = angleTotal / (nombreProjectiles - 1);

        for (int i = 0; i < nombreProjectiles; i++)
        {
            float randomSpread = Random.Range(-angleJitter, angleJitter);
            float angle = angleDepart + pas * i + randomSpread;

            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            Vector3 spawnPos = transform.position;
            spawnPos.z = 0f;
            GameObject proj = Instantiate(gameObject, spawnPos, Quaternion.identity);

            // Réutilise le composant du prefab s'il existe déjà pour éviter les doublons.
            DeplacementShotgun mover = proj.GetComponent<DeplacementShotgun>();
            if (mover == null)
            {
                mover = proj.AddComponent<DeplacementShotgun>();
            }

            ShotGunFeu projSort = proj.GetComponent<ShotGunFeu>();
            if (projSort != null)
            {
                float centerMultiplier = GetCenterDamageMultiplier(i);
                projSort.damage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * centerMultiplier));
            }

            mover.Initialiser(direction, vitesse, delayVie, triggerImpact, fallbackDestroyDelay);
        }

        if (activeLoopSource != null)
        {
            AudioManager.Instance.StopLoop(activeLoopSource);
            activeLoopSource = null;
        }

        Destroy(gameObject);
    }

    public override void DestroySort(GameObject cible)
    {
        DeplacementShotgun mover = GetComponent<DeplacementShotgun>();

        // Les pellets du shotgun jouent une anim d'impact avant destruction.
        if (mover != null && cible != null && cible != gameObject)
        {
            OnImpact(cible);
            mover.DemarrerImpact(cible);
            return;
        }

        base.DestroySort(cible);
    }

    private float GetCenterDamageMultiplier(int projectileIndex)
    {
        if (centralProjectileCount <= 0 || centralDamageMultiplier <= 1f)
        {
            return 1f;
        }

        int centerIndex = (nombreProjectiles - 1) / 2;
        int halfWindow = Mathf.Max(0, (centralProjectileCount - 1) / 2);

        return Mathf.Abs(projectileIndex - centerIndex) <= halfWindow
            ? centralDamageMultiplier
            : 1f;
    }

}

