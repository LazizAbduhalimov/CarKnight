using Sirenix.OdinInspector;
using UnityEngine;

public class SpawnTester : MonoBehaviour
{
    private SpawnSystem spawnSystem;

    private void Start()
    {
        spawnSystem = GetComponent<SpawnSystem>();
    }

    [Button]
    public void LvlUp()
    {
        spawnSystem.LvlUp();
    }

    [Button]
    public void countUpgrade(int howMuch)
    {
        spawnSystem.SpawnCountUpgrader(howMuch);
    }
}
