using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

public sealed class GameAudioService : IGameAudioService
{
    private const float MinimumVolume = 0.0001f;

    private readonly GameAudioSettingsSO _settings;
    private readonly MMSoundManager _backend;
    private readonly Dictionary<AudioCueId, float> _lastPlayTimes = new();
    private readonly Dictionary<AttackShotAudioDefinition, float> _lastShotPlayTimes = new();
    private AudioSource _musicSource;

    public GameAudioService(GameAudioSettingsSO settings, MMSoundManager backend)
    {
        _settings = settings;
        _backend = backend;
        LoadSettings();
    }

    public AudioHandle Play(AudioCueId cueId) => PlayInternal(cueId, Vector3.zero, null);

    public AudioHandle PlayAt(AudioCueId cueId, Vector3 position) => PlayInternal(cueId, position, null);

    public AudioHandle PlayAt(AttackShotAudioDefinition shotAudio, Vector3 position)
    {
        return PlayInternal(shotAudio, position);
    }

    public AudioHandle PlayAttached(AudioCueId cueId, Transform target)
    {
        return PlayInternal(cueId, target != null ? target.position : Vector3.zero, target);
    }

    public void Stop(AudioHandle handle)
    {
        if (handle.IsValid) _backend.StopSound(handle.Source);
    }

    public void PlayMusic(AudioCueId cueId)
    {
        if (!_settings.Catalog.TryGet(cueId, out var cue) || cue.Bus != AudioBus.Music) return;

        var clip = cue.GetClip();
        if (clip == null) return;

        if (_musicSource != null && _musicSource.clip == clip && _musicSource.isPlaying) return;

        var options = CreateOptions(cue, Vector3.zero, null);
        options.Persistent = true;
        options.Loop = true;
        options.Fade = _musicSource != null;
        options.FadeInitialVolume = 0f;
        options.FadeDuration = _settings.MusicCrossfadeDuration;

        var nextSource = _backend.PlaySound(clip, options);
        if (_musicSource != null)
            _backend.FadeSound(_musicSource, _settings.MusicCrossfadeDuration, _musicSource.volume, 0f, null, true);

        _musicSource = nextSource;
    }

    public float GetVolume(AudioBus bus)
    {
        return PlayerPrefs.GetFloat(GetVolumeKey(bus), GetLegacyVolume(bus));
    }

    public void SetVolume(AudioBus bus, float value)
    {
        var normalized = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(GetVolumeKey(bus), normalized);
        ApplyVolume(bus, normalized);
    }

    public bool IsMuted(AudioBus bus) => PlayerPrefs.GetInt(GetMuteKey(bus), 0) == 1;

    public void SetMuted(AudioBus bus, bool muted)
    {
        PlayerPrefs.SetInt(GetMuteKey(bus), muted ? 1 : 0);
        ApplyVolume(bus, GetVolume(bus));
    }

    private AudioHandle PlayInternal(AudioCueId cueId, Vector3 position, Transform target)
    {
        if (!_settings.Catalog.TryGet(cueId, out var cue))
        {
            Debug.LogWarning($"[SOUND] Cue is not configured: {cueId}");
            return default;
        }

        if (!CanPlay(cue)) return default;

        var clip = cue.GetClip();
        if (clip == null)
        {
            Debug.LogWarning($"[SOUND] Cue has no clips: {cueId}");
            return default;
        }

        var source = _backend.PlaySound(clip, CreateOptions(cue, position, target));
        return new AudioHandle(source);
    }

    private AudioHandle PlayInternal(AttackShotAudioDefinition shotAudio, Vector3 position)
    {
        if (shotAudio == null)
        {
            Debug.LogWarning("[SOUND] Shot request ignored: audio profile is null.");
            return default;
        }

        if (!CanPlay(shotAudio)) return default;

        var clip = shotAudio.GetClip();
        if (clip == null)
        {
            Debug.LogWarning("[SOUND] Attack shot audio has no clips.");
            return default;
        }

        var options = CreateOptions(shotAudio, position);
        var source = _backend.PlaySound(clip, options);
        var mixerGroupName = source != null && source.outputAudioMixerGroup != null
            ? source.outputAudioMixerGroup.name
            : "<none>";

        if (source == null)
        {
            Debug.LogError($"[SOUND] Shot source was not created. clip='{clip.name}', mixerGroup='{mixerGroupName}'.");
        }
        else
        {
            Debug.Log($"[SOUND] Shot played. clip='{clip.name}', mixerGroup='{mixerGroupName}', " +
                      $"volume={source.volume:0.###}, spatialBlend={source.spatialBlend:0.###}, " +
                      $"position={position}, isPlaying={source.isPlaying}.");
        }

        return new AudioHandle(source);
    }

    private MMSoundManagerPlayOptions CreateOptions(AudioCueDefinition cue, Vector3 position, Transform target)
    {
        var options = MMSoundManagerPlayOptions.Default;
        options.MmSoundManagerTrack = GetFeelTrack(cue.Bus);
        options.AudioGroup = _settings.GetGroup(cue.Bus);
        options.Location = position;
        options.AttachToTransform = target;
        options.Loop = cue.Loop;
        options.Volume = cue.GetVolume();
        options.Pitch = cue.GetPitch();
        options.SpatialBlend = cue.Spatial ? 1f : 0f;
        options.MinDistance = cue.MinDistance;
        options.MaxDistance = cue.MaxDistance;
        options.Priority = cue.Priority;
        return options;
    }

    private MMSoundManagerPlayOptions CreateOptions(AttackShotAudioDefinition shotAudio, Vector3 position)
    {
        var options = MMSoundManagerPlayOptions.Default;
        options.MmSoundManagerTrack = MMSoundManager.MMSoundManagerTracks.Sfx;
        options.AudioGroup = _settings.GetGroup(AudioBus.Sfx);
        options.Location = position;
        options.Volume = shotAudio.GetVolume();
        options.Pitch = shotAudio.GetPitch();
        options.SpatialBlend = shotAudio.Spatial ? 1f : 0f;
        options.MinDistance = shotAudio.MinDistance;
        options.MaxDistance = shotAudio.MaxDistance;
        options.Priority = shotAudio.Priority;
        return options;
    }

    private bool CanPlay(AudioCueDefinition cue)
    {
        if (cue.Cooldown <= 0f) return true;

        var now = Time.unscaledTime;
        if (_lastPlayTimes.TryGetValue(cue.Id, out var lastPlay) && now - lastPlay < cue.Cooldown) return false;

        _lastPlayTimes[cue.Id] = now;
        return true;
    }

    private bool CanPlay(AttackShotAudioDefinition shotAudio)
    {
        if (shotAudio.Cooldown <= 0f) return true;

        var now = Time.unscaledTime;
        if (_lastShotPlayTimes.TryGetValue(shotAudio, out var lastPlay) && now - lastPlay < shotAudio.Cooldown)
            return false;

        _lastShotPlayTimes[shotAudio] = now;
        return true;
    }

    private void LoadSettings()
    {
        foreach (AudioBus bus in System.Enum.GetValues(typeof(AudioBus)))
        {
            if (!PlayerPrefs.HasKey(GetVolumeKey(bus)))
                PlayerPrefs.SetFloat(GetVolumeKey(bus), GetLegacyVolume(bus));

            ApplyVolume(bus, GetVolume(bus));
        }

        PlayerPrefs.Save();
    }

    private void ApplyVolume(AudioBus bus, float value)
    {
        if (_settings.Mixer == null) return;

        var actualValue = IsMuted(bus) ? MinimumVolume : Mathf.Max(MinimumVolume, value);
        _settings.Mixer.SetFloat(_settings.GetParameter(bus), Mathf.Log10(actualValue) * 20f);
    }

    private static MMSoundManager.MMSoundManagerTracks GetFeelTrack(AudioBus bus)
    {
        return bus switch
        {
            AudioBus.Master => MMSoundManager.MMSoundManagerTracks.Master,
            AudioBus.Music => MMSoundManager.MMSoundManagerTracks.Music,
            AudioBus.Sfx => MMSoundManager.MMSoundManagerTracks.Sfx,
            AudioBus.Ui => MMSoundManager.MMSoundManagerTracks.UI,
            AudioBus.Voice => MMSoundManager.MMSoundManagerTracks.Other,
            _ => MMSoundManager.MMSoundManagerTracks.Other
        };
    }

    private static string GetVolumeKey(AudioBus bus) => $"GameAudio.Volume.{bus}";
    private static string GetMuteKey(AudioBus bus) => $"GameAudio.Mute.{bus}";

    private static float GetLegacyVolume(AudioBus bus)
    {
        return bus switch
        {
            AudioBus.Music => PlayerPrefs.GetFloat("MusicVolume", 0.5f),
            AudioBus.Sfx => PlayerPrefs.GetFloat("VFXvolume", 0.5f),
            _ => 1f
        };
    }
}
