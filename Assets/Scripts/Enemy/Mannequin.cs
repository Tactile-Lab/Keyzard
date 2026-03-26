// ------------------------- Mannequin.cs -------------------------
using UnityEngine;
using System.Collections;

public class Mannequin : Enemy
{
    [SerializeField] public bool unlockRoomOnHit = true;
    public bool countsForRoomLock = true;

    protected override void Start()
    {
        if (nameText != null && GameManager.Instance != null)
        {
            nameText.text = GameManager.Instance.list_enemies.Find(e => e.enemy == gameObject)?.code ?? "Unknown";
        }
    }

    protected override void FixedUpdate() { }
    protected override void Move() { }
    protected override IEnumerator Shoot() { yield break; }
    protected override void CheckAttack() { }

    public override void TakeDamage(float damageAmount)
    {
        Debug.Log("Mannequin touché par un sort !");

        if (animator != null)
        {
            animator.SetTrigger("TakeDmg");
        }

        if (unlockRoomOnHit && countsForRoomLock && room != null)
        {
            room.EnemyDied(this);
            countsForRoomLock = false; // pour éviter de le compter 2x
            Debug.Log("Mannequin tuto activé : porte ouverte");
        }
    }

    public override void EndTakeDamage() { }
}