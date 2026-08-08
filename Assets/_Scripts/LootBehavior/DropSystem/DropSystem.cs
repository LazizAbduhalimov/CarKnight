using Game.Helpers;
using PoolSystem.Alternative;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

public class DropSystem : MonoBehaviour
{
    [SerializeField] private int _eachPoolSize;
    [SerializeField] private bool _eachPoolAutoExpand;
    [SerializeField] private float _dropPositionDif;
    [SerializeField] private float _dropPositionUpper;
    public static DropSystem Instance;
    private PoolService poolService;

    private GameEvents _gameEvents;
    private GameObject _player;
    [Inject]
    public void Construct(GameEvents gameEvents, PlayerUnit player)
    {
        _gameEvents = gameEvents;
        _player = player.gameObject;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        poolService = new PoolService("DropSystem", new PoolMonoFactory());
    }
    public void Drop(DropTable table, Vector3 position)
    {
        foreach (var item in table.drops)
        {
            if (item.dropChance >= Random.Range(0f, 100f) )
            {
                int amount = Random.Range(item.minAmount, item.maxAmount + 1);

                for (int i = 0; i < amount; i++)
                {
                    var pos = position + Random.insideUnitSphere * _dropPositionDif;

                    pos.y = _player.transform.position.y + _dropPositionUpper;

                    var pool = poolService.GetOrRegisterPool<AutoDeactivator>(item.prefab.GetComponent<AutoDeactivator>(), _eachPoolSize, gameObject.transform, _eachPoolAutoExpand);

                    var drop = pool.GetFreeElement(pos, Quaternion.identity, true);

                    if (drop.TryGetComponent(out IDrop idrop))
                    {
                        idrop.OnDropped(_gameEvents);
                    }

                }
            }
        }
    }

  
}
