using UnityEngine;

public class RingEau : Sort
{

    public float forceKnockback = 5f;

    public override void LancerSort()
    {

        LancerSortCible(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            if (enemy != null)
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;

                enemy.ApplyKnockback(direction, forceKnockback);
            }
        }
    }



    public void OnAnimationFinished()
    {
        Destroy(gameObject);
    }

    public override void DestroySort(GameObject cible)
    {
        return;
    }

    
}
