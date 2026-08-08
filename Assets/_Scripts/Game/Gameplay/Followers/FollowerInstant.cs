using UnityEngine;

public class FollowerInstant : MonoBehaviour
{
    [SerializeField] public Transform _target;

    void LateUpdate()
    {
        Move();
    }

    public void Move()
    {
        transform.position = _target.position;
    }

    public void SetTarget(Transform target)
    {
        _target = target;
    }
}
