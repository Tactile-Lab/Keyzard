using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;

public class TypingSortManager : MonoBehaviour
{
    [Header("Inventaire")]
    [SerializeField] private SpellInventoryManager spellInventory;

    [Header("Ennemis")]
    public List<GameManager.EnemyEntry> listEnemies;

    [Header("Affichage")]
    public TextMeshProUGUI affichage; // TMP principal pour le texte en cours
    public GameObject wordPrefab;     // TMP prefab pour mot terminé

    [Header("Références")]
    public GameManager gameManager;
    public PlayerControler playerController;

    [Header("Animations Lettres")]
    [SerializeField] private float punchScaleAmount = 0.25f;
    [SerializeField] private float punchScaleDuration = 0.15f;
    [SerializeField] private int punchScaleVibrato = 10;
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 25;
    [SerializeField] private float shakeRandomness = 90f;
    [SerializeField] private Color letterColor = Color.cyan;

    [Header("Animations Mot Terminé")]
    [SerializeField] private float wordRiseDistance = 40f;
    [SerializeField] private float wordRiseDuration = 0.6f;
    [SerializeField] private float wordFadeDuration = 0.5f;
    [SerializeField] private Color wordColor = Color.magenta;

    [Header("Particules")]
    public LetterAndWordParticles particlesManager; // Gestion des particules lettres et mots

    private string currentInput = "";
    private GameManager.EnemyEntry selectedEnemy = null;
    private bool sortLibreMode = false;
    private readonly List<Sort> activeSorts = new List<Sort>();
    private bool inventoryMissingWarningLogged;
    private bool gameManagerMissingWarningLogged;

    public GameManager.EnemyEntry SelectedEnemy => selectedEnemy;

    TMP_Text nameEnemy;
    private Tween enemyNameTween;

    private void OnEnable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput += OnTextInput;

        ResolveGameManager();
        RefreshEnemyList();
        ResolveSpellInventory();
        RefreshAvailableSorts();

        if (spellInventory != null)
        {
            spellInventory.InventoryChanged += RefreshAvailableSorts;
        }
    }

    private void OnDisable()
    {
        if (Keyboard.current != null)
            Keyboard.current.onTextInput -= OnTextInput;

        if (spellInventory != null)
        {
            spellInventory.InventoryChanged -= RefreshAvailableSorts;
        }
    }

    private void Start()
    {
        ResolveGameManager();
        RefreshEnemyList();
    }

    private void OnTextInput(char c)
    {
        if (GlossaryToggleController.IsGlossaryOpen || Time.timeScale <= 0f)
            return;

        if (char.IsLetter(c))
            TypeLetter(char.ToUpper(c));
        else if (c == ' ')
            HandleSpace();
    }

    private void TypeLetter(char letter)
    {
        // In bootstrap flows, manager instances may be ready slightly after scene objects.
        ResolveGameManager();
        RefreshEnemyList();

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
                    currentInput = tentative; // 🔹 corrige la première lettre

                    if (enemyCode == tentative)
                    {
                        selectedEnemy = entry;
                        currentInput = ""; // reset si le mot est complet
                    }
                    break;
                }
            }
        }

        // ---------------- Vérifier les sorts ----------------
        if (!matchFound)
        {
            foreach (var sort in activeSorts)
            {
                string sortName = sort.nomSort.ToUpper();
                if (sortName.StartsWith(tentative))
                {
                    matchFound = true;
                    currentInput = tentative; // 🔹 corrige la première lettre
                    break;
                }
            }
        }

        // ---------------- Animation lettre ----------------
        if (matchFound)
            PlayLetterAnimation();
        else
            PlayWrongLetterAnimation(letter);
    }

    private void HandleSpace()
    {
        if (string.IsNullOrEmpty(currentInput) && SelectedEnemy == null)
        {
            FailWordAnimation();
            ResetInput();
            return;
        }

        Sort sortToCast = activeSorts.Find(s => s.nomSort.ToUpper() == currentInput);

        if (sortToCast != null)
        {
            Vector3 spawnPosition = (playerController != null) ? playerController.StaffTipPosition : transform.position;
            GameObject sortInstance = Instantiate(sortToCast.gameObject, spawnPosition, transform.rotation);
            var sortScript = sortInstance.GetComponent<Sort>();

            if (selectedEnemy != null)
                sortScript?.LancerSortCible(selectedEnemy.enemy);
            else
                sortScript?.LancerSort();

            // Animation mot terminé
            PlayWordAnimation(currentInput);

            // Particule mot entier
            particlesManager?.SpawnWordParticle();

            if (affichage != null)
            {
                affichage.color = letterColor;
                affichage.DOColor(wordColor, 0.5f);
            }
        }
        else
        {
            FailWordAnimation();
        }

        // Reset complet
        ResetInput();
    }

    private void ResetInput()
    {
        currentInput = "";
        selectedEnemy = null;
        sortLibreMode = false;

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

    public void ResetInputRoom()
    {
        ResetInput();
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
            affichage.text = $"<color=yellow>{enemyCode}</color> - {currentInput}";
            affichage.color = Color.white;

            nameEnemy = selectedEnemy.enemy.GetComponent<Enemy>().nameText;
            nameEnemy.color = Color.yellow;

            enemyNameTween?.Kill();
            enemyNameTween = nameEnemy.transform.DOScale(Vector3.one * 1.5f, 0.3f).SetEase(Ease.OutCubic);
        }
        else
        {
            affichage.text = currentInput;
            affichage.color = Color.white;

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

    private void PlayLetterAnimation()
    {
        affichage.transform.DOKill(true);
        affichage.transform.localScale = Vector3.one;

        affichage.transform.DOPunchScale(Vector3.one * punchScaleAmount, punchScaleDuration, punchScaleVibrato, 1);
        affichage.transform.DOShakePosition(shakeDuration, shakeStrength, shakeVibrato, shakeRandomness, true);

        affichage.DOColor(letterColor, 0.1f).OnComplete(() =>
        {
            if (selectedEnemy != null)
                affichage.text = $"<color=yellow>{selectedEnemy.code.ToUpper()}</color> - {currentInput}";
            else
                affichage.color = Color.white;
        });

        // Particule lettre
        particlesManager?.SpawnLetterParticleSafe();
    }

    private void PlayWordAnimation(string word)
    {
        if (wordPrefab == null) return;

        GameObject wordObj = Instantiate(wordPrefab, affichage.transform.parent);
        TextMeshProUGUI wordTMP = wordObj.GetComponent<TextMeshProUGUI>();
        wordTMP.text = word;

        RectTransform rt = wordTMP.GetComponent<RectTransform>();
        rt.anchoredPosition = ((RectTransform)affichage.transform).anchoredPosition;
        rt.localScale = Vector3.one;
        wordTMP.alpha = 1f;

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y + wordRiseDistance, wordRiseDuration).SetEase(Ease.OutCubic))
           .Join(wordTMP.DOColor(wordColor, wordRiseDuration))
           .AppendInterval(wordRiseDuration)
           .Append(wordTMP.DOFade(0f, wordFadeDuration))
           .OnComplete(() => Destroy(wordObj));
    }

    private void PlayWrongLetterAnimation(char letter)
    {
        if (wordPrefab == null || affichage == null) return;

        GameObject letterObj = Instantiate(wordPrefab, affichage.transform.parent);
        TextMeshProUGUI tmp = letterObj.GetComponent<TextMeshProUGUI>();
        tmp.text = letter.ToString();
        tmp.color = Color.red;

        RectTransform rt = tmp.GetComponent<RectTransform>();
        RectTransform baseRT = affichage.GetComponent<RectTransform>();
        affichage.ForceMeshUpdate();

        TMP_TextInfo textInfo = affichage.textInfo;
        Vector2 spawnPos = baseRT.anchoredPosition;

        if (textInfo.characterCount > 0)
        {
            int lastIndex = textInfo.characterCount - 1;
            while (lastIndex > 0 && !textInfo.characterInfo[lastIndex].isVisible) lastIndex--;
            TMP_CharacterInfo charInfo = textInfo.characterInfo[lastIndex];
            spawnPos += new Vector2(charInfo.topRight.x, charInfo.topRight.y);
        }

        spawnPos += new Vector2(15f, 0f);
        rt.anchoredPosition = spawnPos;
        rt.localScale = Vector3.one;
        rt.DOKill();
        tmp.DOKill();

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y - 120f, 0.5f).SetEase(Ease.InQuad));
        rt.DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), 0.5f);
        seq.Join(tmp.DOFade(0f, 0.5f));
        seq.OnComplete(() => Destroy(letterObj));
    }

    private void FailWordAnimation()
    {
        if (wordPrefab == null || affichage == null) return;

        affichage.ForceMeshUpdate();
        TMP_TextInfo textInfo = affichage.textInfo;
        RectTransform baseRT = affichage.GetComponent<RectTransform>();

        float delayStep = 0.05f;
        int letterIndex = 0;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible && !char.IsWhiteSpace(charInfo.character)) continue;

            char letter = charInfo.character;

            GameObject letterObj = Instantiate(wordPrefab, affichage.transform.parent);
            TextMeshProUGUI tmp = letterObj.GetComponent<TextMeshProUGUI>();
            tmp.text = letter.ToString();
            tmp.color = Color.red;

            RectTransform rt = tmp.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.DOKill();
            tmp.DOKill();

            Vector2 spawnPos = baseRT.anchoredPosition + new Vector2(charInfo.origin + 2f, charInfo.baseLine - 2f);
            rt.anchoredPosition = spawnPos;

            Sequence seq = DOTween.Sequence();
            seq.PrependInterval(letterIndex * delayStep);
            seq.Append(rt.DOAnchorPosY(rt.anchoredPosition.y - 120f, 0.3f).SetEase(Ease.InQuad));
            seq.Join(tmp.DOFade(0f, 0.3f));
            seq.OnComplete(() => Destroy(letterObj));

            letterIndex++;
        }
    }

    private void ResolveSpellInventory()
    {
        if (spellInventory == null)
        {
            spellInventory = SpellInventoryManager.Instance;
        }

        if (spellInventory == null && !inventoryMissingWarningLogged)
        {
            inventoryMissingWarningLogged = true;
            Debug.LogError("[TypingSortManager] SpellInventoryManager introuvable. Ajoute-le dans la scene de depart.");
        }
    }

    private void ResolveGameManager()
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
        }

        if (gameManager == null && !gameManagerMissingWarningLogged)
        {
            gameManagerMissingWarningLogged = true;
            Debug.LogError("[TypingSortManager] GameManager introuvable. Verifie CoreSystemsBootstrap et le prefab GameManager.");
        }
    }

    private void RefreshEnemyList()
    {
        if (gameManager != null)
        {
            listEnemies = gameManager.list_enemies;
        }
    }

    private void RefreshAvailableSorts()
    {
        activeSorts.Clear();

        if (spellInventory != null)
        {
            IReadOnlyList<Sort> unlocked = spellInventory.GetUnlockedSorts();
            for (int i = 0; i < unlocked.Count; i++)
            {
                if (unlocked[i] != null)
                {
                    activeSorts.Add(unlocked[i]);
                }
            }
        }
    }
}