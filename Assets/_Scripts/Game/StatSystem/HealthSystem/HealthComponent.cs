using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class HealthComponent : MonoBehaviour
{
    public int MaxHealth = 100;
    public int CurrentHealth;
    public Action<int, int, int> OnHealthChanged;
    public Action OnHealthHeal;
    public Action OnDeath;
    [Inject(Optional = true)] private IDamageNumberService _damageNumberService;

    private static readonly string[] EnemyDeathTexts =
    {
        "Kaboom!",
        "Boom!",
        "Eliminated!",
        "Shattered!",
        "Wrecked!"
    };

    private void OnEnable()
    {
        CurrentHealth = MaxHealth;
    }

    public void SetMaxHealth(int maxHealth, bool resetCurrentHealth)
    {
        var sanitizedMaxHealth = Math.Max(1, maxHealth);
        MaxHealth = sanitizedMaxHealth;

        var oldHealth = CurrentHealth;
        CurrentHealth = resetCurrentHealth
            ? sanitizedMaxHealth
            : Mathf.Clamp(CurrentHealth, 0, sanitizedMaxHealth);

        if (oldHealth != CurrentHealth)
        {
            OnHealthChanged?.Invoke(oldHealth, CurrentHealth, MaxHealth);
        }
    }

    public void TakeDamage(int damage, DamagesProContainer.DamageNumberPreset damagePreset = DamagesProContainer.DamageNumberPreset.SpreadUp)
    {
        Debug.Log($"{gameObject.name} takes {damage} damage.");
        var oldHealth = CurrentHealth;
        CurrentHealth -= damage;
        OnHealthChanged?.Invoke(oldHealth, CurrentHealth, MaxHealth);

        if (damage > 0)
        {
            ResolveDamageNumberService()?.SpawnNumber(damagePreset, transform.position.AddY(1f), damage);
        }

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healthToHeal)
    {

        var oldHealth = CurrentHealth;

        CurrentHealth = Math.Min(CurrentHealth + healthToHeal, MaxHealth);

        OnHealthChanged?.Invoke(oldHealth, CurrentHealth, MaxHealth);
        OnHealthHeal?.Invoke();
    }

    private void Die()
    {
        TryShowDeathText();
        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} has died.");
        gameObject.SetActive(false);
    }

    private void TryShowDeathText()
    {
        var damageService = ResolveDamageNumberService();
        if (damageService == null)
        {
            return;
        }

        if (LayerMask.LayerToName(gameObject.layer) != "Enemy")
        {
            return;
        }

        var randomIndex = UnityEngine.Random.Range(0, EnemyDeathTexts.Length);
        var text = EnemyDeathTexts[randomIndex];
        damageService.SpawnText(DamagesProContainer.DamageNumberPreset.Comic, transform.position.AddY(1.8f), text);
    }

    private IDamageNumberService ResolveDamageNumberService()
    {
        if (_damageNumberService != null)
        {
            return _damageNumberService;
        }

        if (ProjectContext.HasInstance)
        {
            _damageNumberService = ProjectContext.Instance.Container.TryResolve<IDamageNumberService>();
        }

        if (_damageNumberService == null)
        {
            var sceneContext = FindAnyObjectByType<SceneContext>();
            if (sceneContext != null)
            {
                _damageNumberService = sceneContext.Container.TryResolve<IDamageNumberService>();
            }
        }

        return _damageNumberService;
    }
}
