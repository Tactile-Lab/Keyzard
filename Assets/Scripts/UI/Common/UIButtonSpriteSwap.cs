using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class UIButtonSpriteSwap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image targetImage;

    [Header("Sprites")]
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite pressedSprite;
    [SerializeField] private Sprite disabledSprite;

    [Header("Behavior")]
    [SerializeField] private bool autoUseImageSpriteAsNormal = true;

    private bool isSelected;
    private bool isPressed;
    private bool isDisabled;

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (targetImage != null && autoUseImageSpriteAsNormal && normalSprite == null)
        {
            normalSprite = targetImage.sprite;
        }

        RefreshSprite();
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (!selected)
        {
            isPressed = false;
        }

        RefreshSprite();
    }

    public void SetPressed(bool pressed)
    {
        isPressed = pressed;
        RefreshSprite();
    }

    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;

        if (disabled)
        {
            isPressed = false;
            isSelected = false;
        }

        RefreshSprite();
    }

    public void RefreshSprite()
    {
        if (targetImage == null)
        {
            return;
        }

        Sprite nextSprite = ResolveSprite();
        if (nextSprite != null)
        {
            targetImage.sprite = nextSprite;
        }
    }

    private Sprite ResolveSprite()
    {
        if (isDisabled)
        {
            return disabledSprite != null ? disabledSprite : normalSprite;
        }

        if (isPressed)
        {
            return pressedSprite != null ? pressedSprite : (selectedSprite != null ? selectedSprite : normalSprite);
        }

        if (isSelected)
        {
            return selectedSprite != null ? selectedSprite : normalSprite;
        }

        return normalSprite;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if (!Application.isPlaying)
        {
            RefreshSprite();
        }
    }
#endif
}
