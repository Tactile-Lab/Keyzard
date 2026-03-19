using UnityEngine;
using System.Collections;
using TMPro;

public class Enemy : MonoBehaviour
{
    // Statistiques runtime copiées depuis EnemyData au démarrage
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
    private Transform playerTargetTransform;
    private PlayerHealth playerHealth;
    private Collider2D playerCollider;

    private bool isStunned = false;

    // Références composants
    private Rigidbody2D rb;
    private Collider2D mainCollider;
    private SpriteRenderer spriteRenderer;

    private PlayerHealth ResolvePlayerHealth(GameObject playerObject)
    {
        if (playerObject == null)
        {
            return null;
        }

        PlayerHealth health = playerObject.GetComponent<PlayerHealth>();
        if (health != null)
        {
            return health;
        }

        health = playerObject.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            return health;
        }

        return playerObject.GetComponentInChildren<PlayerHealth>();
    }

    private Collider2D ResolvePlayerCollider(GameObject playerObject)
    {
        if (playerObject == null)
        {
            return null;
        }

        Collider2D collider = playerObject.GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider;
        }

        collider = playerObject.GetComponentInParent<Collider2D>();
        if (collider != null)
        {
            return collider;
        }

        return playerObject.GetComponentInChildren<Collider2D>();
    }

    private void TryResolvePlayerReferences()
    {
        // Priorite au vrai objet joueur qui porte le controle de mouvement.
        PlayerControler playerController = FindFirstObjectByType<PlayerControler>();
        if (playerController != null)
        {
            player = playerController.gameObject;
        }

        if (player == null)
        {
            // Priorite au tag standard.
            try
            {
                player = GameObject.FindWithTag("Player");
            }
            catch (UnityException)
            {
                // Le tag peut manquer apres un revert de ProjectSettings.
            }

            // Fallback robuste si le tag Player n'existe pas ou n'est pas assigne.
            if (player == null)
            {
                PlayerHealth detectedHealth = FindFirstObjectByType<PlayerHealth>();
                if (detectedHealth != null)
                {
                    player = detectedHealth.gameObject;
                }
            }
        }

        if (player != null)
        {
            playerTargetTransform = player.transform;

            if (playerHealth == null)
            {
                playerHealth = ResolvePlayerHealth(player);
            }

            if (playerCollider == null)
            {
                playerCollider = ResolvePlayerCollider(player);
            }
        }
    }

    private void Start()
    {
        // Récupération du joueur et de ses composants utiles
        TryResolvePlayerReferences();

        // Cache des composants de l'ennemi
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Chargement des stats depuis le ScriptableObject
        if (ennemyData != null)
        {
            health = ennemyData.health;
            type = ennemyData.type;
            damage = ennemyData.damage;
            speed = ennemyData.speed;
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = ennemyData.animatorController;
        }

        if (nameText != null && GameManager.Instance != null)
        {
            nameText.text = GameManager.Instance.list_enemies.Find(e => e.enemy == gameObject)?.code ?? "Unknown";
        }
        if (type != EnemyType.Distant)
        {
            isMoving = true;
            animator.SetTrigger("Move");
        }
        else
        {
            // Ennemi distant: boucle de tir
            StartCoroutine(Shoot());
        }
    }

    private void FixedUpdate()
    {
        if (player == null || playerTargetTransform == null || playerHealth == null || playerCollider == null)
        {
            TryResolvePlayerReferences();
        }

        if (isStunned) return;

        Move();
        CheckContactDamage();
    }

    private void CheckContactDamage()
    {
        if (health == 0 || mainCollider == null || playerCollider == null) return;

        // Détection de chevauchement précis entre colliders
        ColliderDistance2D distanceInfo = mainCollider.Distance(playerCollider);
        if (distanceInfo.isOverlapped)
        {
            TryDamagePlayer(playerCollider.gameObject);
        }
    }

    public void ApplyKnockback(Vector2 direction, float force)
    {
        isStunned = true;

        // Stoppe le mouvement actuel
        rb.linearVelocity = Vector2.zero;

        // Applique le knockback du sort
        rb.AddForce(direction * force, ForceMode2D.Impulse);

        // Arrête le knockback après une courte durée
        StartCoroutine(KnockbackDuration(0.3f));
    }

    private IEnumerator KnockbackDuration(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Évite que l'ennemi continue de glisser
        rb.linearVelocity = Vector2.zero;

        // Fin de l'état de stun
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

    // Appelé par l'animation d'attaque de l'ennemi distant
    public void ShootProjectile()
    {
        if (playerTargetTransform == null) return;

        if (playerTargetTransform.position.x < transform.position.x)
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
        if (projectileToLaunch == null || playerTargetTransform == null) return;

        projectileToLaunch.GetComponent<ProjectileDistant>().Launch(playerTargetTransform.position, damage);
    }

    private void Move()
    {
        if (health == 0 || playerTargetTransform == null) return;

        // Orientation visuelle vers le joueur
        if (playerTargetTransform.position.x < transform.position.x)
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;

        // Les ennemis distants ne se déplacent pas vers le joueur
        if (type == EnemyType.Distant) return;

        CheckAttack();
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTargetTransform.position, speed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, playerTargetTransform.position, speed / 4 * Time.deltaTime);
        }
    }

    private void CheckAttack()
    {
        // Passe en attaque quand le joueur est dans la portée proche
        if (playerTargetTransform != null && Vector2.Distance(transform.position, playerTargetTransform.position) < 1f)
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

        // Garde-fou: dégâts seulement si joueur proche
        if (playerTargetTransform != null && Vector2.Distance(transform.position, playerTargetTransform.position) <= 1.2f)
        {
            playerHealth.TakeDamage(damage);
        }
    }

    private bool TryTakeDamageFromSort(GameObject source)
    {
        if (source == null)
        {
            return false;
        }

        Sort sort = source.GetComponent<Sort>();
        if (sort == null)
        {
            sort = source.GetComponentInParent<Sort>();
        }

        if (sort == null)
        {
            return false;
        }

        sort.DestroySort(gameObject);
        TakeDamage(sort.damage);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (ResolvePlayerHealth(other.gameObject) != null)
        {
            TryDamagePlayer(other.gameObject);
        }

        TryTakeDamageFromSort(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (ResolvePlayerHealth(other.gameObject) != null)
        {
            TryDamagePlayer(other.gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryTakeDamageFromSort(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject playerObject)
    {
        if (health == 0)
        {
            return;
        }

        // Délègue au système de vie du joueur (cooldown géré dans PlayerHealth)
        PlayerHealth targetHealth = ResolvePlayerHealth(playerObject);
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
            // Nettoyage de l'ennemi dans la liste du GameManager
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
