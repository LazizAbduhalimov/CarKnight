using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerUnitConfig))]
public sealed class PlayerUnitConfigEditor : Editor
{
    private SerializedProperty _xpLevelsProperty;
    private bool _showXpPreview = true;

    private static readonly Color GridColor = new Color(1f, 1f, 1f, 0.08f);
    private static readonly Color AxisColor = new Color(1f, 1f, 1f, 0.3f);
    private static readonly Color LineColor = new Color(0.35f, 0.85f, 1f, 1f);
    private static readonly Color ChartBackground = new Color(0.12f, 0.12f, 0.12f, 1f);
    private static readonly Color PlotBackground = new Color(0.16f, 0.16f, 0.16f, 1f);

    private void OnEnable()
    {
        _xpLevelsProperty = serializedObject.FindProperty("_xpLevels");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        DrawXpGrowthPreview();
        serializedObject.ApplyModifiedProperties();
    }

    private void DrawXpGrowthPreview()
    {
        EditorGUILayout.Space(8f);
        _showXpPreview = EditorGUILayout.Foldout(_showXpPreview, "XP Growth Preview", true);
        if (!_showXpPreview)
        {
            return;
        }

        if (_xpLevelsProperty == null || !_xpLevelsProperty.isArray)
        {
            EditorGUILayout.HelpBox("Could not read xpLevels.", MessageType.Warning);
            return;
        }

        var xpValues = ReadXpLevels(_xpLevelsProperty);
        EditorGUILayout.LabelField($"Max Level: {Mathf.Max(1, xpValues.Count)}", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("xpLevels[i] = XP required to reach level i + 1 from the previous level.", MessageType.None);

        if (xpValues.Count == 0)
        {
            EditorGUILayout.HelpBox("xpLevels is empty, so leveling is disabled.", MessageType.Warning);

            if (GUILayout.Button("Fill Linear Defaults (10 Levels)"))
            {
                FillLinearDefaults();
            }

            return;
        }

        DrawChart(xpValues);
        DrawTransitionPreview(xpValues);
    }

    private static List<int> ReadXpLevels(SerializedProperty xpLevelsProperty)
    {
        var result = new List<int>(xpLevelsProperty.arraySize);
        for (var i = 0; i < xpLevelsProperty.arraySize; i++)
        {
            result.Add(xpLevelsProperty.GetArrayElementAtIndex(i).intValue);
        }

        return result;
    }

    private void FillLinearDefaults()
    {
        _xpLevelsProperty.arraySize = 10;
        for (var i = 0; i < _xpLevelsProperty.arraySize; i++)
        {
            _xpLevelsProperty.GetArrayElementAtIndex(i).intValue = i * 30;
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }

    private static void DrawChart(IReadOnlyList<int> xpValues)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Curve", EditorStyles.boldLabel);

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

        EditorGUI.DrawRect(chartRect, ChartBackground);
        EditorGUI.DrawRect(plotRect, PlotBackground);

        ComputeYRange(xpValues, out var minY, out var maxY);
        DrawGridAndAxes(plotRect, minY, maxY, xpValues.Count);
        DrawSeries(plotRect, minY, maxY, xpValues);
    }

    private static void DrawTransitionPreview(IReadOnlyList<int> xpValues)
    {
        EditorGUILayout.Space(4f);
        EditorGUILayout.LabelField("Per-Level Costs", EditorStyles.boldLabel);

        var levelPreviewCount = Mathf.Min(8, xpValues.Count);
        for (var targetLevel = 1; targetLevel <= levelPreviewCount; targetLevel++)
        {
            var requiredXp = Mathf.Max(0, xpValues[targetLevel - 1]);
            EditorGUILayout.LabelField($"Reach L{targetLevel}", requiredXp.ToString());
        }

        if (xpValues.Count > levelPreviewCount)
        {
            EditorGUILayout.LabelField("...", $"{xpValues.Count - levelPreviewCount} more level entries");
        }

        if (xpValues.Count <= 1)
        {
            EditorGUILayout.HelpBox("Need at least 2 entries for level transitions from L1 to L2.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space(2f);
        EditorGUILayout.LabelField("Runtime Transition Costs", EditorStyles.boldLabel);

        var previewCount = Mathf.Min(8, xpValues.Count - 1);
        for (var currentLevel = 1; currentLevel <= previewCount; currentLevel++)
        {
            var requiredXp = Mathf.Max(1, xpValues[currentLevel]);
            EditorGUILayout.LabelField($"L{currentLevel} -> L{currentLevel + 1}", requiredXp.ToString());
        }

        if (xpValues.Count - 1 > previewCount)
        {
            EditorGUILayout.LabelField("...", $"{xpValues.Count - 1 - previewCount} more transitions");
        }
    }

    private static void ComputeYRange(IReadOnlyList<int> xpValues, out float minY, out float maxY)
    {
        minY = float.MaxValue;
        maxY = float.MinValue;

        for (var i = 0; i < xpValues.Count; i++)
        {
            var value = xpValues[i];
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

        const int horizontalTicks = 4;
        for (var i = 0; i <= horizontalTicks; i++)
        {
            var t = i / (float)horizontalTicks;
            var y = Mathf.Lerp(plotRect.yMax, plotRect.yMin, t);
            Handles.color = GridColor;
            Handles.DrawLine(new Vector3(plotRect.xMin, y), new Vector3(plotRect.xMax, y));

            var value = Mathf.Lerp(minY, maxY, t);
            var labelRect = new Rect(plotRect.xMin - 44f, y - 9f, 40f, 18f);
            EditorGUI.LabelField(labelRect, value.ToString("0"), EditorStyles.miniLabel);
        }

        var step = levelCount > 16 ? Mathf.CeilToInt(levelCount / 8f) : 1;
        for (var i = 0; i < levelCount; i += step)
        {
            var x = GetX(plotRect, i, levelCount);
            Handles.color = GridColor;
            Handles.DrawLine(new Vector3(x, plotRect.yMin), new Vector3(x, plotRect.yMax));

            var labelRect = new Rect(x - 14f, plotRect.yMax + 4f, 28f, 18f);
            EditorGUI.LabelField(labelRect, $"L{i + 1}", EditorStyles.miniLabel);
        }

        Handles.color = AxisColor;
        Handles.DrawLine(new Vector3(plotRect.xMin, plotRect.yMin), new Vector3(plotRect.xMin, plotRect.yMax));
        Handles.DrawLine(new Vector3(plotRect.xMin, plotRect.yMax), new Vector3(plotRect.xMax, plotRect.yMax));
        Handles.EndGUI();
    }

    private static void DrawSeries(Rect plotRect, float minY, float maxY, IReadOnlyList<int> xpValues)
    {
        Handles.BeginGUI();
        Handles.color = LineColor;

        var points = new Vector3[xpValues.Count];
        for (var i = 0; i < xpValues.Count; i++)
        {
            var x = GetX(plotRect, i, xpValues.Count);
            var yT = Mathf.InverseLerp(minY, maxY, xpValues[i]);
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
