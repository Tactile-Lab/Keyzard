using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class TypingSortManager : MonoBehaviour
{
    [Header("Sorts disponibles")]
    public List<Sort> sorts = new List<Sort>();

    [Header("Ennemis")]
    public List<GameManager.EnemyEntry> listEnemies;

    [Header("Affichage")]
    public TextMeshProUGUI affichage; // TMP principal pour le texte en cours
    public GameObject wordPrefab;     // TMP prefab pour mot terminé

    public GameManager gameManager;

    private string currentInput = "";         // texte que le joueur tape
    private GameManager.EnemyEntry selectedEnemy = null;
    private bool sortLibreMode = false;

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
            listEnemies = gameManager.list_enemies;
    }

    private void OnTextInput(char c)
    {
        if (char.IsLetter(c))
            TypeLetter(char.ToUpper(c));
        else if (c == ' ')
            HandleSpace();
    }

    private void TypeLetter(char letter)
    {
        if (!sortLibreMode && (listEnemies == null || listEnemies.Count == 0))
            return;

        string tentative = currentInput + letter;
        bool matchFound = false;

        // Vérifier ennemis si aucun sélectionné
        if (selectedEnemy == null && !sortLibreMode)
        {
            foreach (var entry in listEnemies)
            {
                if (entry.code.ToUpper().StartsWith(tentative))
                {
                    matchFound = true;
                    if (entry.code.ToUpper() == tentative)
                        selectedEnemy = entry;
                    currentInput = tentative;
                    break;
                }
            }
        }

        // Vérifier sorts
        if (!matchFound)
        {
            foreach (var sort in sorts)
            {
                if (sort.nomSort.ToUpper().StartsWith(tentative))
                {
                    matchFound = true;
                    currentInput = tentative;
                    break;
                }
            }
        }

        // Animation lettre
        if (matchFound)
            PlayLetterAnimation();
    }

    private void HandleSpace()
    {
        if (string.IsNullOrEmpty(currentInput) && selectedEnemy == null)
            return;

        Sort sortToCast = sorts.Find(s => s.nomSort.ToUpper() == currentInput);

        if (sortToCast != null)
        {
            GameObject sortInstance = Instantiate(sortToCast.gameObject, transform.position, transform.rotation);
            var sortScript = sortInstance.GetComponent<Sort>();

            if (selectedEnemy != null)
            {
                if (sortScript != null)
                    sortScript.LancerSortCible(selectedEnemy.enemy);
            }
            else
            {
                if (sortScript != null)
                    sortScript.LancerSort();
            }

            // Lance animation mot terminé
            PlayWordAnimation(currentInput);

            // Reset input pour le mot suivant
            currentInput = "";
            selectedEnemy = null;
            sortLibreMode = false;
        }
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

    // ----------------- Animations -----------------

    private void PlayLetterAnimation()
    {
        affichage.transform.DOKill(true);
        affichage.DOKill(true);

        affichage.transform.localScale = Vector3.one;

        affichage.transform.DOPunchScale(Vector3.one * 0.25f, 0.15f, 10, 1);
        affichage.DOColor(Color.cyan, 0.1f)
                 .OnComplete(() => affichage.DOColor(Color.white, 0.2f));
        affichage.transform.DOShakePosition(0.1f, 5f, 20);
    }

    private void PlayWordAnimation(string word)
    {
        if (wordPrefab == null)
        {
            Debug.LogWarning("WordPrefab non assigné !");
            return;
        }

        // Instancie un TMP temporaire pour le mot terminé
        GameObject wordObj = Instantiate(wordPrefab, affichage.transform.parent);
        TextMeshProUGUI wordTMP = wordObj.GetComponent<TextMeshProUGUI>();
        wordTMP.text = word;

        RectTransform rt = wordTMP.GetComponent<RectTransform>();
        rt.anchoredPosition = ((RectTransform)affichage.transform).anchoredPosition;
        rt.localScale = Vector3.one;
        wordTMP.alpha = 1f; // s'assure que le mot est visible

        // Animation : monte + magenta + reste visible + fade out
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + 40f, 0.6f).SetEase(Ease.OutCubic))
           .Join(wordTMP.DOColor(Color.magenta, 0.6f))
           .AppendInterval(0.6f)                 // mot reste visible
           .Append(wordTMP.DOFade(0f, 0.5f))     // disparaît en fondu
           .OnComplete(() => Destroy(wordObj));
    }
}
