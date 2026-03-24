using UnityEngine;
using UnityEngine.UI;

public class GlossarySlotRefs : MonoBehaviour
{
    [Header("Slot UI Refs")]
    [SerializeField] private Image iconSortImage;
    [SerializeField] private GameObject selectionObject;

    public Image IconSortImage => iconSortImage;
    public GameObject SelectionObject => selectionObject;
}
