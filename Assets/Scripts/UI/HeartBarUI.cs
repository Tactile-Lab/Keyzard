using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Affiche la barre de vie du joueur sous forme de cœurs.
/// Chaque cœur représente une certaine quantité de HP (configurable).
/// Génère automatiquement les slots UI ou utilise ceux existants.
/// </summary>
public class HeartBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Heart Display")]
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Generation")]
    [SerializeField] private bool autoGenerateHearts = true;
    [SerializeField] private Vector2 heartSlotSize = new Vector2(48f, 48f);

    private Image[] heartImages;
    private const int HpPerHeart = 10;

    private void OnEnable()
    {
        // S'abonner aux changements de vie pour mettre à jour l'affichage.
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        // Se désabonner pour éviter les fuites de références.
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= Refresh;
        }
    }

    private void Start()
    {
        // Initialiser le nombre de cœurs et l'affichage visuel.
        if (playerHealth != null)
        {
            EnsureHeartSlots(playerHealth.MaxHealth);
            Refresh(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    /// <summary>
    /// Met à jour l'affichage des cœurs en fonction de la vie actuelle.
    /// </summary>
    private void Refresh(int currentHealth, int maxHealth)
    {
        EnsureHeartSlots(maxHealth);

        if (heartImages == null || heartImages.Length == 0)
        {
            return;
        }

        // Parcourir chaque cœur et déterminer son état (plein, demi, vide).
        for (int index = 0; index < heartImages.Length; index++)
        {
            if (heartImages[index] == null)
            {
                continue;
            }

            Sprite sprite = GetHeartSpriteForHealth(currentHealth, index);
            heartImages[index].sprite = sprite;
        }
    }

    /// <summary>
    /// Détermine le sprite du cœur en fonction de la vie résiduelle à cet index.
    /// </summary>
    private Sprite GetHeartSpriteForHealth(int currentHealth, int heartIndex)
    {
        int heartStart = heartIndex * HpPerHeart;
        int hpInThisHeart = Mathf.Clamp(currentHealth - heartStart, 0, HpPerHeart);

        if (hpInThisHeart >= HpPerHeart)
        {
            return fullHeart;
        }

        if (hpInThisHeart >= HpPerHeart / 2)
        {
            return halfHeart;
        }

        return emptyHeart;
    }

    /// <summary>
    /// Crée ou ajuste le nombre de slots de cœurs selon la vie maximale.
    /// </summary>
    private void EnsureHeartSlots(int maxHealth)
    {
        if (!autoGenerateHearts)
        {
            return;
        }

        int requiredHearts = Mathf.Max(1, Mathf.CeilToInt(maxHealth / (float)HpPerHeart));

        // Pas besoin de régénérer si le nombre de cœurs est déjà correct.
        if (heartImages != null && heartImages.Length == requiredHearts)
        {
            return;
        }

        // Supprimer les anciens cœurs.
        for (int index = transform.childCount - 1; index >= 0; index--)
        {
            Destroy(transform.GetChild(index).gameObject);
        }

        // Créer les nouveaux slots.
        heartImages = new Image[requiredHearts];
        for (int index = 0; index < requiredHearts; index++)
        {
            CreateHeartSlot(index);
        }
    }

    /// <summary>
    /// Crée un seul slot de cœur et l'initialise.
    /// </summary>
    private void CreateHeartSlot(int slotIndex)
    {
        GameObject heartSlot = new GameObject($"Heart_{slotIndex + 1}", typeof(RectTransform), typeof(Image));
        heartSlot.transform.SetParent(transform, false);

        // Configurer la taille et la position du slot.
        RectTransform rectTransform = heartSlot.GetComponent<RectTransform>();
        rectTransform.sizeDelta = heartSlotSize;

        // Initialiser l'image du cœur.
        Image image = heartSlot.GetComponent<Image>();
        image.preserveAspect = true;
        image.sprite = emptyHeart;

        heartImages[slotIndex] = image;
    }
}