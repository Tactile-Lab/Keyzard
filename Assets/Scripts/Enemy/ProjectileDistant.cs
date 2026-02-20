using UnityEngine;

public class ProjectileDistant : MonoBehaviour
{
    private int damage;
    [SerializeField]
    private float speed;

    private bool isMoving;
    private Vector2 directionBase;


    void Start()
    {
        transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
    }


    void FixedUpdate()
    {
        if (isMoving)
        {
            transform.position += new Vector3(directionBase.x, directionBase.y, 0) * speed * Time.deltaTime;
        }
        else
        {
            transform.localScale = transform.localScale + new Vector3(0.01f, 0.01f, 0.01f);
        }
    }

    public void Launch(Vector3 playerPos, int projectileDamage)
    {
        transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        directionBase = (playerPos - transform.position).normalized;
        damage = projectileDamage;
        isMoving = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
