using UnityEngine;
using System.Collections;
using System.Net;
using Unity.VisualScripting;
using TMPro;

public enum EnemyAction
{
    Move,
    Attack,
    TakeDamage,
    Die
}

public class Enemy : MonoBehaviour
{
    private float health;
    private EnemyType type;
    private float damage;
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


    private void Start()
    {
        player = GameObject.FindWithTag("Player");
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
        Move();
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

        projectileToLaunch.GetComponent<ProjectileDistant>().Launch(player.transform.position);
    }

    private void Move()
    {
        if (health == 0)
            return;
        

        SpriteRenderer spR = GetComponent<SpriteRenderer>();
        if (player.transform.position.x < transform.position.x)
            spR.flipX = true;
        else
            spR.flipX = false;


        if (type == EnemyType.Distant) return;

        checkAttack();
        if (isMoving)
        {

            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed / 4 * Time.deltaTime);
        }
        
    }

    private void checkAttack()
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

        private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("trigger");

        if (other.CompareTag("Player"))
        {
            // TODO : quelque chose comme
            // other.GetComponent<Player>().TakeDamage(damage);
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
