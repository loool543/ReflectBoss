using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public enum BossStage { Stage1, Stage2 }

    [SerializeField] private Transform _releasePoint;
    [SerializeField] private Player _player;
    [SerializeField, Min(0.01f)] private float _attackInterval = 1.5f;
    [SerializeField, Range(0f, 1f)] private float _basketBallChance = 0.5f;
    [SerializeField, Min(1)] private int _maxHP = 200;
    [SerializeField, Range(0f, 180f)] private float _stage2SpreadAngle = 30f;

    private Collider _bodyCollider;
    private Coroutine _attackCoroutine;
    private int _currentHP;

    public int CurrentHP => _currentHP;
    public int MaxHP => _maxHP;
    public BossStage CurrentStage { get; private set; }

    public Vector3 HitPosition => _bodyCollider != null ? _bodyCollider.bounds.center : transform.position;

    private void Awake()
    {
        _bodyCollider = GetComponent<Collider>();
        if (_releasePoint == null)
            _releasePoint = transform.Find("ReleasePoint");
        if (_player == null)
            _player = FindFirstObjectByType<Player>();
    }

    private void OnEnable()
    {
        _currentHP = _maxHP;
        CurrentStage = BossStage.Stage1;
        EventManager.Instance.TriggerEvent(Define.EEventType.BossHPChanged);
        _attackCoroutine = StartCoroutine(CoAttack());
    }

    private void OnDisable()
    {
        if (_attackCoroutine == null)
            return;

        StopCoroutine(_attackCoroutine);
        _attackCoroutine = null;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || _currentHP <= 0)
            return;

        _currentHP = Mathf.Max(0, _currentHP - damage);
        CheckStageTransition();
        Debug.Log($"Boss Hit - Current HP : {_currentHP}", this);
        EventManager.Instance.TriggerEvent(Define.EEventType.BossHit);
        EventManager.Instance.TriggerEvent(Define.EEventType.BossHPChanged);
    }

    private void CheckStageTransition()
    {
        if (CurrentStage != BossStage.Stage1 || _currentHP > _maxHP * 0.5f)
            return;

        CurrentStage = BossStage.Stage2;
        Debug.Log("Boss Stage2 Started", this);
    }

    private IEnumerator CoAttack()
    {
        if (_releasePoint == null || _player == null)
        {
            Debug.LogError("Boss: ReleasePoint and Player are required.", this);
            yield break;
        }

        // Always wait before firing so scene resource initialization can finish.
        // HP reaching zero intentionally does not stop attacks at this stage.
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.01f, _attackInterval));
            if (_player == null || _releasePoint == null)
                continue;

            Collider targetCollider = _player.GetComponent<Collider>();
            Vector3 targetPosition = targetCollider != null ? targetCollider.bounds.center : _player.transform.position;
            Vector3 direction = (targetPosition - _releasePoint.position).normalized;
            if (CurrentStage == BossStage.Stage2)
                FireStage2Projectiles(direction);
            else
                FireProjectile(direction, false);
        }
    }

    private void FireStage2Projectiles(Vector3 direction)
    {
        FireProjectile(direction, true);
        FireProjectile(Quaternion.AngleAxis(-_stage2SpreadAngle, Vector3.up) * direction, true);
        FireProjectile(Quaternion.AngleAxis(_stage2SpreadAngle, Vector3.up) * direction, true);
    }

    private void FireProjectile(Vector3 direction, bool stage2)
    {
        if (_player == null || _releasePoint == null)
            return;

        bool basketBall = _basketBallChance >= 1f || Random.value < _basketBallChance;
        string prefabName = basketBall ? "BasketBall" : "Ball";
        GameObject prefab = ResourceManager.Instance.Get<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"Boss: Resource '{prefabName}' is not loaded.", this);
            return;
        }

        // The existing Instantiate API resolves the same cached prefab through Get.
        GameObject instance = ResourceManager.Instance.Instantiate(prefabName);
        instance.transform.position = _releasePoint.position;
        BossProjectile projectile = instance.GetComponent<BossProjectile>();
        if (projectile == null)
        {
            Debug.LogError($"Boss: '{prefabName}' needs a BossProjectile component.", instance);
            ResourceManager.Instance.Destroy(instance);
            return;
        }

        projectile.Launch(this, _player, direction, stage2);
    }
}
