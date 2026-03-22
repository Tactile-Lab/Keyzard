using UnityEngine;
using System.Collections;

public class ProjectileDistant : MonoBehaviour
{
    private int damage;
    [SerializeField] private float speed = 5f;
    [SerializeField] private Vector3 finalScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float initialScaleValue = 0.3f; // scale au spawn
    [SerializeField] private float growSpeed = 1.5f; // calculé pour frames 4->11 speed 0.25

    private bool isMoving;
    private Vector2 directionBase;

    void Start()
    {
        // commence visible mais petit
        transform.localScale = new Vector3(initialScaleValue, initialScaleValue, initialScaleValue);
        StartGrowth();
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position += (Vector3)directionBase * speed * Time.deltaTime;
        }
    }

    // démarre la croissance
    public void StartGrowth()
    {
        StartCoroutine(Grow());
    }

    private IEnumerator Grow()
    {
        Vector3 startScale = new Vector3(initialScaleValue, initialScaleValue, initialScaleValue);

        while (transform.localScale.magnitude < finalScale.magnitude)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, finalScale, growSpeed * Time.deltaTime);
            yield return null;
        }

        transform.localScale = finalScale;
    }

    // lancer le projectile vers le joueur (appelé depuis Enemy)
    public void Launch(Vector3 playerPos, int projectileDamage)
    {
        damage = projectileDamage;
        directionBase = (playerPos - transform.position).normalized;
        isMoving = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>() 
                                 ?? other.GetComponentInParent<PlayerHealth>() 
                                 ?? other.GetComponentInChildren<PlayerHealth>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}