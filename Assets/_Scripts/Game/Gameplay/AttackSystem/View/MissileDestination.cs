using UnityEngine;

public class MissileDestination : MonoBehaviour
{
    public float Diameter;

    [SerializeField] private Transform FullRadiusSprite;
    [SerializeField] private Transform IncreaseingRadiusSprite;
    [SerializeField][Range(0f, 1f)] private float fillAmount = 1f; // реактивщина нужна

    public void OnValidate()
    {
        transform.localScale = Vector3.one;
        if (FullRadiusSprite != null)
        {
            FullRadiusSprite.localScale = new Vector3(Diameter, Diameter, Diameter);
        }
        SetFillAmount(fillAmount);
    }

    public void SetFillAmount(float amount)
    {
        fillAmount = amount;
        var scaledDiameter = Diameter * amount;
        IncreaseingRadiusSprite.localScale = new Vector3(scaledDiameter, scaledDiameter, scaledDiameter);
    }

    public void SetDiameter(float diameter)
    {
        Diameter = diameter;
        OnValidate();
    }
}