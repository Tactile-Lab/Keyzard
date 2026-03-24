using UnityEngine;
using System.Collections;

public class ProjectileDistant : MonoBehaviour
{
    private int damage;
    [SerializeField] private float speed = 5f;
    [SerializeField] private Vector3 finalScale = new Vector3(1f, 1f, 1f);
    [SerializeField] private float initialScaleValue = 0.3f; // scale au spawn
    [SerializeField] private float growSpeed = 1.5f; // calculé pour frames 4->11 speed 0.25

    [SerializeField] private LayerMask wallLayer; // assigné dans l'inspecteur

    private bool isMoving;

    private bool islauch = false;
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
            if (directionBase == Vector2.zero)
            {
                Destroy(gameObject);
                return;
            }

            float moveDistance = speed * Time.fixedDeltaTime;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionBase, moveDistance, wallLayer);
            if (hit.collider != null)
            {
                Destroy(gameObject);
                return;
            }

            transform.position += (Vector3)(directionBase * moveDistance);
        }
        else if (islauch)
        {
            Destroy(gameObject);
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

    public void Launch(Vector3 targetPosition, int projectileDamage)
    {
        Vector2 dir = targetPosition - transform.position;

        if (dir == Vector2.zero)
        {
            // cible disparue ou trop proche
            Destroy(gameObject);
            return;
        }

        directionBase = dir.normalized;
        damage = projectileDamage;
        isMoving = true;
        islauch = true;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        // Gestion joueur
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