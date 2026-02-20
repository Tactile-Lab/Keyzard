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
    public PlayerControler playerController;

    private string currentInput = "";         // texte que le joueur tape
    private GameManager.EnemyEntry selectedEnemy = null;
    private bool sortLibreMode = false;

    public GameManager.EnemyEntry SelectedEnemy => selectedEnemy;

    TMP_Text nameEnemy;
    private Tween enemyNameTween; // Tween pour le zoom du nom

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

        // ---------------- Vérifier les ennemis ----------------
        if (selectedEnemy == null && !sortLibreMode)
        {
            foreach (var entry in listEnemies)
            {
                string enemyCode = entry.code.ToUpper();
                if (enemyCode.StartsWith(tentative))
                {
                    matchFound = true;

                    // Si le code est complet, on sélectionne l'ennemi
                    if (enemyCode == tentative)
                    {
                        selectedEnemy = entry;
                        currentInput = ""; // On reset pour taper le sort après
                    }
                    else
                    {
                        currentInput = tentative; // On continue à taper le code
                    }

                    break;
                }
            }
        }

        // ---------------- Vérifier les sorts ----------------
        if (!matchFound)
        {
            foreach (var sort in sorts)
            {
                string sortName = sort.nomSort.ToUpper();
                if (sortName.StartsWith(tentative))
                {
                    matchFound = true;
                    currentInput = tentative;
                    break;
                }
            }
        }

        // ---------------- Animation lettre ----------------
        if (matchFound)
            PlayLetterAnimation();
    }

    private void HandleSpace()
    {
        // Si rien n'est tapé et pas d'ennemi sélectionné, on ne fait rien
        if (string.IsNullOrEmpty(currentInput) && selectedEnemy == null)
            return;

        // Cherche un sort correspondant au texte actuel
        Sort sortToCast = sorts.Find(s => s.nomSort.ToUpper() == currentInput);

        if (sortToCast != null)
        {
            // Get staff tip position from player controller
            Vector3 spawnPosition = (playerController != null) 
                ? playerController.StaffTipPosition 
                : transform.position;

            // Instancie le sort au bout du bâton
            GameObject sortInstance = Instantiate(sortToCast.gameObject, spawnPosition, transform.rotation);
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

            // Animation mot terminé
            PlayWordAnimation(currentInput);
        }

        // ---------------- Reset complet ----------------
        currentInput = "";
        selectedEnemy = null;
        sortLibreMode = false;

        // Si un nom est sélectionné, on remet taille et couleur progressivement
        if (nameEnemy != null)
        {
            enemyNameTween?.Kill();
            // Tween de dézoom et couleur en simultané
            Sequence seq = DOTween.Sequence();
            seq.Append(nameEnemy.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic));
            seq.Join(nameEnemy.DOColor(Color.white, 0.5f));
            enemyNameTween = seq;
            nameEnemy = null;
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
        {
            string enemyCode = selectedEnemy.code.ToUpper();
            string restText = currentInput;

            // Colorier les 2 premières lettres en jaune
            affichage.text = $"<color=yellow>{enemyCode}</color> - {restText}";
            nameEnemy = selectedEnemy.enemy.GetComponent<Enemy>().nameText;

            // Couleur
            nameEnemy.color = Color.yellow;

            // ---------------- Zoom plus fort ----------------
            enemyNameTween?.Kill();
            enemyNameTween = nameEnemy.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutCubic);
        }
        else
        {
            affichage.text = currentInput;

            // Si un nom existe encore, dézoom + couleur progressive
            if (nameEnemy != null)
            {
                enemyNameTween?.Kill();
                Sequence seq = DOTween.Sequence();
                seq.Append(nameEnemy.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutCubic));
                seq.Join(nameEnemy.DOColor(Color.white, 0.5f));
                enemyNameTween = seq;
                nameEnemy = null;
            }
        }
    }

    // ----------------- Animations -----------------

    private void PlayLetterAnimation()
    {
        affichage.transform.DOKill(true);
        affichage.transform.localScale = Vector3.one;

        affichage.transform.DOPunchScale(Vector3.one * 0.25f, 0.15f, 10, 1);
        affichage.DOColor(Color.cyan, 0.1f)
                .OnComplete(() => 
                {
                    // Remet le texte avec la couleur des 2 premières lettres
                    if (selectedEnemy != null)
                        affichage.text = $"<color=yellow>{selectedEnemy.code.ToUpper()}</color>{currentInput}";
                    else
                        affichage.color = Color.white;
                });
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