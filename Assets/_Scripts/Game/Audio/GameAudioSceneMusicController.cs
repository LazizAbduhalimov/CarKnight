using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public sealed class GameAudioSceneMusicController : IInitializable, IDisposable
{
    private readonly IGameAudioService _audio;
    private readonly GameAudioSettingsSO _settings;

    public GameAudioSceneMusicController(IGameAudioService audio, GameAudioSettingsSO settings)
    {
        _audio = audio;
        _settings = settings;
    }

    public void Initialize()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        PlaySceneMusic(SceneManager.GetActiveScene());
    }

    public void Dispose()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlaySceneMusic(scene);
    }

    private void PlaySceneMusic(Scene scene)
    {
        var cueId = scene.name switch
        {
            "MainScene" => AudioCueId.MusicMain,
            "level_01" => AudioCueId.MusicGameplay,
            "Tutorial" => AudioCueId.MusicTutorial,
            _ => AudioCueId.None
        };

        if (cueId == AudioCueId.None) return;

        StopSceneMusicSources(scene);
        _audio.PlayMusic(cueId);
    }

    private void StopSceneMusicSources(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        foreach (var source in root.GetComponentsInChildren<AudioSource>(true))
        {
            if (source.outputAudioMixerGroup != _settings.GetGroup(AudioBus.Music)) continue;

            source.Stop();
            source.playOnAwake = false;
        }
    }
}
