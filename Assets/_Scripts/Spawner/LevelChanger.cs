using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class LevelChanger : MonoBehaviour
{
    [Header("Lvl upgrader")]
    [SerializeField, Tooltip("¬рем€ в секундах перед улучшением уровн€ спавнера ")] private float _timeBeforeLvlUp = 60f;
    private float _lvlTimer;

    [Header("Enemy count upgrader")]
    [SerializeField, Tooltip("¬рем€ в секундах перед улучшением максимального количества врагов  ")] private float _timeBeforeCountUp = 30f;
    [SerializeField, Tooltip("Ќасколько увеличиваетс€ максимальное количество врагов ")] private int _countUpgrade = 2;
    private float _countTimer;

    private SpawnSystem _spawnSystem;

    private void Start() =>  _spawnSystem = GetComponent<SpawnSystem>();
    

    private void Update()
    {
        _countTimer += Time.deltaTime;
        _lvlTimer += Time.deltaTime;

        if (_countTimer >= _timeBeforeCountUp)
        {
            _countTimer = 0;
            CountUpgrade();
        }

        if (_lvlTimer >= _timeBeforeLvlUp)
        {
            _lvlTimer = 0;
            LvlUp();
        }
    }

    public void LvlUp()
    {
        if (_spawnSystem != null) _spawnSystem.LvlUp();
    }

    public void CountUpgrade()
    {
        if (_spawnSystem != null) _spawnSystem.SpawnCountUpgrader(_countUpgrade);
    }
}