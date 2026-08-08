using MoreMountains.Tools;
using UnityEngine;
using Zenject;

public sealed class GameAudioInstaller : MonoInstaller
{
    [SerializeField] private GameAudioSettingsSO _settings;
    [SerializeField] private MMSoundManager _soundManager;

    public override void InstallBindings()
    {
        if (_settings == null)
        {
            Debug.LogError("GameAudioSettings is not assigned in GameAudioInstaller.", this);
            return;
        }

        if (_soundManager == null)
        {
            Debug.LogError("MMSoundManager is not assigned in GameAudioInstaller.", this);
            return;
        }

        Container.Bind<IGameAudioService>()
            .To<GameAudioService>()
            .AsSingle()
            .WithArguments(_settings, _soundManager)
            .NonLazy();

        Container.BindInterfacesTo<GameAudioSceneMusicController>()
            .AsSingle()
            .WithArguments(_settings)
            .NonLazy();
    }
}
