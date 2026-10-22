using UnityEngine;
using UnityEngine.Rendering;

/// <summary>World-space presentation for the existing single-player ReflectSuccess event.</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Player))]
public sealed class PlayerReflectFeedback : MonoBehaviour
{
    [SerializeField] private Material _ringMaterial;
    [SerializeField] private Vector3 _localOffset = new Vector3(0f, 0.08f, 0f);
    [SerializeField] private Color _ringColor = new Color(0.3f, 1f, 0.85f, 0.85f);
    [SerializeField, Min(0.01f)] private float _duration = 0.24f;
    [SerializeField, Min(0.01f)] private float _radius = 0.45f;
    [SerializeField, Range(0.01f, 0.3f)] private float _relativeWidth = 0.07f;
    [SerializeField, Range(1f, 2f)] private float _expansion = 1.35f;

    private EventManager _events;
    private ParticleSystem _ring;
    private Mesh _ownedMesh;

    private void Awake()
    {
        if (_ringMaterial == null)
        {
            Debug.LogError("PlayerReflectFeedback requires a transparent particle material.", this);
            enabled = false;
            return;
        }

        // Allocate the object and geometry once. Reflection only restarts one particle.
        var effect = new GameObject("ReflectRing", typeof(ParticleSystem));
        effect.layer = gameObject.layer;
        effect.transform.SetParent(transform, false);
        _ring = effect.GetComponent<ParticleSystem>();
        _ring.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = _ring.main;
        main.playOnAwake = false;
        main.loop = false;
        main.duration = Mathf.Max(0.01f, _duration);
        main.startLifetime = Mathf.Max(0.01f, _duration);
        main.startSpeed = 0f;
        main.startSize = Mathf.Max(0.01f, _radius) * 2f;
        main.startColor = _ringColor;
        main.startRotation3D = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.scalingMode = ParticleSystemScalingMode.Shape;
        main.useUnscaledTime = true;
        main.maxParticles = 1;
        main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;

        var emission = _ring.emission;
        emission.enabled = false;
        var shape = _ring.shape;
        shape.enabled = false;
        var size = _ring.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f,
            AnimationCurve.Linear(0f, 0.55f, 1f, _expansion));
        var color = _ring.colorOverLifetime;
        color.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.15f),
                new GradientAlphaKey(0f, 1f) });
        color.color = fade;

        _ownedMesh = CreateRingMesh(Mathf.Clamp(_relativeWidth, 0.01f, 0.3f));
        var particleRenderer = effect.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
        particleRenderer.alignment = ParticleSystemRenderSpace.World;
        particleRenderer.mesh = _ownedMesh;
        particleRenderer.sharedMaterial = _ringMaterial;
        particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
        particleRenderer.receiveShadows = false;
        particleRenderer.lightProbeUsage = LightProbeUsage.Off;
        particleRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
    }

    private void OnEnable()
    {
        if (_ring == null) return;
        _events = EventManager.Instance;
        _events.AddEvent(Define.EEventType.ReflectSuccess, PlayFeedback);
        _events.AddEvent(Define.EEventType.GameStateChanged, OnGameStateChanged);
    }

    private void OnDisable()
    {
        if (_events != null)
        {
            _events.RemoveEvent(Define.EEventType.ReflectSuccess, PlayFeedback);
            _events.RemoveEvent(Define.EEventType.GameStateChanged, OnGameStateChanged);
        }
        StopFeedback();
    }

    private void OnDestroy()
    {
        if (_ring != null) Destroy(_ring.gameObject);
        if (_ownedMesh != null) Destroy(_ownedMesh);
    }

    [ContextMenu("Preview Reflect Ring (Play Mode)")]
    public void PlayFeedback()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || _ring == null ||
            GameManager.Instance.CurrentState != Define.EGameState.Playing)
            return;

        // World position is captured at success; a subsequent hit replaces the previous ring.
        _ring.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _ring.Play(false);
        var particle = new ParticleSystem.EmitParams
        {
            position = transform.TransformPoint(_localOffset),
            velocity = Vector3.zero,
            rotation3D = Vector3.zero
        };
        _ring.Emit(particle, 1);
    }

    private void OnGameStateChanged()
    {
        if (GameManager.Instance.CurrentState != Define.EGameState.Playing)
            StopFeedback();
    }

    private void StopFeedback()
    {
        if (_ring != null)
            _ring.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private static Mesh CreateRingMesh(float relativeWidth)
    {
        const int segments = 48;
        var vertices = new Vector3[segments * 2];
        var triangles = new int[segments * 6];
        var uv = new Vector2[vertices.Length];
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            vertices[i * 2] = direction * 0.5f;
            vertices[i * 2 + 1] = direction * (0.5f * (1f - relativeWidth));
            uv[i * 2] = new Vector2(0.5f, 0.5f);
            uv[i * 2 + 1] = uv[i * 2];
            int next = ((i + 1) % segments) * 2;
            int t = i * 6;
            triangles[t] = i * 2;
            triangles[t + 1] = next;
            triangles[t + 2] = i * 2 + 1;
            triangles[t + 3] = i * 2 + 1;
            triangles[t + 4] = next;
            triangles[t + 5] = next + 1;
        }
        var mesh = new Mesh { name = "ReflectRing" };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
