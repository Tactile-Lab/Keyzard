using UnityEngine;
using System.Collections.Generic;

public class ShotGunFeu : Sort
{
    public int nombreProjectiles = 13;
    public float angleTotal = 60f;

    public float delayVie = 0.4f;

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

            mover.Initialiser(direction, vitesse, delayVie);
        }

        Destroy(gameObject);
    }

    // Petite classe interne uniquement utilisée par le shotgun
    private class DeplacementShotgun : MonoBehaviour
{
    private Vector2 direction;
    private float vitesse;
    private float delayVie; // Durée de vie du projectile

    private float timer = 0f;

    // Initialise le projectile avec direction, vitesse et durée de vie
    public void Initialiser(Vector2 dir, float vit, float delay)
    {
        direction = dir.normalized;
        vitesse = vit;
        delayVie = delay;
    }

    void Update()
    {
        // Déplacement
        transform.position += (Vector3)(direction * vitesse * Time.deltaTime);

        // Rotation du projectile selon sa direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Gestion de la durée de vie
        timer += Time.deltaTime;
        if (timer >= delayVie)
        {
            Destroy(gameObject);
        }
    }
}

}

