using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Tooltip("Скорость вращения в градусах в секунду")]
    public float speed = 90f;

    [Tooltip("Ось вращения (например, (0,1,0) для Y)")]
    public Vector3 axis = Vector3.up;

    void Update()
    {
        transform.Rotate(axis.normalized, speed * Time.unscaledDeltaTime);
    }
}