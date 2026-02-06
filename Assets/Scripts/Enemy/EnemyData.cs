using UnityEditor.Animations;
using UnityEngine;

public enum EnemyType
{
    Rapide,
    Lourd,
    Distant
}

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Ennemy", order = 1)]
public class EnemyData : ScriptableObject
{
    public int health;
    public EnemyType type;
    public int damage;
    public float speed;
    public AnimatorController animatorController;
}
