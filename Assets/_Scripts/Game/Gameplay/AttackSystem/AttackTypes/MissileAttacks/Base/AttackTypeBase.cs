using System;
using OnlyFun.EventBus;
using PoolSystem.Alternative;
using UnityEngine;

[Serializable]
public abstract class AttackTypeBase : IAttackRuntime
{
    protected PoolService PoolService { get; private set; }
    protected EnemiesNearbyFinder EnemiesNearby { get; private set; }
    protected EventBus EventBus { get; private set; }
    protected IGameAudioService Audio { get; private set; }
    private AttackShotAudioDefinition _shotAudio;
    private bool _shotAudioWarningShown;
    
    public Transform Owner { get; set; }


    [Header("Base Settings")]
    public float AttackRate;
    [SerializeField] private float _baseDamage;
    private float _damageMultiplier = 1f;

    public float Damage
    {
        get => Mathf.Max(0f, _baseDamage * _damageMultiplier);
        set => _baseDamage = Mathf.Max(0f, value);
    }
    
    protected Transform Target;
    protected float PassedTime;

    public float CooldownDuration => Mathf.Max(0.0001f, AttackRate);
    public float CooldownElapsed => Mathf.Clamp(PassedTime, 0f, CooldownDuration);
    public float CooldownRemaining => Mathf.Max(0f, CooldownDuration - CooldownElapsed);
    public float CooldownRemainingNormalized => CooldownDuration <= 0f ? 0f : CooldownRemaining / CooldownDuration;
    public float CooldownProgressNormalized => CooldownDuration <= 0f ? 1f : CooldownElapsed / CooldownDuration;

    public virtual void PreInit() {}

    public virtual void Initialize(AttackRuntimeContext context)
    {
        PreInit();
        PoolService = context.PoolService;
        EnemiesNearby = context.EnemiesNearby;
        Owner = context.Owner;
        EventBus = context.EventBus;
        Audio = context.Audio;
        // Debug.Log($"Initializing attack: {GetType().Name} for owner: {Owner.name}");
        PostInit();
    }

    public virtual void PostInit() {}

    public virtual void Tick()
    {
        PassedTime += Time.deltaTime;
        if (PassedTime >= CooldownDuration)
        {
            Target = EnemiesNearby.GetNearestEnemy(Owner.position);
            if (Target == null) return;
            Attack();
            PassedTime = 0;
        }
    }

    public void SetDamageMultiplier(float multiplier)
    {
        _damageMultiplier = Mathf.Max(0f, multiplier);
    }

    public void SetShotAudio(AttackShotAudioDefinition shotAudio)
    {
        _shotAudio = shotAudio;
    }

    public void Attack()
    {
        if (ExecuteAttack()) PlayShotSound(Owner.position);
    }

    protected virtual bool ExecuteAttack() => false;

    protected void PlayShotSound(Vector3 position)
    {
        if (_shotAudio == null || !_shotAudio.HasClips)
        {
            if (!_shotAudioWarningShown)
            {
                Debug.LogWarning($"[SOUND] Shot audio is not configured for attack runtime '{GetType().Name}'.");
                _shotAudioWarningShown = true;
            }

            return;
        }

        if (Audio == null)
        {
            Debug.LogError($"[SOUND] Shot audio service is not available for attack runtime '{GetType().Name}'.");
            return;
        }

        Audio.PlayAt(_shotAudio, position);
    }

    public virtual void Dispose() {}
}
