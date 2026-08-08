using UnityEngine;

public sealed class AttackRuntimeFactory
{
    public IAttackRuntime CreateRuntime(AttackSO attackConfig, int level, AttackRuntimeContext context)
    {
        if (attackConfig == null || attackConfig.Spec == null)
        {
            return null;
        }

        var levelData = AttackLevelTableUtility.GetLevelOrNull(attackConfig.LevelTable, level);
        var runtime = CreateRuntimeBySpec(attackConfig.Spec);
        if (runtime == null)
        {
            return null;
        }

        ApplySharedStats(runtime, attackConfig.Spec, levelData);
        ApplyTypeSpecificStats(runtime, attackConfig.Spec, level, levelData);
        if (runtime is AttackTypeBase attackRuntime)
            attackRuntime.SetShotAudio(attackConfig.ShotAudio);
        runtime.Initialize(context);
        return runtime;
    }

    private static IAttackRuntime CreateRuntimeBySpec(AttackSpecBase spec)
    {
        return spec switch
        {
            ArcMissileAttackSpec => new ArcAttack(),
            AutoAimMissileAttackSpec => new AutoAimAttack(),
            MortarMissileAttackSpec => new MortarAttack(),
            PatternedMissileAttackSpec => new PatternedMissileAttack(),
            RadialArcMissileAttackSpec => new RadialArcAttack(),
            SpiralHomingMissileAttackSpec => new SpiralHomingAttack(),
            BeamRaycastAttackSpec => new RaycastMissileAttack(),
            ForwardSphereBurstRaycastAttackSpec => new ForwardSphereBurstRaycastAttack(),
            MachineGunRaycastAttackSpec => new MachineGunRaycastAttack(),
            LightningRaycastAttackSpec => new LightningAttack(),
            MagneticFieldAttackSpec => new MagneticFieldAttack(),
            BonusRandomMissileAttackSpec => new BonusRandomMissileAttack(),
            GlobalLightningBonusAttackSpec => new GlobalLightningBonusAttack(),
            ArtilleryFireZoneBonusAttackSpec => new ArtilleryFireZoneBonusAttack(),
            LowHealthDamageBonusAttackSpec => new LowHealthDamageBonusAttack(),
            RotatingBeamsBonusAttackSpec => new RotatingBeamsBonusAttack(),
            _ => null
        };
    }

    private static void ApplySharedStats(IAttackRuntime runtime, AttackSpecBase spec, AttackLevelData levelData)
    {
        if (runtime is not AttackTypeBase attackRuntime)
        {
            return;
        }

        var fallbackAttackRate = GetDefaultAttackRate(spec);
        var fallbackDamage = GetDefaultDamage(spec);

        attackRuntime.AttackRate = levelData != null ? levelData.AttackRate : fallbackAttackRate;
        attackRuntime.Damage = levelData != null ? levelData.Damage : fallbackDamage;
    }

    private static void ApplyTypeSpecificStats(IAttackRuntime runtime, AttackSpecBase spec, int level, AttackLevelData levelData)
    {
        switch (runtime)
        {
            case ArcAttack arc when spec is ArcMissileAttackSpec arcSpec:
                arc.MissilePrefab = arcSpec.MissilePrefab;
                arc.MissileSpeed = levelData != null ? levelData.MissileSpeed : arcSpec.DefaultMissileSpeed;
                arc.ProjectilesPerAttack = levelData != null ? levelData.ProjectilesPerAttack : arcSpec.DefaultProjectilesPerAttack;
                arc.DelayBetweenProjectiles = levelData != null ? levelData.DelayBetweenProjectiles : arcSpec.DefaultDelayBetweenProjectiles;
                arc.ExplosionDiameter = levelData != null ? levelData.Radius : arcSpec.DefaultRadius;
                arc.RadiusIncreasePerShot = arcSpec.RadiusIncreasePerShot;
                arc.ArcHeight = arcSpec.ArcHeight;
                break;

            case AutoAimAttack autoAim when spec is AutoAimMissileAttackSpec autoAimSpec:
                autoAim.MissilePrefab = autoAimSpec.MissilePrefab;
                autoAim.MissileSpeed = levelData != null ? levelData.MissileSpeed : autoAimSpec.DefaultMissileSpeed;
                autoAim.TurnRatePerSecond = autoAimSpec.TurnRatePerSecond;
                break;

            case MortarAttack mortar when spec is MortarMissileAttackSpec mortarSpec:
                mortar.MissilePrefab = mortarSpec.MissilePrefab;
                mortar.MissileSpeed = levelData != null ? levelData.MissileSpeed : mortarSpec.DefaultMissileSpeed;
                mortar.ProjectilesPerAttack = levelData != null ? levelData.ProjectilesPerAttack : mortarSpec.DefaultProjectilesPerAttack;
                mortar.DelayBetweenProjectiles = levelData != null ? levelData.DelayBetweenProjectiles : mortarSpec.DefaultDelayBetweenProjectiles;
                mortar.ExplosionDiameter = levelData != null ? levelData.Radius : mortarSpec.DefaultRadius;
                mortar.RadiusIncreasePerShot = mortarSpec.RadiusIncreasePerShot;
                mortar.MaxHeight = mortarSpec.MaxHeight;
                mortar.MinTravelTime = mortarSpec.MinTravelTime;
                break;

            case PatternedMissileAttack patterned when spec is PatternedMissileAttackSpec patternedSpec:
                patterned.MissilePrefab = patternedSpec.MissilePrefab;
                patterned.MissileSpeed = levelData != null ? levelData.MissileSpeed : patternedSpec.DefaultMissileSpeed;
                patterned.Amplitude = patternedSpec.Amplitude;
                patterned.NoiseScale = patternedSpec.NoiseScale;
                patterned.NoiseSpeed = patternedSpec.NoiseSpeed;
                break;

            case RadialArcAttack radial when spec is RadialArcMissileAttackSpec radialSpec:
                radial.MissilePrefab = radialSpec.MissilePrefab;
                radial.MissileSpeed = levelData != null ? levelData.MissileSpeed : radialSpec.DefaultMissileSpeed;
                radial.ProjectilesPerAttack = levelData != null ? levelData.ProjectilesPerAttack : radialSpec.DefaultProjectilesPerAttack;
                radial.DelayBetweenProjectiles = levelData != null ? levelData.DelayBetweenProjectiles : radialSpec.DefaultDelayBetweenProjectiles;
                radial.ExplosionDiameter = levelData != null ? levelData.Radius : radialSpec.DefaultRadius;
                radial.ArcHeight = radialSpec.ArcHeight;
                radial.ProjectilesPerCircle = radialSpec.ProjectilesPerCircle;
                radial.DelayBetweenCircleProjectiles = radialSpec.DelayBetweenCircleProjectiles;
                radial.CircleRadius = radialSpec.CircleRadius;
                radial.AngleOffsetDegrees = radialSpec.AngleOffsetDegrees;
                radial.UseOwnerForwardAsBaseAngle = radialSpec.UseOwnerForwardAsBaseAngle;
                break;

            case SpiralHomingAttack spiral when spec is SpiralHomingMissileAttackSpec spiralSpec:
                spiral.MissilePrefab = spiralSpec.MissilePrefab;
                spiral.MissileSpeed = levelData != null ? levelData.MissileSpeed : spiralSpec.DefaultMissileSpeed;
                spiral.MinFlightTime = spiralSpec.MinFlightTime;
                break;

            case RaycastMissileAttack beam when spec is BeamRaycastAttackSpec beamSpec:
                beam.RayLine = beamSpec.BeamLinePrefab;
                beam.RayDistance = beamSpec.BeamDistance;
                beam.RayRadius = beamSpec.BeamRadius;
                beam.RayMask = beamSpec.BeamMask;
                beam.RayLifetime = beamSpec.BeamLifetime;
                break;

            case ForwardSphereBurstRaycastAttack forwardBurst when spec is ForwardSphereBurstRaycastAttackSpec forwardBurstSpec:
                forwardBurst.Range = forwardBurstSpec.Range;
                forwardBurst.Radius = forwardBurstSpec.Radius;
                forwardBurst.BurstDuration = forwardBurstSpec.BurstDuration;
                forwardBurst.SphereCastCount = forwardBurstSpec.SphereCastCount;
                forwardBurst.MuzzleHeight = forwardBurstSpec.MuzzleHeight;
                forwardBurst.FlattenDirectionToGround = forwardBurstSpec.FlattenDirectionToGround;
                forwardBurst.HitMask = forwardBurstSpec.HitMask;
                forwardBurst.BeamPrefab = forwardBurstSpec.BeamPrefab;
                forwardBurst.DamagePreset = forwardBurstSpec.DamagePreset;
                forwardBurst.DrawDebugRays = forwardBurstSpec.DrawDebugRays;
                break;

            case MachineGunRaycastAttack machineGun when spec is MachineGunRaycastAttackSpec machineGunSpec:
                machineGun.Range = machineGunSpec.Range;
                machineGun.BulletRadius = machineGunSpec.BulletRadius;
                machineGun.SpreadRadius = machineGunSpec.SpreadRadius;
                machineGun.ForwardConeHalfAngle = machineGunSpec.ForwardConeHalfAngle;
                machineGun.MuzzleHeight = machineGunSpec.MuzzleHeight;
                machineGun.HitMask = machineGunSpec.HitMask;
                machineGun.DamagePreset = machineGunSpec.DamagePreset;
                machineGun.DrawDebugRays = machineGunSpec.DrawDebugRays;
                machineGun.TracerTravelDuration = machineGunSpec.TracerTravelDuration;
                machineGun.TracerLifetime = machineGunSpec.TracerLifetime;
                machineGun.UseDefaultImpactIfMissing = machineGunSpec.UseDefaultImpactIfMissing;
                break;

            case LightningAttack lightning when spec is LightningRaycastAttackSpec lightningSpec:
                lightning.ChainRadius = lightningSpec.ChainRadius;
                lightning.ChainInterval = lightningSpec.ChainInterval;
                lightning.MaxBounces = lightningSpec.MaxBounces;
                lightning.ImpactRadius = levelData != null ? levelData.Radius : lightningSpec.ImpactRadius;
                lightning.EnemyMask = lightningSpec.EnemyMask;
                lightning.ImpactColor = ResolveLightningColor(level);
                break;

            case MagneticFieldAttack magneticField when spec is MagneticFieldAttackSpec magneticSpec:
                magneticField.Radius = levelData != null ? levelData.Radius : magneticSpec.Radius;
                magneticField.MagneticEffectPrefab = magneticSpec.MagneticEffectPrefab;
                break;

            case BonusRandomMissileAttack bonusMissile when spec is BonusRandomMissileAttackSpec bonusMissileSpec:
                bonusMissile.MissilePrefab = bonusMissileSpec.MissilePrefab;
                bonusMissile.MissileSpeed = levelData != null ? levelData.MissileSpeed : bonusMissileSpec.DefaultMissileSpeed;
                bonusMissile.ExplosionDiameter = levelData != null ? levelData.Radius : bonusMissileSpec.DefaultRadius;
                bonusMissile.MinTargetRadius = bonusMissileSpec.MinTargetRadius;
                bonusMissile.MaxTargetRadius = bonusMissileSpec.MaxTargetRadius;
                bonusMissile.StrikeRadius = levelData != null ? levelData.Radius : bonusMissileSpec.DefaultRadius;
                bonusMissile.SpawnHeight = bonusMissileSpec.SpawnHeight;
                bonusMissile.MaxHeight = bonusMissileSpec.MaxHeight;
                bonusMissile.MinTravelTime = bonusMissileSpec.MinTravelTime;
                bonusMissile.UseMissileExplosionPool = bonusMissileSpec.UseMissileExplosionPool;
                break;

            case GlobalLightningBonusAttack globalLightning when spec is GlobalLightningBonusAttackSpec globalLightningSpec:
                globalLightning.GlobalRadius = levelData != null ? levelData.Radius : globalLightningSpec.GlobalRadius;
                globalLightning.ImpactColor = globalLightningSpec.ImpactColor;
                break;

            case ArtilleryFireZoneBonusAttack fireZone when spec is ArtilleryFireZoneBonusAttackSpec fireZoneSpec:
                fireZone.FireZonePrefab = fireZoneSpec.FireZonePrefab;
                fireZone.InitialPoolSize = fireZoneSpec.InitialPoolSize;
                fireZone.ZoneRadius = levelData != null ? levelData.Radius : fireZoneSpec.ZoneRadius;
                fireZone.ZoneDuration = fireZoneSpec.ZoneDuration;
                fireZone.ZoneTickInterval = levelData != null && levelData.DelayBetweenProjectiles > 0f
                    ? levelData.DelayBetweenProjectiles
                    : fireZoneSpec.ZoneTickInterval;
                fireZone.SpawnHeightOffset = fireZoneSpec.SpawnHeightOffset;
                fireZone.DamagePreset = fireZoneSpec.DamagePreset;
                break;

            case LowHealthDamageBonusAttack lowHealthBonus when spec is LowHealthDamageBonusAttackSpec lowHealthBonusSpec:
                lowHealthBonus.HealthThresholdNormalized = lowHealthBonusSpec.HealthThresholdNormalized;
                lowHealthBonus.BonusDamagePercent = levelData != null && levelData.Damage > 0f
                    ? levelData.Damage
                    : lowHealthBonusSpec.BonusDamagePercent;
                lowHealthBonus.ScaleByMissingHealth = lowHealthBonusSpec.ScaleByMissingHealth;
                lowHealthBonus.RecheckInterval = lowHealthBonusSpec.RecheckInterval;
                break;

            case RotatingBeamsBonusAttack rotatingBeams when spec is RotatingBeamsBonusAttackSpec rotatingBeamsSpec:
                rotatingBeams.BeamCount = levelData != null ? levelData.ProjectilesPerAttack : rotatingBeamsSpec.BeamCount;
                rotatingBeams.RotationSpeed = rotatingBeamsSpec.RotationSpeed;
                rotatingBeams.Range = levelData != null && levelData.Radius > 0f ? levelData.Radius : rotatingBeamsSpec.Range;
                rotatingBeams.BeamRadius = rotatingBeamsSpec.BeamRadius;
                rotatingBeams.HitHeight = rotatingBeamsSpec.HitHeight;
                rotatingBeams.MuzzleHeight = rotatingBeamsSpec.MuzzleHeight;
                rotatingBeams.BurstDuration = rotatingBeamsSpec.BurstDuration;
                rotatingBeams.RaycastInterval = levelData != null ? levelData.AttackRate : rotatingBeamsSpec.DefaultAttackRate;
                rotatingBeams.AttackRate = rotatingBeamsSpec.BurstCooldown;
                rotatingBeams.DrawDebugRays = rotatingBeamsSpec.DrawDebugRays;
                rotatingBeams.BeamPrefab = rotatingBeamsSpec.BeamPrefab;
                rotatingBeams.DamagePreset = rotatingBeamsSpec.DamagePreset;
                break;
        }
    }

    private static float GetDefaultAttackRate(AttackSpecBase spec)
    {
        return spec is IAttackStatDefaults defaults ? defaults.DefaultAttackRateValue : 1f;
    }

    private static float GetDefaultDamage(AttackSpecBase spec)
    {
        return spec is IAttackStatDefaults defaults ? defaults.DefaultDamageValue : 2f;
    }

    private static LightningAttack.LightningImpactColor ResolveLightningColor(int level)
    {
        return level switch
        {
            <= 1 => LightningAttack.LightningImpactColor.Yellow,
            2 => LightningAttack.LightningImpactColor.Blue,
            _ => LightningAttack.LightningImpactColor.Red
        };
    }
}
