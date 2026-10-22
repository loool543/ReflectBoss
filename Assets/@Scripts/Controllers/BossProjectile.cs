using UnityEngine;

public enum ProjectileType
{
    BasketBall,
    Ball,
}

[RequireComponent(typeof(SphereCollider))]
public class BossProjectile : MonoBehaviour
{
    [SerializeField] private ProjectileType _projectileType;
    [SerializeField, Min(0.01f)] private float _speed = 11f;
    [SerializeField, Min(0.01f)] private float _lifeTime = 10f;
    [SerializeField] private LayerMask _envLayerMask;

    public ProjectileType Type => _projectileType;
    public Vector3 Direction { get; private set; }
    // Gameplay Reflect by Player Block only; Env bounces never change this value.
    public bool IsReflected { get; private set; }

    private const int Damage = 10;
    private SphereCollider _sphere;
    private Boss _boss;
    private Player _player;
    private float _elapsedTime;
    private bool _launched;
    private bool _finished;
    private bool _stage2;
    private bool _hasLeftBoss;
    private const float WallSkin = 0.001f;
    private const int InitialQueryCapacity = 16;
    // Each projectile owns its buffers; only capacity growth allocates after creation.
    private Collider[] _overlapBuffer = new Collider[InitialQueryCapacity];
    private RaycastHit[] _sweepBuffer = new RaycastHit[InitialQueryCapacity];

    private void Awake()
    {
        _sphere = GetComponent<SphereCollider>();
        // Queries handle hits; the moving sphere must not physically push CharacterController.
        _sphere.isTrigger = true;
        if (_envLayerMask.value == 0)
            _envLayerMask = LayerMask.GetMask("Env");
    }

    public void Launch(Boss boss, Player player)
    {
        _boss = boss;
        _player = player;
        Collider targetCollider = player.GetComponent<Collider>();
        Vector3 targetPosition = targetCollider != null ? targetCollider.bounds.center : player.transform.position;
        Launch(boss, player, targetPosition - transform.position, false);
    }

    public void Launch(Boss boss, Player player, Vector3 direction, bool stage2)
    {
        _boss = boss;
        _player = player;
        Direction = direction.normalized;
        _stage2 = stage2;
        IsReflected = false;
        _hasLeftBoss = false;
        _elapsedTime = 0f;
        _finished = false;
        _launched = true;
    }

    private void LateUpdate()
    {
        if (!_launched || _finished || Time.deltaTime <= 0f)
            return;

        _elapsedTime += Time.deltaTime;
        if (!_stage2 && _elapsedTime >= _lifeTime)
        {
            Remove();
            return;
        }

        // Player moves in Update. Synchronize its existing colliders before querying them.
        Physics.SyncTransforms();
        Vector3 center = transform.TransformPoint(_sphere.center);
        Vector3 scale = transform.lossyScale;
        float radius = _sphere.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));

        // Handle initial overlaps explicitly before sweeping the movement path.
        int overlapCount = QueryOverlaps(center, radius, ~0);
        if (!_hasLeftBoss)
        {
            bool overlapsBoss = false;
            for (int i = 0; i < overlapCount; i++)
                overlapsBoss |= IsBoss(_overlapBuffer[i]);
            _hasLeftBoss = !overlapsBoss;
        }

        for (int i = 0; i < overlapCount; i++)
        {
            Collider overlap = _overlapBuffer[i];
            if (IsTarget(overlap))
            {
                HandleHit(overlap);
                return;
            }
        }

        // Recover a sphere spawned inside a wall before sweeping. Never use the
        // synthetic normal returned by a SphereCast starting inside a collider.
        if (_stage2)
        {
            // The target overlap results are no longer needed, so reuse the same buffer.
            int wallCount = QueryOverlaps(center, radius, _envLayerMask);
            for (int i = 0; i < wallCount; i++)
            {
                Collider wall = _overlapBuffer[i];
                if (!IsWall(wall) || !Physics.ComputePenetration(_sphere, transform.position,
                    transform.rotation, wall, wall.transform.position, wall.transform.rotation,
                    out Vector3 normal, out float depth))
                    continue;

                transform.position += normal * (depth + WallSkin);
                if (Vector3.Dot(Direction, normal) < 0f)
                {
                    Direction = Vector3.Reflect(Direction, normal).normalized;
                }
                center = transform.TransformPoint(_sphere.center);
            }
        }

        float distance = _speed * Time.deltaTime;
        // Consume the remaining travel after each bounce. Bound iterations for
        // degenerate corners without moving through a wall or deleting the ball.
        for (int bounce = 0; bounce < 8 && distance > 0f; bounce++)
        {
            RaycastHit nearestHit = default;
            float nearestDistance = float.PositiveInfinity;
            int hitCount = QuerySweep(center, radius, distance);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit hit = _sweepBuffer[i];
                bool target = IsTarget(hit.collider);
                bool wall = IsWall(hit.collider) && Vector3.Dot(Direction, hit.normal) < -0.0001f;
                if ((target || wall) && hit.distance < nearestDistance)
                {
                    nearestHit = hit;
                    nearestDistance = hit.distance;
                }
            }

            if (float.IsPositiveInfinity(nearestDistance))
            {
                transform.position += Direction * distance;
                return;
            }

            transform.position += Direction * nearestDistance;
            if (IsTarget(nearestHit.collider))
            {
                HandleHit(nearestHit.collider);
                return;
            }

            distance -= nearestDistance;
            Direction = Vector3.Reflect(Direction, nearestHit.normal).normalized;
            transform.position += nearestHit.normal * WallSkin;
            center = transform.TransformPoint(_sphere.center);
        }
    }

    private int QueryOverlaps(Vector3 center, float radius, int layerMask)
    {
        while (true)
        {
            int count = Physics.OverlapSphereNonAlloc(center, radius, _overlapBuffer,
                layerMask, QueryTriggerInteraction.Collide);
            if (count < _overlapBuffer.Length)
                return count;

            // A full buffer may be truncated. Grow and repeat before processing any hits.
            _overlapBuffer = new Collider[_overlapBuffer.Length * 2];
        }
    }

    private int QuerySweep(Vector3 center, float radius, float distance)
    {
        while (true)
        {
            int count = Physics.SphereCastNonAlloc(center, radius, Direction, _sweepBuffer,
                distance, ~0, QueryTriggerInteraction.Collide);
            if (count < _sweepBuffer.Length)
                return count;

            // A truncated cast is not guaranteed to contain the nearest hit.
            _sweepBuffer = new RaycastHit[_sweepBuffer.Length * 2];
        }
    }

    private bool IsWall(Collider other)
    {
        return _stage2 && other != _sphere &&
            (_envLayerMask.value & (1 << other.gameObject.layer)) != 0 &&
            other.GetComponentInParent<BossProjectile>() == null &&
            other.GetComponentInParent<Player>() == null &&
            other.GetComponentInParent<Boss>() == null;
    }

    private bool IsTarget(Collider other)
    {
        // Ignore only the initial overlap with the firing Boss, not a later return.
        if (IsBoss(other))
            return IsReflected || _hasLeftBoss;

        return !IsReflected && _player != null && other.GetComponentInParent<Player>() == _player;
    }

    private bool IsBoss(Collider other)
    {
        return _boss != null && other.GetComponentInParent<Boss>() == _boss;
    }

    private void HandleHit(Collider other)
    {
        if (_finished)
            return;

        if (IsBoss(other))
        {
            Remove();
            if (_projectileType == ProjectileType.BasketBall && IsReflected && _boss != null)
                _boss.TakeDamage(Damage);
            return;
        }

        if (_projectileType == ProjectileType.BasketBall && _player.IsBlocking)
        {
            if (_boss == null)
            {
                Remove();
                return;
            }

            IsReflected = true;
            Direction = (_boss.HitPosition - transform.position).normalized;
            Debug.Log("Reflect Success", this);
            EventManager.Instance.TriggerEvent(Define.EEventType.ReflectSuccess);
            SoundManager.Instance.Play2D(Define.ESound.Reflect, "reflect");
            return;
        }

        // Mark consumed before notifying listeners, including when Player has multiple colliders.
        Remove();
        SoundManager.Instance.Play2D(Define.ESound.Collision, "collision");
        _player.TakeDamage(Damage);
    }

    private void Remove()
    {
        if (_finished)
            return;

        _finished = true;
        _sphere.enabled = false;
        ResourceManager.Instance.Destroy(gameObject);
    }
}
