using UnityEngine;
using Zenject;
using PoolSystem.Alternative;

public class PoolServiceInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Debug.Log("Installing PoolService as Singleton");
        Container.Bind<IPoolMonoFactory>().To<PoolMonoInjectedFactory>().AsSingle().NonLazy();
        Container.Bind<PoolService>().AsSingle().WithArguments("Pool").NonLazy();
    }
}