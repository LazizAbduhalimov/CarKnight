using UnityEngine;
using Zenject;

public class ExpBall : MonoBehaviour, IDrop
{
    [SerializeField] private int _expAmount;

    public GameEvents _gameEvents;


    public void OnDropped(GameEvents events)
    {
        _gameEvents = events;
    }

    public void OnPickedUp()
    {
        _gameEvents.OnExpCollected(_expAmount);

        gameObject.SetActive(false);

        gameObject.SetActive(false);
    }
}
