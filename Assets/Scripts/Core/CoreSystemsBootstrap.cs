using UnityEngine;

public class CoreSystemsBootstrap : MonoBehaviour
{
    [Header("Manager Prefabs")]
    [SerializeField] private GameManager gameManagerPrefab;
    [SerializeField] private SpellInventoryManager spellInventoryManagerPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;

    private void Awake()
    {
        EnsureManagers();
    }

    private void EnsureManagers()
    {
        if (GameManager.Instance == null && gameManagerPrefab != null)
        {
            Instantiate(gameManagerPrefab);
        }

        if (SpellInventoryManager.Instance == null && spellInventoryManagerPrefab != null)
        {
            Instantiate(spellInventoryManagerPrefab);
        }

        if (AudioManager.Instance == null && audioManagerPrefab != null)
        {
            Instantiate(audioManagerPrefab);
        }
    }
}