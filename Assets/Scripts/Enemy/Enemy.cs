using UnityEngine;
using System.Collections;
using TMPro;
using NUnit.Framework;

public class Enemy : MonoBehaviour
{
    // Statistiques runtime copiées depuis EnemyData au démarrage
    private float health;
    private EnemyType type;
    private int damage;
    private float speed;
    protected Animator animator;

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

    private bool playerReferencesResolved = false;

    protected RoomManager room;

    private bool isAttacking = false;


    /// <summary>
    /// Résout les références du joueur une seule fois au Start.
    /// Utilise plusieurs stratégies: PlayerController -> tag -> PlayerHealth.
    /// </summary>
    private void ResolvePlayerReferencesOnce()
    {
        if (playerReferencesResolved)
        {
            return;
        }

        // Stratégie 1: Trouver par PlayerControler
        PlayerControler playerController = FindFirstObjectByType<PlayerControler>();
        if (playerController != null)
        {
            player = playerController.gameObject;
        }

        // Stratégie 2: Fallback au tag
        if (player == null)
        {
            try
            {
                player = GameObject.FindWithTag("Player");
            }
            catch (UnityException)
            {
                // Le tag peut manquer après revert
            }
        }

        // Stratégie 3: Fallback à FindFirstObjectByType<PlayerHealth>
        if (player == null)
        {
            PlayerHealth detectedHealth = FindFirstObjectByType<PlayerHealth>();
            if (detectedHealth != null)
            {
                player = detectedHealth.gameObject;
            }
        }

        // Une fois le joueur trouvé, récupérer ses composants
        if (player != null)
        {
            playerTargetTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
            playerCollider = player.GetComponent<Collider2D>();
            playerReferencesResolved = true;
        }
    }

    protected virtual void Start()
    {
        // Récupérer les références au joueur une seule fois
        ResolvePlayerReferencesOnce();

        // Cache des composants de l'ennemi
        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Charger les stats depuis le ScriptableObject
        if (ennemyData != null)
        {
            health = ennemyData.health;
            type = ennemyData.type;
            damage = ennemyData.damage;
            speed = ennemyData.speed;
            animator = GetComponent<Animator>();
            animator.runtimeAnimatorController = ennemyData.animatorController;
        }

        // Afficher le code de l'ennemi si le UI le permet
        if (nameText != null && GameManager.Instance != null)
        {
            nameText.text = GameManager.Instance.list_enemies.Find(e => e.enemy == gameObject)?.code ?? "Unknown";
        }


        // Initialiser l'état d'animation selon le type
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

    protected virtual void FixedUpdate()
    {
        // Assurer que les références sont résolues une seule fois
        if (!playerReferencesResolved)
        {
            ResolvePlayerReferencesOnce();
        }

        if (isStunned)
        {
            return;
        }

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
            TryDamagePlayer();
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

    protected virtual IEnumerator Shoot()
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
        isAttacking = true;
    }

    public void BeginAnimationHitOrDeath()
    {
        if (isAttacking)
        {
            Destroy(projectileToLaunch.gameObject);
        }
    }

    public void LaunchProjectile()
    {
        if (projectileToLaunch == null || playerTargetTransform == null)
        {
            if (projectileToLaunch != null)
            {
                Destroy(projectileToLaunch.gameObject);
            }
            return; // sortir de la fonction pour éviter d’appeler Launch
        }

        projectileToLaunch.GetComponent<ProjectileDistant>().Launch(playerTargetTransform.position, damage);
        isAttacking = false;
    }

    protected virtual void Move()
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

    protected virtual void CheckAttack()
    {
        // Passe en attaque quand le joueur est dans la portée proche
        if (playerTargetTransform != null && Vector2.Distance(transform.position, playerTargetTransform.position) < 1f)
        {
            isMoving = false;
            animator.SetTrigger("Attack");
        }
    }

    public virtual void EndAttack()
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

    /// <summary>
    /// Essaie de recevoir des dégâts d'un sort détecté en collision.
    /// Cherche le composant Sort sur l'objet ou ses parents.
    /// </summary>
    private bool TryTakeDamageFromSort(GameObject source)
    {
        if (source == null)
        {
            return false;
        }

        // Chercher le composant Sort sur l'objet ou son parent
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
        // Appliquer des dégâts au joueur si c'est le bon objet
        if (other.gameObject == player)
        {
            TryDamagePlayer();
        }

        // Recevoir des dégâts d'un sort
        TryTakeDamageFromSort(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Continuer à appliquer des dégâts au joueur à chaque frame
        if (other.gameObject == player)
        {
            TryDamagePlayer();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Recevoir des dégâts d'un sort en collision (non-trigger)
        TryTakeDamageFromSort(collision.gameObject);
    }

    /// <summary>
    /// Applique des dégâts au joueur via son système de santé.
    /// </summary>
    private void TryDamagePlayer()
    {
        if (health == 0 || playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(damage);
    }

    public virtual void TakeDamage(float damageAmount)
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

    public virtual void EndTakeDamage()
    {
        if (health == 0)
        {
            // Nettoyage de l'ennemi dans la liste du GameManager
            GameManager.Instance.list_enemies.RemoveAll(e => e.enemy == gameObject);
            room.EnemyDied(this);
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

    public void SetRoom(RoomManager r)
    {
        room = r;
    }
}
