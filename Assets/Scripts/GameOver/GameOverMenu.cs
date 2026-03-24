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
        // Calculer le décalage initial
        decalage = iconSelection.anchoredPosition - imageRejouer.anchoredPosition;

        // Placer l'icône correctement sur la première image
        UpdateIconPosition();
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Navigation gauche/droite
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

        // Validation : espace, entrée principale ou pavé numérique
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
        RectTransform targetImage = (selectionIndex == 0) ? imageRejouer : imageMenu;
        iconSelection.anchoredPosition = targetImage.anchoredPosition + decalage;
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