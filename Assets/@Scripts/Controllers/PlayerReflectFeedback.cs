using System.Collections;
using UnityEngine;

/// <summary>반사 성공 시 플레이어 발밑에 퍼지는 링 연출.</summary>
[RequireComponent(typeof(Player))]
public class PlayerReflectFeedback : MonoBehaviour
{
    [SerializeField] private Material _ringMaterial;
    [SerializeField] private Vector3 _localOffset = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private Color _ringColor = new Color(0.3f, 1f, 0.85f, 0.85f);
    [SerializeField, Min(0.01f)] private float _duration = 0.24f;
    [SerializeField, Min(0.01f)] private float _radius = 0.45f;
    [SerializeField, Range(1f, 2f)] private float _expansion = 1.35f;
    [SerializeField, Min(0.005f)] private float _lineWidth = 0.04f;

    private const int Segments = 48;
    private LineRenderer _line;
    private Coroutine _routine;
    private EventManager _events;

    private void Awake()
    {
        var go = new GameObject("ReflectRing");
        go.layer = gameObject.layer;
        go.transform.SetParent(transform, false);
        _line = go.AddComponent<LineRenderer>();
        _line.loop = true;
        _line.useWorldSpace = true;
        _line.positionCount = Segments;
        _line.startWidth = _line.endWidth = _lineWidth;
        _line.sharedMaterial = _ringMaterial;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;
        _line.enabled = false;
    }

    private void OnEnable()
    {
        _events = EventManager.Instance;
        _events.AddEvent(Define.EEventType.ReflectSuccess, Play);
        _events.AddEvent(Define.EEventType.GameStateChanged, Stop);
    }

    private void OnDisable()
    {
        if (_events != null)
        {
            _events.RemoveEvent(Define.EEventType.ReflectSuccess, Play);
            _events.RemoveEvent(Define.EEventType.GameStateChanged, Stop);
        }
        Stop();
    }

    private void OnDestroy()
    {
        if (_line != null) Destroy(_line.gameObject);
    }

    [ContextMenu("Preview Reflect Ring (Play Mode)")]
    private void Play()
    {
        if (!Application.isPlaying || !isActiveAndEnabled ||
            GameManager.Instance.CurrentState != Define.EGameState.Playing)
            return;

        // 연속 반사 시 이전 링을 끊고 새로 시작한다.
        Stop();
        _routine = StartCoroutine(CoRing());
    }

    private void Stop()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        if (_line != null) _line.enabled = false;
    }

    private IEnumerator CoRing()
    {
        // 부모를 분리하지 않고 정점의 월드 좌표로 반사 순간 위치를 고정한다.
        Vector3 center = transform.TransformPoint(_localOffset);
        float duration = Mathf.Max(0.01f, _duration);
        float elapsed = 0f;
        _line.enabled = true;
        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);
            SetCircle(center, _radius * Mathf.Lerp(0.55f, _expansion, t));
            Color c = _ringColor;
            c.a *= 1f - t;
            _line.startColor = _line.endColor = c;
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }
        _line.enabled = false;
        _routine = null;
    }

    private void SetCircle(Vector3 center, float radius)
    {
        for (int i = 0; i < Segments; i++)
        {
            float angle = i * Mathf.PI * 2f / Segments;
            _line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
        }
    }
}
