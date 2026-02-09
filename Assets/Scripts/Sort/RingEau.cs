using UnityEngine;

public class RingEau : Sort
{

    public float forceKnockback = 5f;

    public override void LancerSort()
    {

        LancerSortCible(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Rigidbody2D rb = collision.gameObject.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                Vector2 direction = (collision.transform.position - transform.position).normalized;

                rb.AddForce(direction * forceKnockback, ForceMode2D.Impulse);
            }

            DestroySort(collision.gameObject);
        }
    }

    public void OnAnimationFinished()
    {
        Destroy(gameObject);
    }

    
}
