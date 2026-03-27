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

    [Header("Audio")]
    [SerializeField] private SFXEventKey onSelectedSfx = SFXEventKey.UIMenuMove;
    [SerializeField] private SFXEventKey onPressedSfx = SFXEventKey.UIMenuConfirm;

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
        if (isSelected == selected)
        {
            return;
        }

        isSelected = selected;

        if (!selected)
        {
            isPressed = false;
        }

        if (selected)
        {
            AudioManager.Instance?.PlaySFXEvent(onSelectedSfx);
        }

        RefreshSprite();
    }

    public void SetPressed(bool pressed)
    {
        if (isPressed == pressed)
        {
            return;
        }

        isPressed = pressed;

        if (pressed)
        {
            AudioManager.Instance?.PlaySFXEvent(onPressedSfx);
        }

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
