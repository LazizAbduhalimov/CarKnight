using UnityEngine;
using Zenject;

public class UIMovementSettings : MonoBehaviour
{
    private MovementFsmController _controller;


    [Inject]
    private void Initial(PlayerUnit unit)
    {
        unit.gameObject.TryGetComponent(out MovementFsmController controller);
        _controller = controller;
    }
    public void SwitchToHover() => _controller?.SwitchToHover();
    public void SwitchToLevitating() => _controller?.SwitchToLevitating();
    public void SwitchToRacing() => _controller?.SwitchToRacing();
    public void SwitchToTank() => _controller?.SwitchToTank();  
    public void SwitchToWheelCar() => _controller?.SwitchToWheelCar();
}
