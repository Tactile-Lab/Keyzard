using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    Combat,
    Reward,
    Boss
}

public class RoomManager : MonoBehaviour
{
    [Header("Room Settings")]
    [SerializeField] private RoomType roomType;
    [SerializeField] private SpellUnlockTrigger rewardTrigger;
    [SerializeField] private bool rewardGiven = false;

    [Header("Enemies & Doors")]
    [SerializeField] private List<Enemy> enemies = new List<Enemy>();
    [SerializeField] private DoorController door; 
    [SerializeField] private bool playerInside = false;
    [SerializeField] private bool roomCleared = false;

    private List<Mannequin> nonBlockingMannequins = new List<Mannequin>();

    void Awake()
    {
        // Récupérer tous les enemies enfants
        enemies.AddRange(GetComponentsInChildren<Enemy>(true));

        foreach (var enemy in enemies)
        {
            enemy.SetRoom(this);
            enemy.enabled = false;
            var col = enemy.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            var sr = enemy.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = false;
        }

        door = GetComponentInChildren<DoorController>();
        if (door == null)
            Debug.LogWarning($"Room {name} : pas de DoorController trouvé !");

        // Désactiver le trigger au départ
        if (rewardTrigger != null)
            rewardTrigger.gameObject.SetActive(false);
    }

    void Start()
    {
        if (enemies.Count == 0)
        {
            roomCleared = true;
            door?.Open();
        }
        else
        {
            door?.Close();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null && col.OverlapPoint(player.transform.position))
                ForceEnterRoom();
        }
    }

    public void OnPlayerEnter()
    {
        if (playerInside) return;
        playerInside = true;

        if (roomCleared)
            door?.Open();

        HandleDoor();
        ActivateEnemies();

        PrepareReward(); // <--- prépare le sort pour le trigger

        AddTutoMannequinsToGameManager();
        AddNonBlockingMannequinsToGameManager();
    }

    public void ForceEnterRoom()
    {
        playerInside = true;

        if (roomCleared)
        {
            door?.Open();
        }
        else
        {
            HandleDoor();
            ActivateEnemies();

            PrepareReward();

            AddTutoMannequinsToGameManager();
            AddNonBlockingMannequinsToGameManager();
        }
    }

    public void PlayerExited()
    {
        playerInside = false;

        if (GameManager.Instance != null)
        {
            foreach (var m in nonBlockingMannequins)
                GameManager.Instance.list_enemies.RemoveAll(e => e.enemy == m.gameObject);

            foreach (var m in GetComponentsInChildren<Mannequin>(true))
            {
                if (!m.countsForRoomLock)
                    GameManager.Instance.list_enemies.RemoveAll(e => e.enemy == m.gameObject);
            }
        }

        nonBlockingMannequins.Clear();
    }

    private void PrepareReward()
    {
        if (rewardGiven) return;
        if (roomType != RoomType.Reward) return;
        if (SpellInventoryManager.Instance == null) return;
        if (rewardTrigger == null) return;

        Sort spell = SpellInventoryManager.Instance.GetRandomLockedSpell();
        if (spell == null)
        {
            Debug.Log("Tous les sorts sont déjà débloqués.");
            return;
        }

        rewardTrigger.SetSpell(spell);
        rewardTrigger.gameObject.SetActive(true);

        Debug.Log($"Room {name} : sort préparé dans le trigger -> {spell.nomSort}");

        rewardGiven = true;
    }

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
                enemy.SetRoom(this);
                var col = enemy.GetComponent<Collider2D>();
                if (col != null) col.enabled = true;
                var sr = enemy.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;

                activeEnemies.Add(enemy.gameObject);
            }
        }

        GameManager.Instance.RegisterEnemies(activeEnemies);
    }

    public void EnemyDied(Enemy enemy)
    {
        enemies.Remove(enemy);

        if (enemies.Count == 0)
        {
            roomCleared = true;
            door?.Open();
            Debug.Log($"Room {name} cleared, porte ouverte");
        }
    }

    private void HandleDoor()
    {
        if (door == null) return;

        bool anyBlocking = enemies.Exists(e => e is Mannequin m ? m.countsForRoomLock : true);

        if (roomCleared || !anyBlocking)
            door.Open();
        else
            door.Close();
    }

    private void AddTutoMannequinsToGameManager()
    {
        foreach (var m in GetComponentsInChildren<Mannequin>(true))
        {
            if (m.unlockRoomOnHit && m.countsForRoomLock)
            {
                if (!GameManager.Instance.list_enemies.Exists(e => e.enemy == m.gameObject))
                    GameManager.Instance.RegisterEnemies(new List<GameObject> { m.gameObject });
            }
        }
    }

    private void AddNonBlockingMannequinsToGameManager()
    {
        foreach (var m in GetComponentsInChildren<Mannequin>(true))
        {
            if (!m.countsForRoomLock && !nonBlockingMannequins.Contains(m))
            {
                if (!GameManager.Instance.list_enemies.Exists(e => e.enemy == m.gameObject))
                {
                    GameManager.Instance.RegisterEnemies(new List<GameObject> { m.gameObject });
                    nonBlockingMannequins.Add(m);
                }
            }
        }
    }
}