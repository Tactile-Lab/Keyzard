using UnityEngine;
using System.Collections;
using System.Net;
using Unity.VisualScripting;

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

    private bool isMoving;

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
        isMoving = true;
        animator.SetBool("Move", true);
    }

    private void FixedUpdate()
    {
        Move();
    }



    private void Move()
    {
        checkAttack();
        if (isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        }
        else if (health==0)
        {
            animator.SetTrigger("Die");
            gameObject.GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed/4 * Time.deltaTime);
        }
    }

    private void checkAttack()
    {
        if (Vector2.Distance(transform.position, player.transform.position) < 1f)
        {
            isMoving = false;
            animator.SetBool("Move", false);
            animator.SetTrigger("Attack");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    { 
        Debug.Log("collison");
        if (collision.gameObject.CompareTag("Player"))
        {
            //TODO : quelque chose comme collision.gameObject.GetComponent<Player>().TakeDamage(damage);
        }

        if (collision.gameObject.CompareTag("Projectile"))
        {
            //TODO : quelque chose comme
            collision.gameObject.GetComponent<Sort>().DestroySort();
            TakeDamage(collision.gameObject.GetComponent<Sort>().damage);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        animator.SetTrigger("TakeDmg");
        Debug.Log(health + " " + damageAmount + " " + (health - damageAmount));
        health -= damageAmount;
        if (health < 0)
        {
            health = 0;
        }
    }


    private IEnumerator PlayDeath()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);

        Destroy(gameObject); // détruira l'objet DeathAnimation
    }


}
