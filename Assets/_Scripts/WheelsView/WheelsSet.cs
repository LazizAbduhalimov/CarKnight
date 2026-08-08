using UnityEngine;

public class WheelsSet : MonoBehaviour
{
    [SerializeField] private GameObject _wheelSet;

    [SerializeField] private GameObject _turnRight;
    [SerializeField] private GameObject _turnLeft;
    [SerializeField] private GameObject _turnDefault;


    private int _currentState = 0;

    public void Turn(float turn)
    {

        if (_turnRight == null || _turnLeft == null || _wheelSet == null)
        {
            return;
        }
        int newState = 0;
        if (turn > 0.01f) newState = 1;
        else if (turn < -0.01f) newState = 2;

        if (newState == _currentState) return;

        _currentState = newState;

        _turnRight.SetActive(newState == 1);
        _turnLeft.SetActive(newState == 2);
        _turnDefault.SetActive(newState == 0);
    }
}
