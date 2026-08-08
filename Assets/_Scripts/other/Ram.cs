using UnityEngine;
using System.Collections.Generic;

public class Ram : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Attacker _attacker; 
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private MovementFsmController _movement;

    [Header("Push settings")]
    [SerializeField] private float _pushForce;
    private bool _isActive = false;

    [Header("Damage settings")]
    [SerializeField] private List<int> _damageLevels = new List<int>();
    [SerializeField] private ParticleSystem _hitEffect;

    public int damage;
    public int _currentLevelIndex = 0;

    private void OnEnable()
    {
        if (_attacker != null)
        {
            _attacker.OwnedAttacksChanged += UpdateDamageLevel;
        }

        UpdateDamageLevel();
    }

    private void OnDisable()
    {
        if (_attacker != null)
        {
            _attacker.OwnedAttacksChanged -= UpdateDamageLevel;
        }
    }

    private void UpdateDamageLevel()
    {
        if (_damageLevels == null || _damageLevels.Count == 0) return;

        if (_currentLevelIndex < _damageLevels.Count - 1)
        {
            _currentLevelIndex++;
            damage = _damageLevels[_currentLevelIndex];

            Debug.Log($"<color=green>Ram Upgrade!</color> Уровень повышен! Теперь индекс: {_currentLevelIndex}, Урон: {damage}");
        }
        else
        {
            Debug.Log("<color=yellow>Ram Max Level!</color> Урон уже на максимуме.");
        }
    }
    private void FixedUpdate()
    {
        _isActive = (_movement.CurrentMode == MovementMode.TankStyle);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Enemy enemy))
        {
            if (_isActive && collision.gameObject.TryGetComponent(out NavMeshPhysics enemyCol))
            {
                enemyCol.ActivatePhysics(transform.forward * _pushForce);
            }

            if (collision.gameObject.TryGetComponent(out HealthComponent health))
            {
                _hitEffect.Play();
                int finalDamage = (int)(_rb.linearVelocity.magnitude * damage);
                health.TakeDamage(finalDamage, DamagesProContainer.DamageNumberPreset.Bleed);
            }
        }
    }
}