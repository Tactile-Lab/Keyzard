using UnityEngine;
using UnityEngine.UI;

public class HeartBarUI : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    private Image[] heartImages;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;
    [SerializeField] private bool autoGenerateHearts = true;
    [SerializeField] private Vector2 heartSlotSize = new Vector2(48f, 48f);

    private const int HpPerHeart = 10;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged += Refresh;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.HealthChanged -= Refresh;
        }
    }

    private void Start()
    {
        if (playerHealth != null)
        {
            EnsureHeartSlots(playerHealth.MaxHealth);
            Refresh(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void Refresh(int currentHealth, int maxHealth)
    {
        EnsureHeartSlots(maxHealth);

        if (heartImages == null || heartImages.Length == 0)
        {
            return;
        }

        for (int index = 0; index < heartImages.Length; index++)
        {
            if (heartImages[index] == null)
            {
                continue;
            }

            int heartStart = index * HpPerHeart;
            int hpInHeart = Mathf.Clamp(currentHealth - heartStart, 0, HpPerHeart);

            if (hpInHeart >= HpPerHeart)
            {
                heartImages[index].sprite = fullHeart;
            }
            else if (hpInHeart >= HpPerHeart / 2)
            {
                heartImages[index].sprite = halfHeart;
            }
            else
            {
                heartImages[index].sprite = emptyHeart;
            }
        }
    }

    private void EnsureHeartSlots(int maxHealth)
    {
        if (!autoGenerateHearts)
        {
            return;
        }

        int requiredHearts = Mathf.Max(1, Mathf.CeilToInt(maxHealth / (float)HpPerHeart));
        if (heartImages != null && heartImages.Length == requiredHearts)
        {
            return;
        }

        for (int index = transform.childCount - 1; index >= 0; index--)
        {
            Destroy(transform.GetChild(index).gameObject);
        }

        heartImages = new Image[requiredHearts];
        for (int index = 0; index < requiredHearts; index++)
        {
            GameObject heartSlot = new GameObject($"Heart_{index + 1}", typeof(RectTransform), typeof(Image));
            heartSlot.transform.SetParent(transform, false);

            RectTransform rectTransform = heartSlot.GetComponent<RectTransform>();
            rectTransform.sizeDelta = heartSlotSize;

            Image image = heartSlot.GetComponent<Image>();
            image.preserveAspect = true;
            image.sprite = emptyHeart;

            heartImages[index] = image;
        }
    }
}