using UnityEngine;
using Zenject;

public class TaskZenject : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<GameEvents>().FromNew().AsSingle();
        Container.Bind<TaskPanelUI>().FromComponentInHierarchy().AsSingle();
    }
}
