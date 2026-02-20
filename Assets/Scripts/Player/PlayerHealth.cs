using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
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

    private float lastDamageTime = -999f;
    private Coroutine blinkRoutine;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        if (targetSpriteRenderer == null)
        {
            targetSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        maxHealth = Mathf.Clamp(maxHealth, 1, maxHealthCap);
        currentHealth = Mathf.Clamp(startingHealth, 1, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void OnDisable()
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

    public bool TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return false;

        if (Time.time - lastDamageTime < damageCooldown)
            return false;

        lastDamageTime = Time.time;
        currentHealth = Mathf.Max(0, currentHealth - amount);
        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
        }

        blinkRoutine = StartCoroutine(BlinkDuringInvincibility());

        if (currentHealth == 0)
            Died?.Invoke();

        return true;
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        int previous = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (currentHealth != previous)
            HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        lastDamageTime = -999f;

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (targetSpriteRenderer != null)
        {
            targetSpriteRenderer.enabled = true;
        }

        HealthChanged?.Invoke(currentHealth, maxHealth);
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