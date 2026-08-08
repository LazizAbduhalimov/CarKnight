using PoolSystem.Alternative;
using UnityEngine;

// пока временно
public class VFXPools : MonoBehaviour
{
    public PoolContainer FireImpactPool;
    public PoolContainer ExplosionImpactPool;
    public PoolContainer[] ExplosionForMissilesPool;
    
    public PoolContainer MissileDestinationPool;
    public PoolContainer PlayerMissileDestinationPool;
    public PoolContainer ZapBluePool;
    public PoolContainer ZapRedPool;
    public PoolContainer ZapYellowPool;
    public PoolContainer MachineGunMuzzleFlashPool;
    public PoolContainer MachineGunTracerPool;
    public PoolContainer MachineGunImpactPool;

    public static VFXPools Instance => _instance;
    private static VFXPools _instance;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
