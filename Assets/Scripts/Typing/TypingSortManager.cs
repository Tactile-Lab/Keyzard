using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using TMPro;

public class TypingSortManager : MonoBehaviour
{
    [Header("Sorts disponibles")]
    public List<Sort> sorts = new List<Sort>();

    [Header("Affichage")]
    public TextMeshProUGUI affichage;

    private string currentInput = "";
    private Keyboard keyboard;

    void Start()
    {
        keyboard = Keyboard.current;
    }

    void Update()
    {
        if (keyboard == null) return;

        // Vérifie si une touche a été pressée
        foreach (KeyControl keyControl in keyboard.allKeys)
        {
            if (keyControl.wasPressedThisFrame)
            {
                TraiterTouche(keyControl);
            }
        }

        MettreAJourAffichage();
    }

    void TraiterTouche(KeyControl keyControl)
    {
        // Espace : lancer sort ou reset
        if (keyControl.keyCode == Key.Space)
        {
            GestionEspace();
            return;
        }

        char lettre = KeyControlToChar(keyControl);

        if (lettre != '\0')
        {
            TaperLettre(lettre);
        }
    }

    void TaperLettre(char lettre)
    {
        string tentative = currentInput + lettre;
        bool correspond = false;

        // Vérifie si au moins un sort commence par la saisie actuelle
        foreach (Sort sort in sorts)
        {
            if (sort.nomSort.StartsWith(tentative))
            {
                correspond = true;
                break;
            }
        }

        if (correspond)
        {
            currentInput = tentative;
        }
        // Sinon on ignore la lettre (pas d'erreur affichée)
    }

    void GestionEspace()
    {
        foreach (Sort sort in sorts)
        {
            if (sort.nomSort == currentInput)
            {
                // Lance le sort
                // sort.Lancer();
                Debug.Log("Sort " + sort.nomSort + " lancée !!!!");
                ResetInput();
                return;
            }
        }

        // Si aucun sort ne correspond exactement, on reset juste
        ResetInput();
    }

    void ResetInput()
    {
        currentInput = "";
    }

    void MettreAJourAffichage()
    {
        if (affichage != null)
        {
            affichage.text = currentInput;
        }
    }

    char KeyControlToChar(KeyControl keyControl)
    {
        string nom = keyControl.displayName;

        // Si c'est une lettre (A-Z)
        if (nom.Length == 1 && char.IsLetter(nom[0]))
        {
            return char.ToLower(nom[0]);
        }

        return '\0';
    }
}
