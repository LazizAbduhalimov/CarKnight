using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _settingsPanel;

    [Header("Load Menu")]
    [SerializeField] private GameObject _sceneLoadPanel;
    [SerializeField] private Image _loadBar;

    [Header("Tutorial")]
    [SerializeField] private GameObject _tutorialOn;
    [SerializeField] private GameObject _tutorialOff;
    [SerializeField] private GameObject _tutorialLoadPanel;
    [SerializeField] private Image _loadBarTutorial;
    [SerializeField] private int _tutorialScene;
    public static bool _isTutroialActive = true;



    private void Start()
    {
        if (_tutorialOn != null && _tutorialOff != null)
        {
            if (_isTutroialActive)
            {
                _tutorialOff.SetActive(true);
                _tutorialOn.SetActive(false);
            }
            else
            {
                _tutorialOff.SetActive(false);
                _tutorialOn.SetActive(true);
            }
        }
    }

    public void SetTutorial(bool activateTutorial)
    {
        _isTutroialActive = activateTutorial;

        if (_isTutroialActive)
        {
            _tutorialOff.SetActive(true);
            _tutorialOn.SetActive(false);
        }
        else
        {
            _tutorialOff.SetActive(false);
            _tutorialOn.SetActive(true);
        }
    }
    public void LoadSceneOrTutorial(int sceneId)
    {
        if (!_isTutroialActive)
        {
            LoadScene(sceneId);
        }
        else
        {
            StartCoroutine(LoadTutorialAsync());

        }
    }
    public void LoadScene(int sceneId)
    {
        _sceneLoadPanel.SetActive(true);
        StartCoroutine(LoadSceneAsync(sceneId));


    }

    private IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        operation.allowSceneActivation = false; 

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            _loadBar.fillAmount = progress;
 
            if (progress >= 1f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null; 
        }
    }

    private IEnumerator LoadTutorialAsync()
    {
        _tutorialLoadPanel.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(_tutorialScene);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            _loadBarTutorial.fillAmount = progress;

            if (progress >= 1f)
            {
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
    public void SettingButton()
    {
        _settingsPanel.SetActive(!_settingsPanel.activeInHierarchy);
    }

    public void Leave()
    {
        Application.Quit();
    }
}
