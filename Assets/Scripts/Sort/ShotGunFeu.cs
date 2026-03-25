using UnityEngine;
using System.Collections.Generic;

public class ShotGunFeu : Sort
{
    public int nombreProjectiles = 13;
    public float angleTotal = 60f;

    public float delayVie = 0.4f;

    [Header("Impact projectile")]
    public string triggerImpact = "Impact";
    public float fallbackDestroyDelay = 0.12f;

    public override void LancerSortCible(GameObject cible)
    {
        Vector2 directionBase = (cible.transform.position - transform.position).normalized;

        float angleBase = Mathf.Atan2(directionBase.y, directionBase.x) * Mathf.Rad2Deg;

        float angleDepart = angleBase - angleTotal / 2f;

        float pas = 0;

        if (nombreProjectiles > 1)
            pas = angleTotal / (nombreProjectiles - 1);

        for (int i = 0; i < nombreProjectiles; i++)
        {
            float angle = angleDepart + pas * i;

            Vector2 direction = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            );

            GameObject proj = Instantiate(gameObject, transform.position, Quaternion.identity);

            // Ici on ajoute un petit composant local qui gère le déplacement
            DeplacementShotgun mover = proj.AddComponent<DeplacementShotgun>();

            mover.Initialiser(direction, vitesse, delayVie, triggerImpact, fallbackDestroyDelay);
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
            mover.DemarrerImpact();
            return;
        }

        base.DestroySort(cible);
    }

}

