using System;
using UnityEngine;
using UnityEngine.UI;

public class LocksTransport : MonoBehaviour
{
    [SerializeField] public MovementMode movement;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _win;
    private Button _button;

    public void SetState(bool state)
    {
        if(_button == null) _button = GetComponent<Button>();

        _lock.SetActive(state);
        _button.interactable = !state;
    }

    internal void MarkAsWinned(bool state)
    {
        _win.SetActive(state);
    }
}
