using Zenject;

public sealed class AttackSystemServicesInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IAttackProgressionServiceFactory>().To<AttackProgressionServiceFactory>().AsSingle().NonLazy();
    }
}
