using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(GridLayoutGroup))]
public class SlotGenerator : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject slotPrefab;

    [Header("Layout")]
    [Min(1)] public int columns = 4;
    [Min(1)] public int rows = 3;

    [Tooltip("Si actif, génère exactement slotCount slots (ex: 21). Les rows sont calculées automatiquement.")]
    public bool useSlotCount = true;
    [Min(1)] public int slotCount = 21;

    [Header("Grid Visual")]
    public Vector2 cellSize = new Vector2(100f, 100f);
    public Vector2 spacing = new Vector2(10f, 10f);
    public RectOffset padding; // pas d'initialisation ici

    [Header("Auto Resize")]
    public bool addContentSizeFitter = true;

#if UNITY_EDITOR
    [Header("Editor")]
    [Tooltip("Regenerer automatiquement les slots en mode editeur quand les valeurs changent.")]
    public bool autoRegenerateInEditor = false;
#endif

    private GridLayoutGroup grid;

    private void Reset()
    {
        EnsureDefaults();
    }

    private void OnEnable()
    {
        EnsureDefaults();
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif
        Regenerate();
    }

    private void Start()
    {
        // Runtime safety: si l'objet est instancié en jeu.
        Regenerate();
    }

    private void OnValidate()
    {
        columns = Mathf.Max(1, columns);
        rows = Mathf.Max(1, rows);
        slotCount = Mathf.Max(1, slotCount);
        EnsureDefaults();

        #if UNITY_EDITOR
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
                    if (!autoRegenerateInEditor)
                    {
                        return;
                    }
            EditorApplication.delayCall -= DelayedRegenerate;
            EditorApplication.delayCall += DelayedRegenerate;
        }
        #endif
    }

#if UNITY_EDITOR
    private void DelayedRegenerate()
    {
        if (this == null) return;
        if (Application.isPlaying) return;

        // Evite de detruire des objets selectionnes dans l'inspecteur pendant le refresh.
        if (Selection.activeGameObject != null && Selection.activeGameObject.transform.IsChildOf(transform))
        {
            return;
        }

        Regenerate();
    }
#endif

    [ContextMenu("Regenerate Slots")]
    public void Regenerate()
    {
        if (slotPrefab == null) return;
        if (!gameObject.scene.IsValid()) return; // Ignore prefab asset editing context

        EnsureComponents();

        int finalRows = useSlotCount ? Mathf.CeilToInt(slotCount / (float)columns) : rows;
        int totalSlots = useSlotCount ? slotCount : rows * columns;

        ConfigureGrid(finalRows);
        ClearChildren();

        for (int i = 0; i < totalSlots; i++)
        {
            GameObject slot = Instantiate(slotPrefab, transform);
            slot.name = $"Slot_{i + 1:00}";
        }
    }

    private void EnsureComponents()
    {
        if (grid == null) grid = GetComponent<GridLayoutGroup>();

        if (addContentSizeFitter)
        {
            ContentSizeFitter fitter = GetComponent<ContentSizeFitter>();
            if (fitter == null) fitter = gameObject.AddComponent<ContentSizeFitter>();

            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }

    private void ConfigureGrid(int finalRows)
    {
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = cellSize;
        grid.spacing = spacing;
        grid.padding = padding;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;

        // Si tu veux forcer la hauteur/largeur manuellement sans fitter, tu peux calculer ici avec finalRows.
        _ = finalRows;
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
#if UNITY_EDITOR
            if (!Application.isPlaying) DestroyImmediate(child.gameObject);
            else Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    private void EnsureDefaults()
    {
        if (padding == null)
            padding = new RectOffset(10, 10, 10, 10);
    }
}