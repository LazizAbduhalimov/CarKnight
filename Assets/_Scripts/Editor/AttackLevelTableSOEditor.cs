using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AttackLevelTableSO))]
public sealed class AttackLevelTableSOEditor : Editor
{
    private readonly struct MetricDef
    {
        public MetricDef(string label, Color color, Func<AttackLevelData, float> getter)
        {
            Label = label;
            Color = color;
            Getter = getter;
        }

        public string Label { get; }
        public Color Color { get; }
        public Func<AttackLevelData, float> Getter { get; }
    }

    private static readonly MetricDef[] Metrics =
    {
        new("Attack Rate", new Color(0.2f, 0.8f, 1f), level => level?.AttackRate ?? 0f),
        new("Damage", new Color(1f, 0.35f, 0.35f), level => level?.Damage ?? 0f),
        new("Missile Speed", new Color(1f, 0.75f, 0.2f), level => level?.MissileSpeed ?? 0f),
        new("Radius", new Color(0.35f, 1f, 0.45f), level => level?.Radius ?? 0f),
        new("Projectiles", new Color(0.95f, 0.4f, 1f), level => level?.ProjectilesPerAttack ?? 0f),
        new("Delay", new Color(0.8f, 0.8f, 0.8f), level => level?.DelayBetweenProjectiles ?? 0f),
    };

    private static readonly string[] MetricNames =
    {
        "Attack Rate",
        "Damage",
        "Missile Speed",
        "Radius",
        "Projectiles",
        "Delay",
    };

    private bool _showChart = true;
    private int _selectedMetric;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(6f);
        _showChart = EditorGUILayout.Foldout(_showChart, "Level Curve", true);
        if (!_showChart)
        {
            return;
        }

        _selectedMetric = EditorGUILayout.Popup("Metric", Mathf.Clamp(_selectedMetric, 0, Metrics.Length - 1), MetricNames);

        var table = (AttackLevelTableSO)target;
        DrawChart(table, Metrics[_selectedMetric]);
    }

    private static void DrawChart(AttackLevelTableSO table, MetricDef metric)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField(metric.Label, EditorStyles.boldLabel);

        if (table == null || table.Levels == null || table.Levels.Count == 0)
        {
            EditorGUILayout.HelpBox("No levels to draw.", MessageType.Info);
            return;
        }

        var chartRect = GUILayoutUtility.GetRect(10f, 210f, GUILayout.ExpandWidth(true));
        if (chartRect.width < 120f)
        {
            return;
        }

        var leftPad = 46f;
        var rightPad = 14f;
        var topPad = 16f;
        var bottomPad = 26f;

        var plotRect = new Rect(
            chartRect.x + leftPad,
            chartRect.y + topPad,
            chartRect.width - leftPad - rightPad,
            chartRect.height - topPad - bottomPad);

        EditorGUI.DrawRect(chartRect, new Color(0.12f, 0.12f, 0.12f, 1f));
        EditorGUI.DrawRect(plotRect, new Color(0.16f, 0.16f, 0.16f, 1f));

        ComputeYRange(table.Levels, metric, out var minY, out var maxY);
        DrawGridAndAxes(plotRect, minY, maxY, table.Levels.Count);
        DrawSeries(table.Levels, plotRect, minY, maxY, metric);
    }

    private static void ComputeYRange(IReadOnlyList<AttackLevelData> levels, MetricDef metric, out float minY, out float maxY)
    {
        minY = float.MaxValue;
        maxY = float.MinValue;

        foreach (var level in levels)
        {
            var value = metric.Getter(level);
            minY = Mathf.Min(minY, value);
            maxY = Mathf.Max(maxY, value);
        }

        if (minY == float.MaxValue || maxY == float.MinValue)
        {
            minY = 0f;
            maxY = 1f;
            return;
        }

        if (Mathf.Approximately(minY, maxY))
        {
            var pad = Mathf.Max(1f, Mathf.Abs(maxY) * 0.25f);
            minY -= pad;
            maxY += pad;
        }
        else
        {
            var pad = (maxY - minY) * 0.1f;
            minY -= pad;
            maxY += pad;
        }
    }

    private static void DrawGridAndAxes(Rect plotRect, float minY, float maxY, int levelCount)
    {
        Handles.BeginGUI();

        var gridColor = new Color(1f, 1f, 1f, 0.08f);
        var axisColor = new Color(1f, 1f, 1f, 0.3f);

        const int horizontalTicks = 4;
        for (var i = 0; i <= horizontalTicks; i++)
        {
            var t = i / (float)horizontalTicks;
            var y = Mathf.Lerp(plotRect.yMax, plotRect.yMin, t);
            Handles.color = gridColor;
            Handles.DrawLine(new Vector3(plotRect.xMin, y), new Vector3(plotRect.xMax, y));

            var value = Mathf.Lerp(minY, maxY, t);
            var labelRect = new Rect(plotRect.xMin - 44f, y - 9f, 40f, 18f);
            EditorGUI.LabelField(labelRect, value.ToString("0.##"), EditorStyles.miniLabel);
        }

        var step = levelCount > 16 ? Mathf.CeilToInt(levelCount / 8f) : 1;
        for (var i = 0; i < levelCount; i += step)
        {
            var x = GetX(plotRect, i, levelCount);
            Handles.color = gridColor;
            Handles.DrawLine(new Vector3(x, plotRect.yMin), new Vector3(x, plotRect.yMax));

            var labelRect = new Rect(x - 14f, plotRect.yMax + 4f, 28f, 18f);
            EditorGUI.LabelField(labelRect, $"L{i + 1}", EditorStyles.miniLabel);
        }

        Handles.color = axisColor;
        Handles.DrawLine(new Vector3(plotRect.xMin, plotRect.yMin), new Vector3(plotRect.xMin, plotRect.yMax));
        Handles.DrawLine(new Vector3(plotRect.xMin, plotRect.yMax), new Vector3(plotRect.xMax, plotRect.yMax));
        Handles.EndGUI();
    }

    private static void DrawSeries(IReadOnlyList<AttackLevelData> levels, Rect plotRect, float minY, float maxY, MetricDef metric)
    {
        Handles.BeginGUI();
        Handles.color = metric.Color;

        var points = new Vector3[levels.Count];
        for (var i = 0; i < levels.Count; i++)
        {
            var x = GetX(plotRect, i, levels.Count);
            var value = metric.Getter(levels[i]);
            var yT = Mathf.InverseLerp(minY, maxY, value);
            var y = Mathf.Lerp(plotRect.yMax, plotRect.yMin, yT);
            points[i] = new Vector3(x, y, 0f);
        }

        if (points.Length == 1)
        {
            Handles.DrawSolidDisc(points[0], Vector3.forward, 3f);
        }
        else
        {
            Handles.DrawAAPolyLine(2.3f, points);
            for (var i = 0; i < points.Length; i++)
            {
                Handles.DrawSolidDisc(points[i], Vector3.forward, 2f);
            }
        }

        Handles.EndGUI();
    }

    private static float GetX(Rect plotRect, int index, int count)
    {
        if (count <= 1)
        {
            return (plotRect.xMin + plotRect.xMax) * 0.5f;
        }

        var t = index / (float)(count - 1);
        return Mathf.Lerp(plotRect.xMin, plotRect.xMax, t);
    }
}
