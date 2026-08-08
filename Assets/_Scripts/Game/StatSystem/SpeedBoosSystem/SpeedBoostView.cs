using UnityEngine;
using UnityEngine.UI;

public class SpeedBoostView : MonoBehaviour
{
    [SerializeField] private Image _speedBoostBarImage; 

    public void UpdateSpeedBoost(float currentSpeedBoost, float maxSpeedBoost)
    {
        _speedBoostBarImage.fillAmount = currentSpeedBoost / maxSpeedBoost;
    }
}