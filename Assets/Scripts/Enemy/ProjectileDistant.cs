using UnityEngine;

public class ProjectileDistant : MonoBehaviour
{
    private float damage;
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

    public void Launch(Vector3 playerPos)
    {
        transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        directionBase = (playerPos - transform.position).normalized;
        isMoving = true;
    }
}
