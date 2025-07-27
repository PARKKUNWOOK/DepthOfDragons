using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class QuarterViewCamera : MonoBehaviour
{
    private Transform _target; // 따라갈 대상
    private Vector3 _offset = new Vector3(0, 15f, -10f); // 위에서 바라보는 쿼터뷰 시점

    public void SetTarget(Transform target)
    {
        _target = target;
        if (_target != null)
        {
            transform.position = _target.position + _offset;
            transform.LookAt(_target);
        }
    }

    private void LateUpdate()
    {
        if (_target == null) return;
        UpdateCameraPosition();
    }

    private void UpdateCameraPosition()
    {
        transform.position = _target.position + _offset;
        transform.LookAt(_target);
    }
}
