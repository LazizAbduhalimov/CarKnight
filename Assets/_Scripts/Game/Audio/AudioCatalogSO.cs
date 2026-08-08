using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "GameAudioCatalog", menuName = "Game/Audio/Catalog")]
public sealed class AudioCatalogSO : ScriptableObject
{
    [SerializeField, TableList]
    private List<AudioCueDefinition> _cues = new();

    private readonly Dictionary<AudioCueId, AudioCueDefinition> _byId = new();
    private bool _isCached;

    public bool TryGet(AudioCueId id, out AudioCueDefinition cue)
    {
        Cache();
        return _byId.TryGetValue(id, out cue);
    }

    private void Cache()
    {
        if (_isCached) return;

        _isCached = true;
        _byId.Clear();

        foreach (var cue in _cues)
        {
            if (cue == null || cue.Id == AudioCueId.None) continue;

            if (_byId.ContainsKey(cue.Id))
            {
                Debug.LogError($"[GameAudio] Duplicate cue id: {cue.Id}", this);
                continue;
            }

            _byId.Add(cue.Id, cue);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        _isCached = false;
    }
#endif
}
