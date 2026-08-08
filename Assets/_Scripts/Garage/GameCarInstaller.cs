using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class GameCarInstaller : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private AudioSource _theme;

    [Header("Configs")]
    [SerializeField] private List<WheelsConfig> _wheelsConfig = new List<WheelsConfig>();
    [SerializeField] private List<GameObject> _carModelPrefabs = new List<GameObject>();
    [SerializeField] private List<int> _carHP = new List<int>();

    private PlayerUnit player;
    [Inject]
    private void Initialize(PlayerUnit player)
    {
       this.player = player;
    }

    private void Start()
    {
        WheelsConfig wheels = _wheelsConfig.Find(w => w.mode == GarageViev.currentMove);

        if (wheels == null)
        {
            Debug.LogError($"[GameCarInstaller] Конфиг для режима {GarageViev.currentMove} не найден в списке _wheelsConfig!");
            return;
        }

        int index = Mathf.Max(0, GarageViev.carLvlId - 1);
        if (index >= _carModelPrefabs.Count)
        {
            Debug.LogError($"[GameCarInstaller] Префаб с индексом {index} не найден! В списке всего {_carModelPrefabs.Count} машин.");
            return;
        }

        GameObject prefab = _carModelPrefabs[index];
        Transform playerTransform = player.transform;
        GameObject carObject = Instantiate(prefab, playerTransform);

        if (carObject.TryGetComponent(out CarWheelsView carView))
        {
            var movement = player.GetComponent<MovementFsmController>();
            movement.SetViev(carView);
            movement.SwitchTo(wheels.mode);
        }
        else
        {
            Debug.LogError($"[GameCarInstaller] На префабе {prefab.name} отсутствует компонент CarWheelsView!");
        }

        if (_theme != null)
        {
            _theme.Stop();
            _theme.playOnAwake = false;
        }

        if (player.TryGetComponent(out Attacker attacker))
        {
            if (attacker.ProgressionService != null)
            {
                attacker.ProgressionService.InitializeStartingAttacks(new List<AttackSO>(wheels._startAttacks));
            }
        }
        else
        {

        }

        if (player.TryGetComponent(out HealthComponent health))
        {
            int hp = _carHP.Count > index ? _carHP[index] : 100;
            health.MaxHealth = hp;
            health.CurrentHealth = hp;
        }
    }
}

[System.Serializable]
public class WheelsConfig
{
    public MovementMode mode;
    public AudioClip wheelsTheme;
    public AttackSO[] _startAttacks;
}
