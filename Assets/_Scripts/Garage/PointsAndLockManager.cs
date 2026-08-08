using System.Collections.Generic;
using UnityEngine;

public class PointsAndLockManager : MonoBehaviour
{
    [Header("Garage scene")]
    [SerializeField] private SuperTextMesh _coinsText;
    [SerializeField] private List<LocksTransport> _locksTransportsUI = new List<LocksTransport>();
    [SerializeField] private List<bool> _isClosed = new List<bool>();
    [SerializeField] private List<bool> _winedOnThis = new List<bool>();

    [Header("Main game")]
    [SerializeField] private GameObject _continueButton;

    [Header("General")]
    private int _coins;
    public static bool newGame;

    public static MovementMode lastWinnedMode;
    public static bool haveWinned;

    private void Start()
    {
        if (_continueButton != null)
        {
            _continueButton.SetActive(PlayerPrefs.HasKey("TotalCoins"));
        }

        if (newGame)
        {
            ResetProgress();
            newGame = false;
        }
        LoadData();
        UpdateUI();
    }

    public void SaveData()
    {
        PlayerPrefs.SetInt("TotalCoins", _coins);

        for (int i = 0; i < _isClosed.Count; i++)
        {
            PlayerPrefs.SetInt("LockState_" + i, _isClosed[i] ? 1 : 0);
        }

        for (int i = 0; i < _winedOnThis.Count; i++)
        {
            PlayerPrefs.SetInt("Winned_" + i, _winedOnThis[i] ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        _coins = PlayerPrefs.GetInt("TotalCoins", 0);

        for (int i = 0; i < _isClosed.Count; i++)
        {
            int state = PlayerPrefs.GetInt("LockState_" + i, _isClosed[i] ? 1 : 0);
            _isClosed[i] = (state == 1);
        }

        if (haveWinned && _winedOnThis.Count > 0)
        {
            haveWinned = false;
            for (int i = 0; i < _locksTransportsUI.Count; i++)
            {
                if (_locksTransportsUI[i].movement == lastWinnedMode)
                {
                    PlayerPrefs.SetInt("Winned_" + i, 1);
                }
            }
            PlayerPrefs.Save();
        }

        for(int i = 0; i < _winedOnThis.Count;i++)
        {
            int state = PlayerPrefs.GetInt("Winned_" + i, 0);
            _winedOnThis[i] = (state == 1);

        }

    }

    public void UnlockTransport(int index)
    {
        if (index >= 0 && index < _isClosed.Count && _coins >= 1)
        {
            _coins -= 1;
            _isClosed[index] = false;
            UpdateUI();
            SaveData();
        }
    }


    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("TotalCoins");
        for (int i = 0; i < _isClosed.Count; i++)
        {
            PlayerPrefs.DeleteKey("LockState_" + i);
        }
        for (int i = 0; i < _winedOnThis.Count; i++)
        {
            PlayerPrefs.DeleteKey("Winned_" + i);
        }
        _coins = 0;
        PlayerPrefs.Save();

        CombinariumManager.DeleteSaveFile(); 
    }

    private void UpdateUI()
    {
        if(_locksTransportsUI.Count > 0)
        {
            for (int i = 0; i < _locksTransportsUI.Count; i++)
            {
                _locksTransportsUI[i].SetState(_isClosed[i]);
                _locksTransportsUI[i].MarkAsWinned(_winedOnThis[i]);
            }
        }
        if (_coinsText != null)
        {
            _coinsText.text = $"Очки улучшения:  {_coins}";
        }
    }

    public void Win()
    {
        _coins++;
        PlayerPrefs.SetInt("TotalCoins", _coins);
        haveWinned = true;
        lastWinnedMode = GarageViev.currentMove;
    }

    public void ResetState()
    {
        newGame = true;
    }
    public void ManualClear()
    {
        PlayerPrefs.DeleteAll(); 
        PlayerPrefs.Save();    
    }
}
