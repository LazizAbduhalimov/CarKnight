using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettngForMainGame : MonoBehaviour
{
    [SerializeField] private GamePanelUI _gamePanelUI;

    [Header("Main settings")]
    [SerializeField] private GameObject _settingButton;
    [SerializeField] private GameObject _settingPanel;

    [SerializeField] private KeyCode _keyCodeToOpenSettings;

    private bool _isActivated;


    private void Update()
    {
        if (Input.GetKeyDown(_keyCodeToOpenSettings))
        {
            if(_isActivated)
            {
                CloseSettings();
            }
            else
            {
                OpenSettings();
            }
        }
    }

    private void OnEnable()
    {
        _gamePanelUI.tabChange += TabActivated;
    }

    private void OnDisable()
    {
        _gamePanelUI.tabChange -= TabActivated;

    }

    private void TabActivated(bool obj)
    {
        CloseSettings();
    }

    public void OpenSettings()
    {
        _settingButton.SetActive(false);
        _settingPanel.SetActive(true);

        Time.timeScale = 0f;
        _isActivated = true;
    }

    public void CloseSettings()
    {
        _settingButton.SetActive(true);
        _settingPanel.SetActive(false);

        Time.timeScale = 1f;
        _isActivated = false;
    }

    public void Leave()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

}
