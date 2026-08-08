using PoolSystem.Alternative;
using UnityEngine;

public class PoolServiceSingletone : MonoBehaviour
{
    public static PoolServiceSingletone Instance { get; private set; }
    public PoolService PoolService { get; private set; }

    private void Awake()
    {
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        //     return;
        // }
        // PoolService = new PoolService("GlobalPoolService");
        // Instance = this;
        // DontDestroyOnLoad(gameObject);
    }
}