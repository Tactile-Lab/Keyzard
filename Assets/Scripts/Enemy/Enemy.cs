using UnityEngine;
using System.Collections;


public class Ennemy : MonoBehaviour
{
    private float health;
    private EnemyType type;
    private float damage;
    private float speed;
    private Animator animator;

    private bool isMoving;

    [SerializeField]
    private EnnemyData ennemyData;

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
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            animator.SetBool("Move", true);
            Move();
        }
        else
        {
            animator.SetBool("Move", false);
        }
    }

    private void Move()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    { 
        if (collision.gameObject.CompareTag("Player"))
        {
            //TODO : quelque chose comme collision.gameObject.GetComponent<Player>().TakeDamage(damage);
        }

        if (collision.gameObject.CompareTag("Projectile"))
        {
            //TODO : quelque chose comme
            //collision.gameObject.GetComponent<Projectile>().DestroyProjectile();
            //TakeDamage(collision.gameObject.GetComponent<Projectile>().damage);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        animator.SetTrigger("TrTakeDmg");
        Debug.Log(health + " " + damageAmount + " " + (health - damageAmount));
        health -= damageAmount;
        if (health <= 0)
        {
            Die();
        }
    }


    public void Die()
    {
        Destroy(gameObject);
    }


}
