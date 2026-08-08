using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "GameAudioSettings", menuName = "Game/Audio/Settings")]
public sealed class GameAudioSettingsSO : ScriptableObject
{
    [SerializeField] private AudioCatalogSO _catalog;
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private AudioMixerGroup _masterGroup;
    [SerializeField] private AudioMixerGroup _musicGroup;
    [SerializeField] private AudioMixerGroup _sfxGroup;
    [SerializeField] private AudioMixerGroup _uiGroup;
    [SerializeField] private AudioMixerGroup _voiceGroup;
    [SerializeField] private MMSoundManagerSettingsSO _feelSettings;
    [SerializeField, Min(1)] private int _poolSize = 20;
    [SerializeField, Min(0.01f)] private float _musicCrossfadeDuration = 1f;

    public AudioCatalogSO Catalog => _catalog;
    public AudioMixer Mixer => _mixer;
    public MMSoundManagerSettingsSO FeelSettings => _feelSettings;
    public int PoolSize => _poolSize;
    public float MusicCrossfadeDuration => _musicCrossfadeDuration;

    public AudioMixerGroup GetGroup(AudioBus bus)
    {
        return bus switch
        {
            AudioBus.Master => _masterGroup,
            AudioBus.Music => _musicGroup,
            AudioBus.Sfx => _sfxGroup,
            AudioBus.Ui => _uiGroup,
            AudioBus.Voice => _voiceGroup,
            _ => null
        };
    }

    public string GetParameter(AudioBus bus)
    {
        return bus switch
        {
            AudioBus.Master => "Master",
            AudioBus.Music => "MusicTheme",
            AudioBus.Sfx => "VFX",
            AudioBus.Ui => "UI",
            AudioBus.Voice => "Dialogues",
            _ => string.Empty
        };
    }
}
