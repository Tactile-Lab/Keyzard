using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameOverMenu : MonoBehaviour
{
    [Header("Images du Game Over")]
    public RectTransform imageRejouer;
    public RectTransform imageMenu;

    [Header("Icône de sélection")]
    public RectTransform iconSelection;

    private int selectionIndex = 0;
    private Vector2 decalage;

    void OnEnable()
    {
        // ✅ On reset la sélection AVANT tout
        selectionIndex = 0;

        // ✅ On calcule le décalage AVEC la bonne position actuelle
        decalage = iconSelection.anchoredPosition - imageRejouer.anchoredPosition;

        UpdateIconPosition();
    }

    void Update()
    {

        if (!PlayerDeathEffects.CanInteract)
        return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.rightArrowKey.wasPressedThisFrame && selectionIndex == 0)
        {
            selectionIndex = 1;
            UpdateIconPosition();
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame && selectionIndex == 1)
        {
            selectionIndex = 0;
            UpdateIconPosition();
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            if (selectionIndex == 0)
                Rejouer();
            else
                RetourMenu();
        }
    }

    void UpdateIconPosition()
    {
        RectTransform target = (selectionIndex == 0) ? imageRejouer : imageMenu;
        iconSelection.anchoredPosition = target.anchoredPosition + decalage;
    }

    void Rejouer()
    {
        SpellInventoryManager.Instance.ResetInventory();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void RetourMenu()
    {
        SceneManager.LoadScene(0);
    }
}