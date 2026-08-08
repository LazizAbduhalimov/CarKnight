using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class AudioCueDefinition
{
    [SerializeField] private AudioCueId _id;
    [SerializeField] private AudioBus _bus = AudioBus.Sfx;
    [SerializeField] private AudioClip[] _clips;
    [SerializeField, Range(0f, 2f)] private float _volume = 1f;
    [SerializeField] private Vector2 _pitchRange = Vector2.one;
    [SerializeField] private bool _loop;
    [SerializeField] private bool _spatial;
    [SerializeField, MinValue(0f)] private float _minDistance = 1f;
    [SerializeField, MinValue(0f)] private float _maxDistance = 30f;
    [SerializeField, Range(0, 256)] private int _priority = 128;
    [SerializeField, MinValue(0f)] private float _cooldown;

    public AudioCueId Id => _id;
    public AudioBus Bus => _bus;
    public bool Loop => _loop;
    public bool Spatial => _spatial;
    public float MinDistance => _minDistance;
    public float MaxDistance => Mathf.Max(_minDistance, _maxDistance);
    public int Priority => _priority;
    public float Cooldown => _cooldown;

    public AudioClip GetClip()
    {
        if (_clips == null || _clips.Length == 0) return null;

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
