using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SlotGenerator : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject slotPrefab;

    [Header("Grid")]
    [Min(1)] [SerializeField] private int slotCount = 12;

    [ContextMenu("Regenerate Slots")]
    public void Regenerate()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("[SlotGenerator] slotPrefab non assigné", this);
            return;
        }

        if (!gameObject.scene.IsValid())
        {
            return;
        }

        ClearChildren();

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            slot.name = $"Slot_{i + 1:00}";
        }
    }

    private void Start()
    {
        Regenerate();
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                DestroyImmediate(child.gameObject);
            else
#endif
                Destroy(child.gameObject);
        }
    }
}