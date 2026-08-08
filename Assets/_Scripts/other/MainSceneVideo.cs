using UnityEngine;
using UnityEngine.Video;

public class MainSceneVideo : MonoBehaviour
{
    [SerializeField] private MainMenuUIManager _UImanager;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private GameObject _videoPanel;  
    [SerializeField] private GameObject _music;  
    [SerializeField] private GameObject _skipButton;
    [SerializeField] private int _nextSceneID;

    private static bool _isFirstView = true;

    private void Start()
    {
        if (_videoPanel != null)
            _videoPanel.SetActive(false);

        if (_skipButton != null)
            _skipButton.SetActive(!_isFirstView);

        _videoPlayer.loopPointReached += OnVideoFinished;
    }

    public void StartVideo()
    {
        _music.SetActive(false);
        if (_videoPanel != null)
            _videoPanel.SetActive(true);

        _videoPlayer.Play();
    }

    public void SkipVideo()
    {
        _videoPlayer.Stop();
        LoadNextScene();
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        _isFirstView = false;

        if (_skipButton != null)
            _skipButton.SetActive(true);

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (_videoPanel != null)
            _videoPanel.SetActive(false);

        _UImanager.LoadScene(_nextSceneID);
    }

    private void OnDestroy()
    {
        _videoPlayer.loopPointReached -= OnVideoFinished;
    }
}