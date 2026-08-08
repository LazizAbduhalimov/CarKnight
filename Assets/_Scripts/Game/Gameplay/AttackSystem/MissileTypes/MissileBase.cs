using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MissileColisionProvider))]
public abstract class MissileBase : MonoBehaviour
{
    public float Speed = 10f;
    public float Damage = 2f;

    private TrailRenderer[] _trailRenderers;
    protected MissileColisionProvider MissileColisionProvider;

    protected virtual void Awake()
    {
        _trailRenderers = GetComponentsInChildren<TrailRenderer>();
        MissileColisionProvider = GetComponent<MissileColisionProvider>();
        if (MissileColisionProvider != null)
        {
            MissileColisionProvider.Exploded += OnMissileExploded;
        }
    }

    public virtual void Init(float speed, float lifeTime = 5f)
    {
        Speed = speed;
        MissileColisionProvider?.SetExplodeOnTriggerEnter(true);
    }

    public virtual void SetDamage(float damage)
    {
        Damage = Mathf.Max(0f, damage);
    }

    protected virtual void OnEnable()
    {
        StartCoroutine(ClearTrailNextFrame());
    }

    protected virtual void OnDestroy()
    {
        if (MissileColisionProvider != null)
        {
            MissileColisionProvider.Exploded -= OnMissileExploded;
        }
    }

    private IEnumerator ClearTrailNextFrame()
    {
        // очищаем трейл, чтобы не было артефактов от предыдущего использования. На всякий случай 2 раза
        if (_trailRenderers != null)
        {
            foreach (var trailRenderer in _trailRenderers)
            {
                trailRenderer.Clear();
            }
            yield return null;
            foreach (var trailRenderer in _trailRenderers)
            {
                trailRenderer.Clear();
            }
        }
    }

    private void OnMissileExploded(MissileImpactData impactData)
    {
        AttackDamageService.ApplyMissileImpactDamage(
            impactData,
            Mathf.RoundToInt(Damage),
            DamagesProContainer.DamageNumberPreset.Flames);
    }
}
