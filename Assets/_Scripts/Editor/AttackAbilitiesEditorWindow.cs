using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

public sealed class AttackAbilitiesEditorWindow : OdinMenuEditorWindow
{
    private static readonly string[] PrefabSearchFolders = { "Assets/_Core" };

    private const string DefaultCreatePath = "Assets/_Core/_Data/Attacks";
    private const string RenameFieldControlName = "AttackRenameField";
    private const float DefaultMenuWidth = 250f;
    private const float DefaultMenuItemHeight = 30f;
    private const float DefaultMenuIconSize = 22f;

    private AttackSO _renameTarget;
    private string _renameBuffer = string.Empty;
    private bool _renameMode;
    private bool _renameFieldWasFocused;
    private AttackSO _referencesTarget;
    private bool _referencesFoldout = true;
    private Vector2 _referencesScroll;
    private List<string> _referencePaths = new List<string>();
    private Dictionary<AttackSO, List<string>> _referencesByAttack = new Dictionary<AttackSO, List<string>>();
    private string[] _prefabGuidsForIndex = Array.Empty<string>();
    private int _indexingCursor;
    private bool _isIndexingReferences;
    private bool _indexingHasResult;
    private double _lastRepaintTime;

    private const int IndexBatchSize = 24;
    private const double RepaintIntervalSec = 0.15d;

    [MenuItem("Laziz/Attack Abilities Config")]
    private static void OpenWindow()
    {
        var window = GetWindow<AttackAbilitiesEditorWindow>();
        window.titleContent = new GUIContent("Attack Abilities");
        window.minSize = new Vector2(1000f, 600f);
        window.Show();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        MenuWidth = Mathf.Max(MenuWidth, DefaultMenuWidth);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopReferencesIndexing();
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true)
        {
            Config =
            {
                DrawSearchToolbar = true
            }
        };

        tree.DefaultMenuStyle.IconSize = DefaultMenuIconSize;
        tree.DefaultMenuStyle.Height = (int)DefaultMenuItemHeight;

        var allAttacks = LoadAllAttackSOs();
        var allAttackTypes = GetAllAttackTypes().ToList();

        foreach (var attackType in allAttackTypes)
        {
            tree.Add($"Attack Types/{attackType.Name}", new AttackTypeGroupMarker(attackType));

            var instances = allAttacks
                .Where(attackSo => GetAttackTypeName(attackSo) == attackType.Name)
                .OrderBy(attackSo => attackSo.name);

            foreach (var attackSo in instances)
            {
                var attackPath = $"Attack Types/{attackType.Name}/{attackSo.name}";
                tree.Add(attackPath, attackSo, attackSo.AbilityIcon);
            }
        }

        foreach (var unassigned in allAttacks.Where(a => a.Spec == null).OrderBy(a => a.name))
        {
            var attackPath = $"Attack Types/Unassigned/{unassigned.name}";
            tree.Add(attackPath, unassigned, unassigned.AbilityIcon);
        }

        return tree;
    }

    protected override void OnBeginDrawEditors()
    {
        TryHandleDoubleClickRename();

        SirenixEditorGUI.BeginHorizontalToolbar();
        var selectedAttackType = GetSelectedAttackType();
        var selectedAttackSo = GetSelectedAttackSO();
        var createLabel = selectedAttackType == null
            ? "Create Attack [Base]"
            : $"Create Attack [{selectedAttackType.Name}]";

        if (SirenixEditorGUI.ToolbarButton("Refresh"))
        {
            ForceMenuTreeRebuild();
            StartReferencesIndexing();
        }

        if (SirenixEditorGUI.ToolbarButton(createLabel))
        {
            var createdAsset = CreateNewAttackSOAsset(selectedAttackType);
            SelectCreatedAssetInMenu(createdAsset);
        }

        if (selectedAttackSo != null && SirenixEditorGUI.ToolbarButton("Copy"))
        {
            var clone = CloneAttackSoAsset(selectedAttackSo);
            SelectCreatedAssetInMenu(clone);
        }

        DrawRenameControls(selectedAttackSo);

        GUILayout.FlexibleSpace();

        if (selectedAttackSo != null)
        {
            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.3f, 1f);
            if (SirenixEditorGUI.ToolbarButton("Delete"))
            {
                TryDeleteSelectedAttack(selectedAttackSo);
            }
            GUI.backgroundColor = previousColor;
        }

        SirenixEditorGUI.EndHorizontalToolbar();
    }

    protected override void OnEndDrawEditors()
    {
        var selectedAttackSo = GetSelectedAttackSO();
        if (selectedAttackSo == null)
        {
            _referencesTarget = null;
            _referencePaths.Clear();
            return;
        }

        if (!ReferenceEquals(_referencesTarget, selectedAttackSo))
        {
            _referencesTarget = selectedAttackSo;
            UpdateSelectedReferencePaths(selectedAttackSo);
        }

        DrawReferencesFoldout(selectedAttackSo);
    }

    private static IEnumerable<Type> GetAllAttackTypes()
    {
        return TypeCache
            .GetTypesDerivedFrom<AttackSpecBase>()
            .Where(type =>
                !type.IsAbstract &&
                !type.IsGenericType &&
                !type.ContainsGenericParameters &&
                !type.Name.EndsWith("Base", StringComparison.Ordinal) &&
                typeof(AttackSpecBase).IsAssignableFrom(type))
            .OrderBy(type => type.Name);
    }

    private static List<AttackSO> LoadAllAttackSOs()
    {
        return AssetDatabase
            .FindAssets("t:AttackSO")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(path => AssetDatabase.LoadAssetAtPath<AttackSO>(path))
            .Where(asset => asset != null)
            .OrderBy(asset => asset.name)
            .ToList();
    }

    private static string GetAttackTypeName(AttackSO attackSo)
    {
        if (attackSo == null || attackSo.Spec == null)
        {
            return "Unassigned";
        }

        var type = attackSo.Spec.GetType();
        return string.IsNullOrWhiteSpace(type.Name) ? "Unknown Type" : type.Name;
    }

    private Type GetSelectedAttackType()
    {
        var selectedItem = MenuTree?.Selection?.FirstOrDefault();
        if (selectedItem == null)
        {
            return null;
        }

        if (selectedItem.Value is AttackSO attackSo && attackSo.Spec != null)
        {
            return attackSo.Spec.GetType();
        }

        if (selectedItem.Value is AttackTypeGroupMarker marker)
        {
            return marker.AttackType;
        }

        return null;
    }

    private AttackSO GetSelectedAttackSO()
    {
        var selectedItem = MenuTree?.Selection?.FirstOrDefault();
        return selectedItem?.Value as AttackSO;
    }

    private void DrawRenameControls(AttackSO selectedAttackSo)
    {
        if (selectedAttackSo == null)
        {
            _renameTarget = null;
            _renameBuffer = string.Empty;
            _renameMode = false;
            _renameFieldWasFocused = false;
            return;
        }

        if (!ReferenceEquals(_renameTarget, selectedAttackSo))
        {
            _renameTarget = selectedAttackSo;
            _renameBuffer = selectedAttackSo.name;
            _renameMode = false;
            _renameFieldWasFocused = false;
        }

        GUILayout.Space(10f);
        GUILayout.Label("Name", GUILayout.Width(38f));
        GUI.SetNextControlName(RenameFieldControlName);
        _renameBuffer = GUILayout.TextField(_renameBuffer, GUILayout.MinWidth(220f), GUILayout.MaxWidth(360f));

        if (_renameMode)
        {
            EditorGUI.FocusTextInControl(RenameFieldControlName);
            _renameMode = false;
        }

        var isRenameFieldFocused = GUI.GetNameOfFocusedControl() == RenameFieldControlName;
        if (_renameFieldWasFocused && !isRenameFieldFocused)
        {
            RenameSelectedAttack(selectedAttackSo, _renameBuffer);
        }

        _renameFieldWasFocused = isRenameFieldFocused;
    }

    private void TryHandleDoubleClickRename()
    {
        var evt = Event.current;
        if (evt == null || evt.type != EventType.MouseDown || evt.button != 0 || evt.clickCount < 2)
        {
            return;
        }

        var selectedItem = MenuTree?.Selection?.FirstOrDefault();
        if (selectedItem == null || selectedItem.Value is not AttackSO)
        {
            return;
        }

        if (!selectedItem.Rect.Contains(evt.mousePosition))
        {
            return;
        }

        _renameMode = true;
        evt.Use();
    }

    private void RenameSelectedAttack(AttackSO attackSo, string desiredName)
    {
        if (attackSo == null)
        {
            return;
        }

        var newName = (desiredName ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(newName) || newName == attackSo.name)
        {
            return;
        }

        var oldName = attackSo.name;
        var assetPath = AssetDatabase.GetAssetPath(attackSo);
        var spec = attackSo.Spec;
        var levelTable = attackSo.LevelTable;

        AssetDatabase.StartAssetEditing();
        try
        {
            var error = AssetDatabase.RenameAsset(assetPath, newName);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Failed to rename AttackSO '{oldName}' to '{newName}': {error}");
                return;
            }

            TryRenameRelatedAsset(spec, oldName, newName, "_Spec");
            TryRenameRelatedAsset(levelTable, oldName, newName, "_LevelTable");
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();
        SelectCreatedAssetInMenu(attackSo);
    }

    private static void TryRenameRelatedAsset(UnityEngine.Object relatedAsset, string oldAttackName, string newAttackName, string fallbackSuffix)
    {
        if (relatedAsset == null)
        {
            return;
        }

        var relatedPath = AssetDatabase.GetAssetPath(relatedAsset);
        if (string.IsNullOrEmpty(relatedPath))
        {
            return;
        }

        var targetName = BuildRenamedRelatedAssetName(relatedAsset.name, oldAttackName, newAttackName, fallbackSuffix);
        if (string.IsNullOrWhiteSpace(targetName) || string.Equals(targetName, relatedAsset.name, StringComparison.Ordinal))
        {
            return;
        }

        var renameError = AssetDatabase.RenameAsset(relatedPath, targetName);
        if (!string.IsNullOrEmpty(renameError))
        {
            Debug.LogWarning($"Failed to rename related asset '{relatedAsset.name}' to '{targetName}': {renameError}");
        }
    }

    private static string BuildRenamedRelatedAssetName(string currentName, string oldAttackName, string newAttackName, string fallbackSuffix)
    {
        if (string.IsNullOrWhiteSpace(newAttackName))
        {
            return currentName;
        }

        if (!string.IsNullOrWhiteSpace(currentName) && !string.IsNullOrWhiteSpace(oldAttackName))
        {
            if (currentName.StartsWith(oldAttackName, StringComparison.OrdinalIgnoreCase))
            {
                return $"{newAttackName}{currentName.Substring(oldAttackName.Length)}";
            }

            var index = currentName.IndexOf(oldAttackName, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                return $"{currentName.Substring(0, index)}{newAttackName}{currentName.Substring(index + oldAttackName.Length)}";
            }
        }

        return $"{newAttackName}{fallbackSuffix}";
    }

    private AttackSO CloneAttackSoAsset(AttackSO source)
    {
        if (source == null)
        {
            return null;
        }

        var sourcePath = AssetDatabase.GetAssetPath(source);
        var targetFolder = string.IsNullOrEmpty(sourcePath)
            ? DefaultCreatePath
            : System.IO.Path.GetDirectoryName(sourcePath)?.Replace("\\", "/");
        if (string.IsNullOrEmpty(targetFolder))
        {
            targetFolder = DefaultCreatePath;
        }

        EnsureFolderExists(targetFolder);

        var clone = Instantiate(source);
        var clonePath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/{source.name}_Copy.asset");
        AssetDatabase.CreateAsset(clone, clonePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = clone;
        EditorGUIUtility.PingObject(clone);
        return clone;
    }

    private void TryDeleteSelectedAttack(AttackSO selectedAttackSo)
    {
        if (selectedAttackSo == null)
        {
            return;
        }

        var attackPath = AssetDatabase.GetAssetPath(selectedAttackSo);
        if (string.IsNullOrEmpty(attackPath))
        {
            return;
        }

        var spec = selectedAttackSo.Spec;
        var levelTable = selectedAttackSo.LevelTable;
        CountDependencyReferencesInAttackAssets(selectedAttackSo, spec, levelTable, out var specRefs, out var levelRefs);

        var dialogMessage =
            $"Delete '{selectedAttackSo.name}'?\n\n" +
            $"Spec: {(spec == null ? "None" : spec.name)} (refs in AttackSO: {specRefs})\n" +
            $"Level Table: {(levelTable == null ? "None" : levelTable.name)} (refs in AttackSO: {levelRefs})\n\n" +
            "Choose delete mode.";

        var action = EditorUtility.DisplayDialogComplex(
            "Delete AttackSO",
            dialogMessage,
            "Delete Attack + Dependencies",
            "Cancel",
            "Delete Attack Only");

        if (action == 1)
        {
            return;
        }

        var deleteDependencies = action == 0;
        var specPath = spec == null ? string.Empty : AssetDatabase.GetAssetPath(spec);
        var levelTablePath = levelTable == null ? string.Empty : AssetDatabase.GetAssetPath(levelTable);

        AssetDatabase.StartAssetEditing();
        try
        {
            if (!AssetDatabase.DeleteAsset(attackPath))
            {
                Debug.LogError($"Failed to delete AttackSO at path '{attackPath}'.");
                return;
            }

            if (deleteDependencies)
            {
                if (!string.IsNullOrEmpty(specPath) && specRefs == 0)
                {
                    AssetDatabase.DeleteAsset(specPath);
                }
                else if (!string.IsNullOrEmpty(specPath) && specRefs > 0)
                {
                    Debug.LogWarning($"Spec '{spec.name}' was not deleted because it has {specRefs} AttackSO reference(s).");
                }

                if (!string.IsNullOrEmpty(levelTablePath) && levelRefs == 0)
                {
                    AssetDatabase.DeleteAsset(levelTablePath);
                }
                else if (!string.IsNullOrEmpty(levelTablePath) && levelRefs > 0)
                {
                    Debug.LogWarning($"Level table '{levelTable.name}' was not deleted because it has {levelRefs} AttackSO reference(s).");
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        AssetDatabase.SaveAssets();

        _renameTarget = null;
        _renameBuffer = string.Empty;
        _renameMode = false;
        ForceMenuTreeRebuild();
    }

    private AttackSO CreateNewAttackSOAsset(Type selectedAttackType)
    {
        var typeFolderName = selectedAttackType == null ? "Base" : SanitizeFolderName(selectedAttackType.Name);
        var targetFolder = $"{DefaultCreatePath}/{typeFolderName}";
        EnsureFolderExists(targetFolder);

        var newAsset = CreateInstance<AttackSO>();
        var serializedObject = new SerializedObject(newAsset);
        serializedObject.FindProperty("_attackId").stringValue = Guid.NewGuid().ToString("N");
        var levelTable = CreateInstance<AttackLevelTableSO>();
        var levelTablePath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/NewAttackLevelTable.asset");
        AssetDatabase.CreateAsset(levelTable, levelTablePath);
        serializedObject.FindProperty("_levelTable").objectReferenceValue = levelTable;
        if (selectedAttackType != null)
        {
            var specAsset = CreateInstance(selectedAttackType) as AttackSpecBase;
            if (specAsset != null)
            {
                var specPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/NewAttackType_Spec.asset");
                AssetDatabase.CreateAsset(specAsset, specPath);
                serializedObject.FindProperty("_spec").objectReferenceValue = specAsset;
            }
        }
        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        var assetPath = AssetDatabase.GenerateUniqueAssetPath($"{targetFolder}/NewAttackType.asset");
        AssetDatabase.CreateAsset(newAsset, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = newAsset;
        EditorGUIUtility.PingObject(newAsset);
        return newAsset;
    }

    private void SelectCreatedAssetInMenu(AttackSO createdAsset)
    {
        if (createdAsset == null)
        {
            return;
        }

        EditorApplication.delayCall += () =>
        {
            if (this == null)
            {
                return;
            }

            ForceMenuTreeRebuild();
            var createdItem = MenuTree?
                .EnumerateTree(item => ReferenceEquals(item.Value, createdAsset), includeRootNode: false)
                .FirstOrDefault();
            createdItem?.Select();
        };
    }

    private static void CountDependencyReferencesInAttackAssets(
        AttackSO excludedAttack,
        AttackSpecBase targetSpec,
        AttackLevelTableSO targetLevelTable,
        out int specRefs,
        out int levelRefs)
    {
        specRefs = 0;
        levelRefs = 0;

        if (targetSpec == null && targetLevelTable == null)
        {
            return;
        }

        var attackGuids = AssetDatabase.FindAssets("t:AttackSO");
        for (var i = 0; i < attackGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(attackGuids[i]);
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            var attackAsset = AssetDatabase.LoadAssetAtPath<AttackSO>(path);
            if (attackAsset == null || ReferenceEquals(attackAsset, excludedAttack))
            {
                continue;
            }

            if (targetSpec != null && ReferenceEquals(attackAsset.Spec, targetSpec))
            {
                specRefs++;
            }

            if (targetLevelTable != null && ReferenceEquals(attackAsset.LevelTable, targetLevelTable))
            {
                levelRefs++;
            }
        }
    }

    private static string SanitizeFolderName(string rawName)
    {
        if (string.IsNullOrWhiteSpace(rawName))
        {
            return "Base";
        }

        var invalidChars = Path.GetInvalidFileNameChars();
        var chars = rawName.Trim().ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            if (invalidChars.Contains(chars[i]))
            {
                chars[i] = '_';
            }
        }

        var safeName = new string(chars).Replace(' ', '_');
        return string.IsNullOrWhiteSpace(safeName) ? "Base" : safeName;
    }

    private static void EnsureFolderExists(string assetFolderPath)
    {
        if (AssetDatabase.IsValidFolder(assetFolderPath))
        {
            return;
        }

        var parts = assetFolderPath.Split('/');
        if (parts.Length == 0 || parts[0] != "Assets")
        {
            throw new ArgumentException($"Invalid Unity asset folder path: {assetFolderPath}");
        }

        var currentPath = parts[0];
        for (var i = 1; i < parts.Length; i++)
        {
            var nextPath = $"{currentPath}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, parts[i]);
            }

            currentPath = nextPath;
        }
    }

    private void DrawReferencesFoldout(AttackSO selectedAttackSo)
    {
        GUILayout.Space(8f);
        _referencesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(
            _referencesFoldout,
            $"Used In Attacker Prefabs ({_referencePaths.Count})");

        if (_referencesFoldout)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh References", GUILayout.Width(250f)))
            {
                StartReferencesIndexing();
                UpdateSelectedReferencePaths(selectedAttackSo);
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (_isIndexingReferences)
            {
                var total = _prefabGuidsForIndex.Length;
                var progress = total <= 0 ? 1f : (float)_indexingCursor / total;
                EditorGUILayout.HelpBox(
                    $"Building references index in background... {_indexingCursor}/{total} ({progress:P0})",
                    MessageType.Info);
            }
            else if (!_indexingHasResult)
            {
                EditorGUILayout.HelpBox("References index is not ready yet.", MessageType.Info);
            }

            if (_referencePaths.Count == 0)
            {
                EditorGUILayout.HelpBox("No references found in prefabs with Attacker.", MessageType.Info);
            }
            else
            {
                var maxHeight = Mathf.Min(280f, 26f * _referencePaths.Count + 8f);
                _referencesScroll = EditorGUILayout.BeginScrollView(_referencesScroll, GUILayout.MaxHeight(maxHeight));

                foreach (var path in _referencePaths)
                {
                    DrawReferenceRow(path);
                }

                EditorGUILayout.EndScrollView();
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private static void DrawReferenceRow(string assetPath)
    {
        var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField(obj, typeof(UnityEngine.Object), false);
        EditorGUILayout.SelectableLabel(assetPath, GUILayout.Height(EditorGUIUtility.singleLineHeight));
        if (GUILayout.Button("Ping", GUILayout.Width(48f)))
        {
            if (obj != null)
            {
                EditorGUIUtility.PingObject(obj);
                Selection.activeObject = obj;
            }
        }

        if (GUILayout.Button("Open", GUILayout.Width(48f)))
        {
            if (obj != null)
            {
                AssetDatabase.OpenAsset(obj);
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void StartReferencesIndexing()
    {
        StopReferencesIndexing();

        var allAttacks = LoadAllAttackSOs();
        _referencesByAttack = new Dictionary<AttackSO, List<string>>(allAttacks.Count);
        for (var i = 0; i < allAttacks.Count; i++)
        {
            var attack = allAttacks[i];
            if (attack != null && !_referencesByAttack.ContainsKey(attack))
            {
                _referencesByAttack.Add(attack, new List<string>());
            }
        }

        _prefabGuidsForIndex = AssetDatabase.FindAssets("t:Prefab", PrefabSearchFolders);
        _indexingCursor = 0;
        _isIndexingReferences = true;
        _indexingHasResult = false;
        EditorApplication.update += TickReferencesIndexing;
        Repaint();
    }

    private void StopReferencesIndexing()
    {
        if (!_isIndexingReferences)
        {
            return;
        }

        _isIndexingReferences = false;
        EditorApplication.update -= TickReferencesIndexing;
    }

    private void TickReferencesIndexing()
    {
        if (!_isIndexingReferences)
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var processed = 0;

        while (_indexingCursor < _prefabGuidsForIndex.Length && processed < IndexBatchSize)
        {
            ProcessPrefabGuid(_prefabGuidsForIndex[_indexingCursor]);
            _indexingCursor++;
            processed++;

            if (stopwatch.ElapsedMilliseconds >= 8)
            {
                break;
            }
        }

        if (_referencesTarget != null)
        {
            UpdateSelectedReferencePaths(_referencesTarget);
        }

        var now = EditorApplication.timeSinceStartup;
        if (now - _lastRepaintTime >= RepaintIntervalSec)
        {
            _lastRepaintTime = now;
            Repaint();
        }

        if (_indexingCursor < _prefabGuidsForIndex.Length)
        {
            return;
        }

        FinalizeReferencesIndex();
        StopReferencesIndexing();
        Repaint();
    }

    private void ProcessPrefabGuid(string prefabGuid)
    {
        var prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
        if (string.IsNullOrEmpty(prefabPath))
        {
            return;
        }

        var prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefabRoot == null)
        {
            return;
        }

        var attackers = prefabRoot.GetComponentsInChildren<Attacker>(true);
        if (attackers == null || attackers.Length == 0)
        {
            return;
        }

        var referencedInPrefab = new HashSet<AttackSO>();
        foreach (var attacker in attackers)
        {
            var list = attacker != null ? attacker.AttackSOList : null;
            if (list == null || list.Count == 0)
            {
                continue;
            }

            for (var j = 0; j < list.Count; j++)
            {
                var attackSo = list[j];
                if (attackSo != null)
                {
                    referencedInPrefab.Add(attackSo);
                }
            }
        }

        foreach (var attackSo in referencedInPrefab)
        {
            if (!_referencesByAttack.TryGetValue(attackSo, out var paths))
            {
                paths = new List<string>();
                _referencesByAttack[attackSo] = paths;
            }

            paths.Add(prefabPath);
        }
    }

    private void FinalizeReferencesIndex()
    {
        foreach (var pair in _referencesByAttack)
        {
            pair.Value.Sort(StringComparer.OrdinalIgnoreCase);
        }

        _indexingHasResult = true;

        if (_referencesTarget != null)
        {
            UpdateSelectedReferencePaths(_referencesTarget);
        }
    }

    private void UpdateSelectedReferencePaths(AttackSO selectedAttackSo)
    {
        if (selectedAttackSo == null)
        {
            _referencePaths.Clear();
            return;
        }

        if (_referencesByAttack.TryGetValue(selectedAttackSo, out var paths) && paths != null)
        {
            _referencePaths = paths;
            return;
        }

        _referencePaths = new List<string>();
    }

    private sealed class AttackTypeGroupMarker
    {
        public Type AttackType { get; }

        public AttackTypeGroupMarker(Type attackType)
        {
            AttackType = attackType;
        }
    }
}
