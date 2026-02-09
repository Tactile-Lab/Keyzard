using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

public class TypingSortManager : MonoBehaviour
{
    [Header("Sorts disponibles")]
    public List<Sort> sorts = new List<Sort>();

    [Header("Ennemis")]
    public List<GameManager.EnemyEntry> listEnemies;

    [Header("Affichage")]
    public TextMeshProUGUI affichage;

    public GameManager gameManager;

    private string currentInput = "";
    private GameManager.EnemyEntry selectedEnemy = null;
    private bool sortLibreMode = false; // true si on tape un sort sans cible

    private void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;
    }
    private void Start()
    {
        if (gameManager != null)
            listEnemies = gameManager.list_enemies; // juste la référence
    }


    private void OnTextInput(char c)
    {
        if (char.IsLetter(c))
        {
            TypeLetter(char.ToUpper(c));
        }
        else if (c == ' ')
        {
            HandleSpace();
        }
    }

    private void TypeLetter(char letter)
{
    // ⚠️ Bloquer si aucun ennemi et pas de sort libre possible
    if (!sortLibreMode && (listEnemies == null || listEnemies.Count == 0))
        return;

    string tentative = currentInput + letter;
    bool matchFound = false;

    // 1️⃣ Vérifier l'ennemi si aucun sélectionné et pas en mode libre
    if (selectedEnemy == null && !sortLibreMode)
    {
        foreach (var entry in listEnemies)
        {
            if (entry.code.ToUpper().StartsWith(tentative))
            {
                matchFound = true;

                // Ennemi complètement tapé
                if (entry.code.ToUpper() == tentative)
                {
                    selectedEnemy = entry;
                    Debug.Log("Ennemi sélectionné : " + selectedEnemy.code);
                    currentInput = ""; // reset input après sélection
                }
                else
                {
                    currentInput = tentative; // input partiel correct
                }
                break;
            }
        }
    }

    // 2️⃣ Vérifier les sorts
    if (!matchFound)
    {
        foreach (var sort in sorts)
        {
            if (sort.nomSort.ToUpper().StartsWith(tentative))
            {
                matchFound = true;
                currentInput = tentative; // input partiel correct pour un sort
                break;
            }
        }
    }

    // 3️⃣ Si aucune correspondance → ignorer la lettre (currentInput inchangé)
}


    private void HandleSpace()
    {
        if (string.IsNullOrEmpty(currentInput) && selectedEnemy == null)
            return;

        // Cherche le sort correspondant à l'input complet
        Sort sortToCast = sorts.Find(s => s.nomSort.ToUpper() == currentInput);

        if (sortToCast != null)
        {
            // Instancie le prefab du sort
            GameObject sortInstance = Instantiate(sortToCast.gameObject); // le prefab du sort

            // Si une cible est sélectionnée
            if (selectedEnemy != null)
            {
                // Appelle la méthode pour lancer le sort sur cible
                var sortScript = sortInstance.GetComponent<Sort>();
                if (sortScript != null)
                    sortScript.LancerSortCible(selectedEnemy.enemy);
            }
            else
            {
                // Appelle la méthode pour lancer le sort libre
                var sortScript = sortInstance.GetComponent<Sort>();
                if (sortScript != null)
                    sortScript.LancerSort();
            }
        }
        ResetInput();
    }




    private void ResetInput()
    {
        currentInput = "";
        selectedEnemy = null;
        sortLibreMode = false;
    }

    private void Update()
    {
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (affichage == null) return;

        if (selectedEnemy != null)
            affichage.text = selectedEnemy.code + " - " + currentInput;
        else
            affichage.text = currentInput;
    }
}
