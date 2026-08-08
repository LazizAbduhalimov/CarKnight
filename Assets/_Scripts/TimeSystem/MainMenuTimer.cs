using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTimer : MonoBehaviour
{
    [SerializeField] private GameObject _timePanel;
    [SerializeField] private List<int> _timePerLvl = new List<int>();
    [SerializeField] private List<SuperTextMesh> _lvlText = new List<SuperTextMesh>();
    [SerializeField] private SuperTextMesh _remainingText;

    [SerializeField] private int _totalMinutes = 15;

    [SerializeField] private int _minPerLvl = 1;


    [Header("error panel")]
    [SerializeField] private int _timeForTabel;

    [SerializeField] private Color _errorTextColor;
    [SerializeField] private RectTransform _tableRect;
    [SerializeField] private Vector2 _startPos;
    [SerializeField] private Vector2 _endPos;
    [SerializeField] private float _moveTime = 0.5f;
    [SerializeField] private float _textFrequency;
    bool _isErrorActive = false;
    private void Start()
    {
        _remainingText.text = "Remaining: " + _totalMinutes;

    }

    public void PlusToLevel(int lvl)
    {
        if (lvl > _timePerLvl.Count)
        {
            Debug.LogError($"There isnt lvl {lvl}");
        }

        if (_totalMinutes <= 0)
        {
            Debug.Log("You cant do it cuz total time is zero now");
            return;
        }

        _timePerLvl[lvl - 1]++;
        _lvlText[lvl - 1].text = _timePerLvl[lvl - 1].ToString();

        _totalMinutes--;
        _remainingText.text = "Remaining: " + _totalMinutes;
    }

    public void MinusToLevel(int lvl)
    {
        if (lvl > _timePerLvl.Count)
        {
            Debug.LogError($"There isnt lvl {lvl}");
        }

        if (_timePerLvl[lvl - 1] <= _minPerLvl)
        {
            Debug.Log("You cant do it cuz number is below min number");
            return;
        }
        _timePerLvl[lvl - 1]--;
        _lvlText[lvl - 1].text = _timePerLvl[lvl - 1].ToString();

        _totalMinutes++;
        _remainingText.text = "Remaining: " + _totalMinutes;
    }


    public void TryToStart(int sceneId)
    {
        if (_isErrorActive)
        {
            return;
        }
        for (int i = 0; i < _timePerLvl.Count; i++)
        {
            if (_timePerLvl[i] < _minPerLvl)
            {
                StartCoroutine(ShowTabel(i));
                Debug.Log("U cant start");
                _isErrorActive = true;
                return;
            }
        }
        StartGame(sceneId);
    }


    private void StartGame(int sceneId)
    {

        PlayerPrefs.DeleteKey("LevelTimes");
        string data = string.Join(",", _timePerLvl);
        PlayerPrefs.SetString("LevelTimes", data);

        GetComponent<MainMenuUIManager>().LoadScene(sceneId);

    }


    IEnumerator ShowTabel(int lvlID)
    {
        float time = 0;

        _tableRect.anchoredPosition = _startPos;

        StartCoroutine(BlinkText(lvlID));

        while (time < _moveTime)
        {
            time += Time.deltaTime;

            float t = time / _moveTime;
            t = Mathf.SmoothStep(0, 1, t);

            _tableRect.anchoredPosition = Vector2.Lerp(_startPos, _endPos, t);

            yield return null;
        }

        _tableRect.anchoredPosition = _endPos;

        yield return new WaitForSeconds(_timeForTabel);

        time = 0;

        while (time < _moveTime)
        {
            time += Time.deltaTime;

            float t = time / _moveTime;
            t = Mathf.SmoothStep(0, 1, t);

            _tableRect.anchoredPosition = Vector2.Lerp(_endPos, _startPos, t);

            yield return null;
        }

        _tableRect.anchoredPosition = _startPos;
        _isErrorActive = false;
    }

    IEnumerator BlinkText(int lvl)
    {
        float time = 0;
        bool inColorNow = false;

        Color col = _lvlText[lvl].color;
        float blinking = _timeForTabel + _moveTime * 2;
        while (time < blinking)
        {
            time += _textFrequency;

            inColorNow = !inColorNow;

            if (inColorNow)
                _lvlText[lvl].color = _errorTextColor;
            else
                _lvlText[lvl].color = col;

            yield return new WaitForSeconds(_textFrequency);
        }

        _lvlText[lvl].color = col;
    }

    public void ActiveOrDeactivePanel()
    {

        _timePanel.SetActive(!_timePanel.activeSelf);
    }

}
