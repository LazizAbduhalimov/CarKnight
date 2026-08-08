using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public class AttackLevelData
{
    [MinValue(0.01f)] public float AttackRate = 1f;
    [MinValue(0f)] public float Damage = 2f;
    [MinValue(0f)] public float MissileSpeed = 15f;
    [MinValue(0f)] public float Radius = 2f;
    [MinValue(1)] public int ProjectilesPerAttack = 1;
    [MinValue(0f)] public float DelayBetweenProjectiles;
}

[CreateAssetMenu(fileName = "AttackLevelTable", menuName = "AttackSystem/Levels/Attack Level Table")]
public sealed class AttackLevelTableSO : ScriptableObject
{
    [Title("Levels")]
    [SerializeField, TableList(ShowIndexLabels = true)] 
    private List<AttackLevelData> _levels = new() { new() };

    public IReadOnlyList<AttackLevelData> Levels => _levels;
}

public static class AttackLevelTableUtility
{
    private static readonly AttackLevelData DefaultLevel = new AttackLevelData();

    public static int GetMaxLevel(AttackLevelTableSO table)
    {
        return table == null || table.Levels == null ? 1 : Mathf.Max(1, table.Levels.Count);
    }

    public static AttackLevelData GetLevelOrDefault(AttackLevelTableSO table, int level)
    {
        if (table == null || table.Levels == null || table.Levels.Count == 0)
        {
            return DefaultLevel;
        }

        var index = Mathf.Clamp(level - 1, 0, table.Levels.Count - 1);
        return table.Levels[index] ?? DefaultLevel;
    }

    public static AttackLevelData GetLevelOrNull(AttackLevelTableSO table, int level)
    {
        if (table == null || table.Levels == null || table.Levels.Count == 0)
        {
            return null;
        }

        var index = Mathf.Clamp(level - 1, 0, table.Levels.Count - 1);
        return table.Levels[index];
    }
}
