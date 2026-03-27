using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Gère la vie du joueur : dégâts, soin, invincibilité temporaire et notification d'état.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    private const float NoDamageTakenYet = float.NegativeInfinity;

    [Header("Health")]
    [SerializeField] private int startingHealth = 30;
    [SerializeField] private int maxHealth = 30;
    [SerializeField] private int maxHealthCap = 60;
    private int currentHealth;

    [Header("Damage")]
    [SerializeField] private float damageCooldown = 2f;
    [SerializeField] private float blinkInterval = 0.12f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer targetSpriteRenderer;

    // Temps du dernier dégât reçu, utilisé pour gérer la fenêtre d'invincibilité.
    private float lastDamageTime = NoDamageTakenYet;
    private Coroutine blinkRoutine;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        // Fallback automatique si aucun renderer n'a été assigné dans l'inspecteur.
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        maxHealth = Mathf.Clamp(maxHealth, 1, maxHealthCap);
        currentHealth = Mathf.Clamp(startingHealth, 1, maxHealth);
        RaiseHealthChanged();
    }

    private void OnDisable()
    {
        StopBlinkAndRestoreSprite();
    }

    /// <summary>
    /// Applique des dégâts si possible (cooldown respecté, joueur vivant).
    /// </summary>
    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return false;
        }

        if (Time.time - lastDamageTime < damageCooldown)
        {
            return false;
        }

        lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        AudioManager.Instance?.PlaySFXEvent(SFXEventKey.PlayerHurt);
        RaiseHealthChanged();

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        blinkRoutine = StartCoroutine(BlinkDuringInvincibility());

        if (currentHealth == 0)
        {
            AudioManager.Instance?.PlaySFXEvent(SFXEventKey.PlayerDeath);
            Died?.Invoke();
        }

        return true;
    }

    /// <summary>
    /// Soigne le joueur sans dépasser sa vie maximale.
    /// </summary>
    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
        {
            return;
        }

        int previous = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (currentHealth != previous)
        {
            RaiseHealthChanged();
        }
    }

    /// <summary>
    /// Réinitialise la vie au maximum et annule l'état visuel d'invincibilité.
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        lastDamageTime = NoDamageTakenYet;

        StopBlinkAndRestoreSprite();

        RaiseHealthChanged();
    }

    private void RaiseHealthChanged()
    {
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void StopBlinkAndRestoreSprite()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.enabled = true;
        }
    }

    private IEnumerator BlinkDuringInvincibility()
    {
        if (targetSpriteRenderer == null)
        {
            yield break;
        }

        while (Time.time - lastDamageTime < damageCooldown && !IsDead)
        {
            targetSpriteRenderer.enabled = !targetSpriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        targetSpriteRenderer.enabled = true;
        blinkRoutine = null;
    }
}