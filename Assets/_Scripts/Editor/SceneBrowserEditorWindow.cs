using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class SceneBrowserEditorWindow : EditorWindow
{
    private const string ScenesRootPath = "Assets/_Core/";
    private const float RowHeight = 22f;
    private const float PingButtonWidth = 46f;
    private const float OpenButtonWidth = 46f;
    private const float SearchMinWidth = 240f;

    private readonly List<SceneEntry> _scenes = new List<SceneEntry>();
    private readonly Dictionary<string, bool> _folderExpanded = new Dictionary<string, bool>();
    private FolderNode _root;
    private string _search = string.Empty;
    private Vector2 _scroll;

    [MenuItem("Laziz/Scene Browser")]
    private static void OpenWindow()
    {
        var window = GetWindow<SceneBrowserEditorWindow>();
        window.titleContent = new GUIContent("Scene Browser");
        window.minSize = new Vector2(700f, 380f);
        window.Show();
    }

    private void OnEnable()
    {
        ReloadScenes();
    }

    private void OnGUI()
    {
        DrawToolbar();
        DrawScenesList();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Search", GUILayout.Width(44f));
        var nextSearch = GUILayout.TextField(_search, EditorStyles.toolbarTextField, GUILayout.MinWidth(SearchMinWidth));
        if (nextSearch != _search)
        {
            _search = nextSearch;
        }

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60f)))
        {
            ReloadScenes();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawScenesList()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);

        if (_root == null)
        {
            EditorGUILayout.HelpBox("No scenes found.", MessageType.Info);
            EditorGUILayout.EndScrollView();
            return;
        }

        var normalizedSearch = string.IsNullOrWhiteSpace(_search) ? string.Empty : _search.Trim().ToLowerInvariant();

        foreach (var scene in _root.Scenes)
        {
            if (!SceneMatches(scene, normalizedSearch))
            {
                continue;
            }

            var rowRect = EditorGUILayout.GetControlRect(false, RowHeight);
            DrawSceneRow(rowRect, scene, 0);
        }

        foreach (var childNode in _root.Children.OrderBy(pair => pair.Key).Select(pair => pair.Value))
        {
            DrawFolderNode(childNode, normalizedSearch, 0);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawFolderNode(FolderNode node, string normalizedSearch, int indentLevel)
    {
        if (!FolderHasMatches(node, normalizedSearch))
        {
            return;
        }

        var headerRect = EditorGUILayout.GetControlRect(false, RowHeight);
        var indentOffset = indentLevel * 16f;
        var foldoutRect = new Rect(headerRect.x + indentOffset, headerRect.y + 2f, headerRect.width - indentOffset, headerRect.height - 4f);

        var isExpanded = GetFolderExpanded(node.Key);
        var nextExpanded = EditorGUI.Foldout(foldoutRect, isExpanded, node.Name, true);
        if (nextExpanded != isExpanded)
        {
            _folderExpanded[node.Key] = nextExpanded;
        }

        if (!nextExpanded)
        {
            return;
        }

        foreach (var scene in node.Scenes)
        {
            if (!SceneMatches(scene, normalizedSearch))
            {
                continue;
            }

            var rowRect = EditorGUILayout.GetControlRect(false, RowHeight);
            DrawSceneRow(rowRect, scene, indentLevel + 1);
        }

        foreach (var childNode in node.Children.OrderBy(pair => pair.Key).Select(pair => pair.Value))
        {
            DrawFolderNode(childNode, normalizedSearch, indentLevel + 1);
        }
    }

    private void DrawSceneRow(Rect rowRect, SceneEntry scene, int indentLevel)
    {
        var eventCurrent = Event.current;
        var indentOffset = indentLevel * 16f;

        if (eventCurrent.type == EventType.Repaint)
        {
            EditorStyles.helpBox.Draw(rowRect, GUIContent.none, false, false, false, false);
        }

        var pingRect = new Rect(rowRect.x + 4f + indentOffset, rowRect.y + 2f, PingButtonWidth, rowRect.height - 4f);
        if (GUI.Button(pingRect, "Ping"))
        {
            PingScene(scene);
        }

        var openRect = new Rect(pingRect.xMax + 4f, rowRect.y + 2f, OpenButtonWidth, rowRect.height - 4f);
        if (GUI.Button(openRect, "Open"))
        {
            OpenScene(scene);
        }

        var labelRect = new Rect(openRect.xMax + 8f, rowRect.y + 2f, rowRect.width - openRect.width - pingRect.width - indentOffset - 24f, rowRect.height - 4f);
        GUI.Label(labelRect, new GUIContent(scene.Name, scene.Path), EditorStyles.label);

        if (eventCurrent.type == EventType.MouseDown &&
            eventCurrent.button == 0 &&
            eventCurrent.clickCount == 2 &&
            rowRect.Contains(eventCurrent.mousePosition))
        {
            OpenScene(scene);
            eventCurrent.Use();
        }
    }

    private static bool SceneMatches(SceneEntry scene, string normalizedSearch)
    {
        if (string.IsNullOrEmpty(normalizedSearch))
        {
            return true;
        }

        return scene.NameLower.Contains(normalizedSearch) || scene.PathLower.Contains(normalizedSearch);
    }

    private bool FolderHasMatches(FolderNode node, string normalizedSearch)
    {
        if (string.IsNullOrEmpty(normalizedSearch))
        {
            return true;
        }

        if (node.NameLower.Contains(normalizedSearch))
        {
            return true;
        }

        if (node.Scenes.Any(scene => SceneMatches(scene, normalizedSearch)))
        {
            return true;
        }

        return node.Children.Values.Any(child => FolderHasMatches(child, normalizedSearch));
    }

    private bool GetFolderExpanded(string key)
    {
        if (_folderExpanded.TryGetValue(key, out var expanded))
        {
            return expanded;
        }

        _folderExpanded[key] = true;
        return true;
    }

    private void ReloadScenes()
    {
        _scenes.Clear();

        var scenePaths = AssetDatabase
            .FindAssets("t:Scene")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => !string.IsNullOrEmpty(path))
            .Where(path => path.StartsWith(ScenesRootPath, System.StringComparison.OrdinalIgnoreCase))
            .OrderBy(path => path)
            .ToList();

        foreach (var scenePath in scenePaths)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            var name = sceneAsset != null ? sceneAsset.name : System.IO.Path.GetFileNameWithoutExtension(scenePath);
            _scenes.Add(new SceneEntry(name, scenePath, sceneAsset, name.ToLowerInvariant(), scenePath.ToLowerInvariant()));
        }

        BuildTree();
    }

    private void BuildTree()
    {
        _root = new FolderNode("_Core", "_Core", "_core");

        foreach (var scene in _scenes)
        {
            var relativePath = scene.Path;
            if (relativePath.StartsWith(ScenesRootPath, System.StringComparison.OrdinalIgnoreCase))
            {
                relativePath = relativePath.Substring(ScenesRootPath.Length);
            }

            var folderPath = System.IO.Path.GetDirectoryName(relativePath);
            if (string.IsNullOrEmpty(folderPath))
            {
                _root.Scenes.Add(scene);
                continue;
            }

            var parts = folderPath.Replace("\\", "/").Split('/');
            var current = _root;
            var currentKey = "_Core";
            foreach (var part in parts)
            {
                currentKey = $"{currentKey}/{part}";
                if (!current.Children.TryGetValue(part, out var child))
                {
                    child = new FolderNode(part, currentKey, part.ToLowerInvariant());
                    current.Children.Add(part, child);
                }

                current = child;
            }

            current.Scenes.Add(scene);
        }
    }

    private static void PingScene(SceneEntry scene)
    {
        if (scene.SceneAsset != null)
        {
            EditorGUIUtility.PingObject(scene.SceneAsset);
            Selection.activeObject = scene.SceneAsset;
            return;
        }

        var fallback = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.Path);
        if (fallback != null)
        {
            EditorGUIUtility.PingObject(fallback);
            Selection.activeObject = fallback;
        }
    }

    private static void OpenScene(SceneEntry scene)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Single);
    }

    private readonly struct SceneEntry
    {
        public string Name { get; }
        public string Path { get; }
        public SceneAsset SceneAsset { get; }
        public string NameLower { get; }
        public string PathLower { get; }

        public SceneEntry(string name, string path, SceneAsset sceneAsset, string nameLower, string pathLower)
        {
            Name = name;
            Path = path;
            SceneAsset = sceneAsset;
            NameLower = nameLower;
            PathLower = pathLower;
        }
    }

    private sealed class FolderNode
    {
        public string Name { get; }
        public string Key { get; }
        public string NameLower { get; }
        public Dictionary<string, FolderNode> Children { get; } = new Dictionary<string, FolderNode>();
        public List<SceneEntry> Scenes { get; } = new List<SceneEntry>();

        public FolderNode(string name, string key, string nameLower)
        {
            Name = name;
            Key = key;
            NameLower = nameLower;
        }
    }
}
