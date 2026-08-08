using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeGameManager : MonoBehaviour
{
    [Header("TImer")]
    [SerializeField] private int _timer;
    [SerializeField] private SuperTextMesh _textMesh;
    [SerializeField] private GameObject _endMenuPanel;
    [SerializeField] private GameFinalizator _finalizator;

    private int _minutes = 0;
    private Coroutine _timerRoutine; 
    public Action onTimeEnd;

    void Start()
    {
        _minutes = _timer;
        _timerRoutine = StartCoroutine(SceneCicle());
    }

    IEnumerator SceneCicle()
    {
        while (_minutes > 0)
        {
            _minutes--;
            int seconds = 59;
            while (seconds >= 0)
            {
                _textMesh.text = $"<j=underground>{_minutes}:{seconds:00}";
                yield return new WaitForSeconds(1f);
                seconds--;
            }
        }
        onTimeEnd?.Invoke();
    }

    public void EndTimer()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine); 
            _timerRoutine = null;
        }

        _minutes = 0;
        _textMesh.text = $"<j=underground>0:00"; 

        onTimeEnd?.Invoke(); 
    }

    public void End()
    {
        _finalizator.End();
        Time.timeScale = 0;
        _endMenuPanel.SetActive(true);
    }

    public void LoadFirst()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1f;
    }
}
