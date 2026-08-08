using UnityEngine;
using Zenject;

public sealed class DamagesProInstaller : MonoInstaller
{
    [SerializeField] private DamagesProContainer _containerPrefab;

    public override void InstallBindings()
    {
        if (_containerPrefab == null)
        {
            Debug.LogError("DamagesProContainer prefab is not assigned in DamagesProInstaller.", this);
            return;
        }

        Container.Bind<DamagesProContainer>()
            .FromComponentInNewPrefab(_containerPrefab)
            .AsSingle()
            .NonLazy();

        Container.Bind<IDamageNumberService>().To<DamageNumberService>().AsSingle();
    }
}
