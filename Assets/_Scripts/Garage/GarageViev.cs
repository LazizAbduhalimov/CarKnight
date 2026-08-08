using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarageViev : MonoBehaviour
{
    [SerializeField] List<EachCarViev> _cars = new List<EachCarViev>();

    [Header("Arm Viev")]
    [SerializeField] private Animator _armAnimator;

    [Header("Other settings")]
    [SerializeField] private GameObject _buttons;
    [SerializeField] private GameObject _startButton;
    private EachCarViev _activeCar;

    public static int carLvlId;
    public static MovementMode currentMove;

    public bool _isChanging;

    private void Start()
    {
        carLvlId = -1;
    }

    public void ActivateCar(int carId)
    {
        if (carId != carLvlId && !_isChanging)
        {
            StartCoroutine(CarChangeSequence(carId));
        }
    }


    IEnumerator CarChangeSequence(int carId)
    {
        _isChanging = true;
        _buttons.SetActive(false); 

        _armAnimator.SetTrigger("Down");

        yield return new WaitUntil(() => _armAnimator.GetCurrentAnimatorStateInfo(0).IsName("ArmDown"));
        while (_armAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        ApplyCarChange(carId);

        _armAnimator.SetTrigger("Up");

        yield return new WaitUntil(() => _armAnimator.GetCurrentAnimatorStateInfo(0).IsName("ArmUp"));
        while (_armAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
        {
            yield return null;
        }

        _buttons.SetActive(true);
        _startButton.SetActive(true);
        _isChanging = false;
    }

    private void ApplyCarChange(int carId)
    {
        int index = carId - 1;
        if (index >= 0 && index < _cars.Count)
        {
            carLvlId = carId;
            _activeCar?.Deactivate();
            _activeCar = _cars[index];
            _activeCar.Activate();
            currentMove = _activeCar.baseMovement;
        }
    }


    public void ChooseHover() => ApplyMode(MovementMode.HoverCar);
    public void ChooseLevitating() => ApplyMode(MovementMode.LevitatingCar);
    public void ChooseRacer() => ApplyMode(MovementMode.RacingCar);
    public void ChooseTank() => ApplyMode(MovementMode.TankStyle);
    public void ChooseBasic() => ApplyMode(MovementMode.WheelCar);

    private void ApplyMode(MovementMode mode)
    {
        if (_activeCar != null && currentMove != mode)
        {
            StartCoroutine(ModeChangeSequence(mode));
        }
    }

    IEnumerator ModeChangeSequence(MovementMode mode)
    {
        if (_activeCar == null || _isChanging) yield break;

        _isChanging = true;
        _buttons.SetActive(false);

        _armAnimator.SetTrigger("Down");
        yield return new WaitUntil(() => _armAnimator.GetCurrentAnimatorStateInfo(0).IsName("ArmDown"));
        while (_armAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;

        _activeCar.SetMovementMode(mode);
        currentMove = mode;

        _buttons.SetActive(true);

        _armAnimator.SetTrigger("Up");
        yield return new WaitUntil(() => _armAnimator.GetCurrentAnimatorStateInfo(0).IsName("ArmUp"));
        while (_armAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f) yield return null;

        _isChanging = false;

    }
}