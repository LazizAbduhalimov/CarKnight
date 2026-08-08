using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Zenject;

public class SettingsMenu : MonoBehaviour
{
    [Header("Scrolls")]
    [SerializeField] private Scrollbar _musicScroll;
    [SerializeField] private Scrollbar _VFXScroll;
    [SerializeField] private Scrollbar _masterScroll;
    [SerializeField] private Scrollbar _uiScroll;
    [SerializeField] private Scrollbar _voiceScroll;

    [Header("Mixer")]
    [SerializeField] private AudioMixer _mixer;


    private const string MusicParam = "MusicTheme";
    private const string VFXParam = "VFX";
    private IGameAudioService _audio;

    [Inject]
    private void Construct([InjectOptional] IGameAudioService audio)
    {
        _audio = audio;
    }

    private void Start()
    {
        InitialCheck();
    }

    private void InitialCheck()
    {
        if (_mixer == null && _audio == null)
        {
            Debug.Log("Music mixer is null");
            return;
        }

        float savedValue = _audio != null
            ? _audio.GetVolume(AudioBus.Music)
            : PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        float vfxValue = _audio != null
            ? _audio.GetVolume(AudioBus.Sfx)
            : PlayerPrefs.GetFloat("VFXvolume", 0.5f);

        if (_musicScroll != null)
        {
            _musicScroll.value = savedValue;
        }

        if (_VFXScroll != null)
        {
            _VFXScroll.value = vfxValue;
        }

        InitializeOptionalScroll(_masterScroll, AudioBus.Master);
        InitializeOptionalScroll(_uiScroll, AudioBus.Ui);
        InitializeOptionalScroll(_voiceScroll, AudioBus.Voice);

        SetMusicVolume(savedValue);
        SetVFXVolume(vfxValue);
    }

    public void OnScrollMusicChanged()
    {
        float volume = Mathf.Clamp(_musicScroll.value, 0.0001f, 1f);

        _musicScroll.value = volume;

        SetMusicVolume(volume);

        PlayerPrefs.SetFloat("MusicVolume", volume);
    }


    public void OnScrollVFXChanged()
    {
        float volume = Mathf.Clamp(_VFXScroll.value, 0.0001f, 1f);

        _VFXScroll.value = volume;

        SetVFXVolume(volume);

        PlayerPrefs.SetFloat("VFXvolume", volume);
    }

    public void OnScrollMasterChanged() => SetOptionalScrollVolume(_masterScroll, AudioBus.Master);
    public void OnScrollUIChanged() => SetOptionalScrollVolume(_uiScroll, AudioBus.Ui);
    public void OnScrollVoiceChanged() => SetOptionalScrollVolume(_voiceScroll, AudioBus.Voice);

    private void SetMusicVolume(float value)
    {
        if (_audio != null)
        {
            _audio.SetVolume(AudioBus.Music, value);
            return;
        }

        float dB = Mathf.Log10(value) * 20;
        _mixer.SetFloat(MusicParam, dB);
    }
    private void SetVFXVolume(float value)
    {
        if (_audio != null)
        {
            _audio.SetVolume(AudioBus.Sfx, value);
            return;
        }

        float dB = Mathf.Log10(value) * 20;
        _mixer.SetFloat(VFXParam, dB);
    }

    private void InitializeOptionalScroll(Scrollbar scroll, AudioBus bus)
    {
        if (scroll != null && _audio != null) scroll.value = _audio.GetVolume(bus);
    }

    private void SetOptionalScrollVolume(Scrollbar scroll, AudioBus bus)
    {
        if (scroll == null || _audio == null) return;

        var volume = Mathf.Clamp(scroll.value, 0.0001f, 1f);
        scroll.value = volume;
        _audio.SetVolume(bus, volume);
    }
}
