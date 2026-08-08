#if UNITY_EDITOR
using UnityEngine;
using Zenject;

public class DeveloperCheetSheet : MonoBehaviour
{
    private XPController _xPController;

    [Inject]
    private void Construct(XPController xPController)
    {
        _xPController = xPController;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Cheat: Adding 15 XP");
            _xPController.AddXP(15);
        }
    }
}
#endif