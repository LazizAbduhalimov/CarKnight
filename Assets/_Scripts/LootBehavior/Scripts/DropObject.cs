using System;
using UnityEngine;
using Zenject;

public interface IDrop
{
    public void OnDropped(GameEvents events);
    public void OnPickedUp();
}

public class DropObject : MonoBehaviour, IDrop
{
    private GameEvents gameEvents;

    public void OnDropped(GameEvents events)
    {
    }
    public void OnPickedUp()
    {

    }
}
