using System;
using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "NewAttackType", menuName = "AttackType/AttackSO", order = -10000)]
public class AttackSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField, ReadOnly] private string _attackId;

    [TabGroup("AttackData", "Spec")]
    [SerializeField, InlineEditor(InlineEditorModes.GUIOnly)] 
    [TableList]private AttackSpecBase _spec;

    [TabGroup("AttackData", "Level Table")]
    [SerializeField, InlineEditor(InlineEditorModes.GUIOnly)] 
    private AttackLevelTableSO _levelTable;

    [TabGroup("AttackData", "Sound")]
    [SerializeField] private AttackShotAudioDefinition _shotAudio;

    [TabGroup("AttackData", "UI Info")]
    [PreviewField]
    public Sprite AbilityIcon;
    [TabGroup("AttackData", "UI Info")]
    public string AbilityName;
    [TabGroup("AttackData", "UI Info")]
    [TextArea] public string AbilityDescription;

    public string AttackId => _attackId;
    public AttackSpecBase Spec => _spec;
    public AttackLevelTableSO LevelTable => _levelTable;
    public AttackShotAudioDefinition ShotAudio => _shotAudio;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrWhiteSpace(_attackId))
        {
            return;
        }

        _attackId = Guid.NewGuid().ToString("N");
    }
#endif
}
