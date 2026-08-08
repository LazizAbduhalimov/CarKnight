using UnityEngine;

public class HealerWrench : MonoBehaviour
{
    [SerializeField] private int _healAmount;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerUnit player))
        {
            HealthComponent health = player.GetComponent<HealthComponent>();
            health.Heal(_healAmount);
            TakeObjectAnim();
            gameObject.SetActive(false);
        }
    }


    private void TakeObjectAnim()
    {

    }
}


