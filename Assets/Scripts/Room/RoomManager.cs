using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{

    [SerializeField]
    private List<Enemy> enemies = new List<Enemy>();

    [SerializeField]
    private DoorController door; // une seule porte par room
    [SerializeField]
    private bool playerInside = false;
    [SerializeField]
    private bool roomCleared = false;

    void Awake()
    {
        // Détecte automatiquement tous les ennemis enfants (actifs et inactifs)
        enemies.AddRange(GetComponentsInChildren<Enemy>(true));

        // Désactive tous les ennemis au départ
        foreach (var enemy in enemies)
        {
            enemy.SetRoom(this);
            enemy.enabled = false;
            var col = enemy.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            var sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        // Détecte la porte enfant
        door = GetComponentInChildren<DoorController>();
        if (door == null)
            Debug.LogWarning($"Room {name} : pas de DoorController trouvé !");
    }

    void Start()
    {
        // Salle vide → porte ouverte
        if (enemies.Count == 0)
        {
            roomCleared = true;
            if (door != null) door.Open();
        }
        else
        {
            // Salle avec ennemis → porte fermée
            if (door != null) door.Close();
        }

        // Vérifie si le joueur start déjà dans la room
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(player.transform.position))
                ForceEnterRoom();
        }
    }

    // Appelé par RoomTrigger quand le joueur entre
    public void OnPlayerEnter()
    {
        if (playerInside) return;
        playerInside = true;

        if (roomCleared)
        {
            if (door != null) door.Open();
            return;
        }

        if (door != null) door.Close();
        ActivateEnemies();
    }

    // Appelé si le joueur start déjà dans la room
    public void ForceEnterRoom()
    {
        playerInside = true;

        if (roomCleared)
        {
            if (door != null) door.Open();
        }
        else
        {
            if (door != null) door.Close();
            ActivateEnemies();
        }
    }

    // Appelé par RoomTrigger quand le joueur sort
    public void PlayerExited()
    {
        playerInside = false;
    }

    // Active les ennemis et les enregistre dans le GameManager
    private void ActivateEnemies()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager manquant !");
            return;
        }

        List<GameObject> activeEnemies = new List<GameObject>();

        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.enabled = true;
                var col = enemy.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
                var sr = enemy.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;

                activeEnemies.Add(enemy.gameObject);
            }
        }

        GameManager.Instance.RegisterEnemies(activeEnemies);
        Debug.Log($"Room {name} : {activeEnemies.Count} ennemis activés et enregistrés");
    }

    // Appelé par chaque Enemy quand il meurt
    public void EnemyDied(Enemy enemy)
    {
        enemies.Remove(enemy);

        if (enemies.Count == 0)
        {
            roomCleared = true;
            if (door != null) door.Open();
            Debug.Log($"Room {name} cleared, porte ouverte");
        }
    }
}