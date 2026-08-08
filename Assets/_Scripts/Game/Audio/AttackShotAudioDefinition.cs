using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class AttackShotAudioDefinition
{
    [SerializeField] private AudioClip[] _clips;
    [SerializeField, Range(0f, 2f)] private float _volume = 1f;
    [SerializeField] private Vector2 _pitchRange = Vector2.one;
    [SerializeField] private bool _spatial = true;
    [SerializeField, MinValue(0f)] private float _minDistance = 1f;
    [SerializeField, MinValue(0f)] private float _maxDistance = 35f;
    [SerializeField, Range(0, 256)] private int _priority = 80;
    [SerializeField, MinValue(0f)] private float _cooldown;

    public bool HasClips
    {
        get
        {
            if (_clips == null) return false;

            for (var i = 0; i < _clips.Length; i++)
            {
                if (_clips[i] != null) return true;
            }

            return false;
        }
    }
    public bool Spatial => _spatial;
    public float MinDistance => _minDistance;
    public float MaxDistance => Mathf.Max(_minDistance, _maxDistance);
    public int Priority => _priority;
    public float Cooldown => _cooldown;

    public AudioClip GetClip()
    {
        if (!HasClips) return null;

        var startIndex = UnityEngine.Random.Range(0, _clips.Length);
        for (var offset = 0; offset < _clips.Length; offset++)
        {
            var clip = _clips[(startIndex + offset) % _clips.Length];
            if (clip != null) return clip;
        }

        return null;
    }

    public float GetVolume() => _volume;

    public float GetPitch()
    {
        var min = Mathf.Min(_pitchRange.x, _pitchRange.y);
        var max = Mathf.Max(_pitchRange.x, _pitchRange.y);
        return UnityEngine.Random.Range(min, max);
    }
}
