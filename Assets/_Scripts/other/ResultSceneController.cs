using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class ResultSceneController : MonoBehaviour
{
    [SerializeField] private bool _isVideoScene;
    public static bool IsWin;

    [Header("Win")]
    [SerializeField] private int _sceneIdWin;
    [SerializeField] private GameObject _winPanel;
    [SerializeField] private VideoPlayer _winVideo;

    [Header("Lose")]
    [SerializeField] private int _sceneIdLose;
    [SerializeField] private GameObject _losePanel;
    [SerializeField] private VideoPlayer _loseVideo;

    [Header("Scene")]
    [SerializeField] private int _mainMenuSceneID;

    private VideoPlayer _activeVideo;

    private void Start()
    {
        if (_isVideoScene)
        {
            _winPanel.SetActive(false);
            _losePanel.SetActive(false);

            if (IsWin)
            {
                _winPanel.SetActive(true);
                _activeVideo = _winVideo;
                _mainMenuSceneID = _sceneIdWin;
            }
            else
            {
                _losePanel.SetActive(true);
                _activeVideo = _loseVideo;
                _mainMenuSceneID = _sceneIdLose;

            }

            _activeVideo.loopPointReached += OnVideoFinished;
            _activeVideo.Play();
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        GoToMainMenu();
    }

    public void SkipVideo()
    {
        _activeVideo.Stop();
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene(_mainMenuSceneID);
    }

    public void ToNextScene(bool isWined)
    {
        IsWin = isWined;
        Time.timeScale = 1f;
        SceneManager.LoadScene(4);
    }

    private void OnDestroy()
    {
        if (_activeVideo != null)
            _activeVideo.loopPointReached -= OnVideoFinished;
    }
}