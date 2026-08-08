using UnityEngine;

public readonly struct AudioHandle
{
    internal readonly AudioSource Source;

    internal AudioHandle(AudioSource source)
    {
        Source = source;
    }

    public bool IsValid => Source != null;
}

public interface IGameAudioService
{
    AudioHandle Play(AudioCueId cueId);
    AudioHandle PlayAt(AudioCueId cueId, Vector3 position);
    AudioHandle PlayAt(AttackShotAudioDefinition shotAudio, Vector3 position);
    AudioHandle PlayAttached(AudioCueId cueId, Transform target);
    void Stop(AudioHandle handle);
    void PlayMusic(AudioCueId cueId);
    float GetVolume(AudioBus bus);
    void SetVolume(AudioBus bus, float value);
    bool IsMuted(AudioBus bus);
    void SetMuted(AudioBus bus, bool muted);
}
