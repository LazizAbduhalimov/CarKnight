using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

public class GameFinalizator : MonoBehaviour
{
    [Header("UI Texts")]
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _enemyKilledText;
    [SerializeField] private TextMeshProUGUI _levelUppedText;
    [SerializeField] private TextMeshProUGUI _distanceCoveredText;
    [SerializeField] private TextMeshProUGUI _killStreakText;
    [SerializeField] private TextMeshProUGUI _damageGettedText;

    [Header("For Grid")]
    [SerializeField] private GameObject _endPanel;
    [SerializeField] private GameObject _gridObject;
    [SerializeField] private List<finalIcons> _finalIcons = new List<finalIcons>();



    private float _minutesInGame;
    private int _killedCount;
    private float _distanceCovered;
    private int _damageGetted;

    private int _currentStreak;
    private int _bestStreak;

    private GameEvents _gameEvents;
    private HealthComponent _playerHealth;
    private Attacker _playerAttacks;
    private Ram _forPlayerLvl;

    private Transform _playerTransform;
    private Vector3 _lastPosition;
    public bool _isRunning;

    [Inject]public void Initial(PlayerUnit player, GameEvents events)
    {
        _playerHealth = player.GetComponent<HealthComponent>();
        _playerAttacks = player.GetComponent<Attacker>();
        _forPlayerLvl = player.GetComponent<Ram>();
        _playerTransform = player.transform;
        _gameEvents = events;

        _minutesInGame = 0f;
        _killedCount = 0;
        _distanceCovered = 0f;
        _damageGetted = 0;
        _currentStreak = 0;
        _bestStreak = 0;

        _lastPosition = _playerTransform.position;
        _isRunning = true;

        _gameEvents.EnemyKilled += OnEnemyKilled;
        _playerHealth.OnHealthChanged += OnPlayerHealthChanged;

        _endPanel.SetActive(false);
    }

    private void Update()
    {
        if (!_isRunning) return;
        
        _minutesInGame += Time.deltaTime / 60f;

        var pos = _playerTransform.position;
        _distanceCovered += Vector3.Distance(pos, _lastPosition);
        _lastPosition = pos;
    }

    public void End()
    {
        _isRunning = false;
        DrawAbilities();
        _endPanel.SetActive(true);

        _gameEvents.EnemyKilled -= OnEnemyKilled;
        _playerHealth.OnHealthChanged -= OnPlayerHealthChanged;

        int level = _forPlayerLvl != null ? _forPlayerLvl._currentLevelIndex : 0;

        _timeText.text = $"Время затраченное на игру: {FormatTime(_minutesInGame)}";
        _enemyKilledText.text = $"Врагов убито: {_killedCount}";
        _levelUppedText.text = $"Уровней поднято: {level}";
        _distanceCoveredText.text = $"Расстояния пройдено: {(int)_distanceCovered}м";
        _killStreakText.text = $"Серия убийств без получения урона: {_bestStreak}";
        _damageGettedText.text = $"Всего урона получено: {_damageGetted}";
    }


    private void OnEnemyKilled(long enemyID)
    {
        _killedCount++;
        _currentStreak++;

        if (_currentStreak > _bestStreak)
            _bestStreak = _currentStreak;
    }

    private void OnPlayerHealthChanged(int oldHp, int newHp, int maxHp)
    {
        if (newHp < oldHp)                      
        {
            int dmg = oldHp - newHp;
            _damageGetted += dmg;
            _currentStreak = 0;                   
        }
    }

    private string FormatTime(float minutes)
    {
        int totalSeconds = Mathf.RoundToInt(minutes * 60f);
        int m = totalSeconds / 60;
        int s = totalSeconds % 60;
        return $"{m} мин {s} сек";
    }

    private void OnDestroy()
    {
        if (_gameEvents != null)
            _gameEvents.EnemyKilled -= OnEnemyKilled;

        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= OnPlayerHealthChanged;
    }

    private void DrawAbilities()
    {

        for (int i = 0; i < _finalIcons.Count; i++)
        {
            if (i < _playerAttacks.OwnedAttacks.Count)
            {
                _finalIcons[i].gameObject.SetActive(true);
                _finalIcons[i].ShowIcon(_playerAttacks.OwnedAttacks[i].Config);
            }
            else
            {
                _finalIcons[i].gameObject.SetActive(false);
            }
        }
    }
}