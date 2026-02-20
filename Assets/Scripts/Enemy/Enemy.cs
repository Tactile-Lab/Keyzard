using UnityEngine;
using System.Collections;
using TMPro;

public class Enemy : MonoBehaviour
{
    private float health;
    private EnemyType type;
    private int damage;
    private float speed;
    private Animator animator;

    [SerializeField]
    private GameObject projectileEnemy;
    private GameObject projectileToLaunch;

    private bool isMoving;
    public TMP_Text nameText;

    [SerializeField]
    private EnemyData ennemyData;

    private GameObject player;
    private PlayerHealth playerHealth;
    private Collider2D playerCollider;

    private bool isStunned = false;

    private Rigidbody2D rb;
    private Collider2D mainCollider;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerCollider = player.GetComponent<Collider2D>();
        }
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (ennemyData != null)
        {
            health = ennemyData.health;
            type = ennemyData.type;
            damage = ennemyData.damage;
            speed = ennemyData.speed;
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = ennemyData.animatorController;
        }

        nameText.text = GameManager.Instance.list_enemies.Find(e => e.enemy == gameObject)?.code ?? "Unknown";
        if (type != EnemyType.Distant)
        {
            isMoving = true;
            animator.SetTrigger("Move");
        }
        else
        {
            StartCoroutine(Shoot());
        }
    }

    private void FixedUpdate()
    {
        if (isStunned) return;

        Move();
        CheckContactDamage();
    }

    private void CheckContactDamage()
    {
        if (health == 0 || mainCollider == null || playerCollider == null) return;

        ColliderDistance2D distanceInfo = mainCollider.Distance(playerCollider);
        if (distanceInfo.isOverlapped)
        {
            TryDamagePlayer(playerCollider.gameObject);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        isStunned = true;

        // Stop le mouvement actuel
        rb.linearVelocity = Vector2.zero;

        // Applique le knockback
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        // Commence une coroutine pour stopper la velocity après un court instant
        StartCoroutine(KnockbackDuration(0.3f));
    }

    private IEnumerator KnockbackDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Stop la velocity pour éviter que le mob continue en arrière
        rb.linearVelocity = Vector2.zero;

        // Fin du stun
        isStunned = false;
    }

    private IEnumerator Shoot()
    {
        while (true)
        {
            yield return new WaitForSeconds(4f);
            if (health == 0) yield break;
            animator.SetTrigger("Attack");
        }
    }

    public void ShootProjectile() //triggered dans l'animation d'attaque du distant
    {
        if (player == null) return;

        if (player.transform.position.x < transform.position.x)
        {
            projectileToLaunch = Instantiate(projectileEnemy, transform.position + Vector3.left * 0.4f, Quaternion.identity);
        }
        else
        {
            projectileToLaunch = Instantiate(projectileEnemy, transform.position + Vector3.right * 0.4f, Quaternion.identity);
        }
    }

    public void LaunchProjectile()
    {
        if (projectileToLaunch == null || player == null) return;

        projectileToLaunch.GetComponent<ProjectileDistant>().Launch(player.transform.position, damage);
    }

    private void Move()
    {
        if (health == 0 || player == null) return;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (player.transform.position.x < transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;

        if (type == EnemyType.Distant) return;

        CheckAttack();
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed / 4 * Time.deltaTime);
        }
    }

    private void CheckAttack()
    {
        if (Vector2.Distance(transform.position, player.transform.position) < 1f)
        {
            isMoving = false;
            animator.SetTrigger("Attack");
        }
    }

    public void EndAttack()
    {
        isMoving = true;
        animator.SetTrigger("Move");
    }

    public void DealDamageToPlayer()
    {
        if (playerHealth == null || health == 0)
        {
            return;
        }

        if (Vector2.Distance(transform.position, player.transform.position) <= 1.2f)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer(other.gameObject);
        }

        if (other.CompareTag("Projectile"))
        {
            Sort sort = other.GetComponent<Sort>();
            if (sort != null)
            {
                sort.DestroySort(gameObject);
                TakeDamage(sort.damage);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TryDamagePlayer(other.gameObject);
        }
    }

    private void TryDamagePlayer(GameObject playerObject)
    {
        if (health == 0)
        {
            return;
        }

        PlayerHealth targetHealth = playerObject.GetComponent<PlayerHealth>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        animator.SetTrigger("TakeDmg");
        isMoving = false;
        Debug.Log(health + " " + damageAmount + " " + (health - damageAmount));
        health -= damageAmount;
        if (health < 0)
        {
            health = 0;
        }
    }

    public void EndTakeDamage()
    {
        if (health == 0)
        {
            GameManager.Instance.list_enemies.RemoveAll(e => e.enemy == gameObject);
            animator.SetTrigger("Death");
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            isMoving = true;
            animator.SetTrigger("Move");
        }
    }

    public void EndDeath()
    {
        Destroy(gameObject);
    }
}
