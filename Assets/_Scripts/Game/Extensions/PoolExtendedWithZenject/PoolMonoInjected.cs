using PoolSystem.Alternative;
using UnityEngine;
using Zenject;

public class PoolMonoInjected<T> : PoolMono<T> where T : MonoBehaviour
{
    private readonly DiContainer _diContainer;

    public PoolMonoInjected(
        DiContainer diContainer,
        T prefab, int count = 0, Transform container = null, bool autoExpand = true) : 
        base(prefab, 0, container, autoExpand)
    {
        _diContainer = diContainer;
        CreatePool(count);
    }

    public override T CreateObject(bool isActivateByDefault = false)
    {
        var createdObjcet = _diContainer.InstantiatePrefab(Prefab, Container);
        var type = createdObjcet.GetComponent<T>();
        createdObjcet.gameObject.SetActive(isActivateByDefault);
        Pool.Add(type);
        return type;
    }
}

public class PoolMonoInjectedFactory: IPoolMonoFactory 
{
    private DiContainer _diContainer;
    public PoolMonoInjectedFactory(DiContainer diContainer)
    {
        _diContainer = diContainer;
    }

    public IPool<T> Create<T>(T prefab, int count, Transform container = null, bool autoExpand = true) where T : MonoBehaviour
    {
        return new PoolMonoInjected<T>(_diContainer, prefab, count, container, autoExpand);
    }
}