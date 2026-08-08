using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class SceneQuickLaunchToolbar
{
    private const string ScenesRootPath = "Assets/_Core/";
    private const float PopupWidth = 146f;

    private static readonly Type ToolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");

    private static ScriptableObject _currentToolbar;
    private static VisualElement _container;

    private static readonly List<SceneEntry> Scenes = new();
    private static int _selectedSceneIndex;
    private static double _lastSceneReloadTime;

    static SceneQuickLaunchToolbar()
    {
        ReloadScenes();
        EditorApplication.projectChanged += OnProjectChanged;
        EditorApplication.update += TryInstallIntoToolbar;
    }

    private static void OnProjectChanged()
    {
        ReloadScenes();
    }

    private static void TryInstallIntoToolbar()
    {
        if (ToolbarType == null)
        {
            return;
        }

        var toolbar = Resources.FindObjectsOfTypeAll(ToolbarType).FirstOrDefault() as ScriptableObject;
        if (toolbar == null)
        {
            return;
        }

        if (ReferenceEquals(_currentToolbar, toolbar) && _container != null && _container.parent != null)
        {
            return;
        }

        _currentToolbar = toolbar;
        _container = null;

        var rootField = ToolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
        var root = rootField?.GetValue(toolbar) as VisualElement;
        if (root == null)
        {
            return;
        }

        // Keep quick launch on the right side of Unity toolbar.
        var zone = root.Q("ToolbarZoneRightAlign")
                   ?? root.Q("ToolbarZonePlayModes")
                   ?? root.Q("ToolbarZoneLeftAlign");
        if (zone == null)
        {
            return;
        }

        _container = new IMGUIContainer(DrawToolbarGUI)
        {
            style =
            {
                flexDirection = FlexDirection.Row,
                marginLeft = 6f,
                marginRight = 6f
            }
        };

        zone.Add(_container);
    }

    private static void DrawToolbarGUI()
    {
        // Keep scene list fresh if domain was reloaded without projectChanged.
        var now = EditorApplication.timeSinceStartup;
        if (now - _lastSceneReloadTime > 5d)
        {
            ReloadScenes();
        }

        if (Scenes.Count == 0)
        {
            GUILayout.Label("No _Core scenes", EditorStyles.miniLabel, GUILayout.Width(PopupWidth));
            return;
        }

        EditorGUILayout.BeginHorizontal();

        var names = Scenes.Select(s => s.Name).ToArray();
        _selectedSceneIndex = Mathf.Clamp(_selectedSceneIndex, 0, names.Length - 1);
        var nextIndex = EditorGUILayout.Popup(_selectedSceneIndex, names, EditorStyles.toolbarPopup, GUILayout.Width(PopupWidth));
        if (nextIndex != _selectedSceneIndex)
        {
            _selectedSceneIndex = nextIndex;
            OpenSelectedScene();
        }

        EditorGUILayout.EndHorizontal();
    }

    private static void OpenSelectedScene()
    {
        if (_selectedSceneIndex < 0 || _selectedSceneIndex >= Scenes.Count)
        {
            return;
        }

        var scene = Scenes[_selectedSceneIndex];
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Single);
    }

    private static void ReloadScenes()
    {
        Scenes.Clear();

        var scenePaths = AssetDatabase
            .FindAssets("t:Scene", new[] { ScenesRootPath.TrimEnd('/') })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrEmpty(path))
            .Where(path => path.StartsWith(ScenesRootPath, StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path)
            .ToList();

        for (var i = 0; i < scenePaths.Count; i++)
        {
            var scenePath = scenePaths[i];
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            var displayName = sceneAsset != null ? sceneAsset.name : System.IO.Path.GetFileNameWithoutExtension(scenePath);
            Scenes.Add(new SceneEntry(displayName, scenePath));
        }

        _selectedSceneIndex = Mathf.Clamp(_selectedSceneIndex, 0, Mathf.Max(0, Scenes.Count - 1));
        _lastSceneReloadTime = EditorApplication.timeSinceStartup;
    }

    private readonly struct SceneEntry
    {
        public string Name { get; }
        public string Path { get; }

        public SceneEntry(string name, string path)
        {
            Name = name;
            Path = path;
        }
    }
}
