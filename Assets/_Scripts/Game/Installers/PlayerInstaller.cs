using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class PlayerInstaller : MonoInstaller
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private CameraBox _cameraBox;
    [SerializeField] private PlayerUnit _playerPrefab;
    [SerializeField] private XPView _xpView;
    [Header("Attack Combo")]
    [SerializeField] private AttackComboTableSO _attackComboTable;

    [Header("Enemy Teleport System")]
    [SerializeField] private bool _enableEnemyTeleportSystem = true;
    [SerializeField, MinValue(1f)] private float _enemyMaxDistanceFromHero = 120f;
    [SerializeField, MinValue(0.1f)] private float _enemyTeleportDistanceAheadOfHero = 25f;
    [SerializeField, MinValue(0.05f)] private float _enemyTeleportCheckInterval = 0.35f;
    [SerializeField, MinValue(0.05f)] private float _enemyListRefreshInterval = 1f;
    [SerializeField, MinValue(0f)] private float _enemyTeleportSpreadRadius = 4f;
    [SerializeField, Range(0f, 89f)] private float _enemyTeleportSpreadAngleDegrees = 15f;
    [SerializeField, MinValue(0.1f)] private float _enemyTeleportNavMeshSampleRadius = 12f;
    [SerializeField, MinValue(1)] private int _enemyTeleportCandidateAttempts = 8;

    public override void InstallBindings()
    {
        Debug.Log("Installing Player and XP System");
        BuildPlayerAndAssignToCamera();
        BindAttackComboSystem();
        BindXP();
        BindEnemyTeleportSystem();
    }

    private void BuildPlayerAndAssignToCamera()
    {
        var player = Container.InstantiatePrefabForComponent<PlayerUnit>(_playerPrefab, _spawnPoint.position, _spawnPoint.rotation, null);
        var attacker = player.GetComponent<Attacker>();
        if (attacker == null)
            Debug.LogError("Player prefab does not contain Attacker component.", _playerPrefab);
        var movement = player.GetComponent<MovementFsmController>();

        Container.Bind<MovementFsmController>().FromInstance(movement).AsSingle().NonLazy();
        Container.Bind<PlayerUnit>().FromInstance(player).AsSingle().NonLazy();
        Container.Bind<Attacker>().FromInstance(attacker).AsSingle().NonLazy();
        Container.Bind<IAttackProgressionService>().FromInstance(attacker.ProgressionService).AsSingle().NonLazy();
        Container.Bind<HealthComponent>().FromInstance(player.HealthComponent).AsSingle().NonLazy();

        Container.Bind<CameraBox>().FromInstance(_cameraBox).AsSingle().NonLazy();
        _camera.Follow = player.transform;
    }

    private void BindXP()
    {
        var playerConfig = Container.Resolve<PlayerUnit>()?.Config;
        Container.Bind<XPView>().FromInstance(_xpView).AsSingle().NonLazy();
        Container.Bind<XPModel>().AsSingle().WithArguments(0, 1, 30, playerConfig);
        Container.Bind<XPController>().AsSingle().NonLazy();
        Container.Bind<XPGainer>().AsSingle().NonLazy();
    }

    private void BindAttackComboSystem()
    {
        if (_attackComboTable == null)
        {
            return;
        }

        Container.Bind<AttackComboTableSO>().FromInstance(_attackComboTable).AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<AttackComboCoordinator>().AsSingle().NonLazy();
    }

    private void BindEnemyTeleportSystem()
    {
        if (!_enableEnemyTeleportSystem)
            return;

        var settings = new EnemyTeleportSettings(
            _enemyMaxDistanceFromHero,
            _enemyTeleportDistanceAheadOfHero,
            _enemyTeleportCheckInterval,
            _enemyListRefreshInterval,
            _enemyTeleportSpreadRadius,
            _enemyTeleportSpreadAngleDegrees,
            _enemyTeleportNavMeshSampleRadius,
            _enemyTeleportCandidateAttempts);

        Container.Bind<EnemyTeleportSettings>().FromInstance(settings).AsSingle();
        Container.BindInterfacesAndSelfTo<EnemyTeleportSystem>().AsSingle().NonLazy();
    }
}
