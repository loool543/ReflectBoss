using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _followTarget;
    [SerializeField] private Vector3 _positionOffset = new Vector3(0f, 5f, -2.5f);
    [SerializeField, Min(0.01f)] private float _followSmoothTime = 0.2f;

    private Vector3 _followVelocity;

    private void Reset()
    {
        // Set the initial view toward Player; following never changes this rotation.
        Player player = FindFirstObjectByType<Player>();
        if (player == null)
            return;

        _followTarget = player.transform;
        transform.SetPositionAndRotation(_followTarget.position + _positionOffset,
            Quaternion.LookRotation(-_positionOffset, Vector3.up));
    }

    private void OnEnable()
    {
        _followVelocity = Vector3.zero;
    }

    private void LateUpdate()
    {
        if (_followTarget == null)
            return;

        // World-space offset and position-only follow keep camera-relative movement stable.
        Vector3 targetPosition = _followTarget.position + _positionOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
            ref _followVelocity, _followSmoothTime);
    }
}
