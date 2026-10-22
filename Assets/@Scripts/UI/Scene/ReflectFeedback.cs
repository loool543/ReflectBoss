using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Presentation only: listens to the confirmed ReflectSuccess event.</summary>
[DisallowMultipleComponent]
public sealed class ReflectFeedback : MonoBehaviour
{
    [Header("Optional UI overrides (children of Feedback Root)")]
    [SerializeField] private RectTransform _feedbackRoot;
    [SerializeField] private TMP_Text _label;
    [SerializeField] private Image _icon;
    [SerializeField] private Image _glowImage;

    [Header("Default UI")]
    [SerializeField] private string _message = "REFLECT!";
    [SerializeField] private Vector2 _screenAnchor = new Vector2(0.5f, 0.28f);
    [SerializeField] private Vector2 _offset = Vector2.zero;
    [SerializeField] private Color _textColor = new Color(0.7f, 1f, 0.95f, 1f);

    [Header("Timing (unscaled seconds)")]
    [SerializeField, Min(0.01f)] private float _duration = 0.45f;
    [SerializeField, Min(0f)] private float _holdDuration = 0.12f;
    [SerializeField, Min(0.01f)] private float _glowDuration = 0.16f;
    [SerializeField, Range(1f, 1.5f)] private float _popScale = 1.12f;
    [SerializeField, Range(0f, 1f)] private float _glowIntensity = 0.65f;
    [SerializeField] private Color _glowColor = new Color(0.2f, 1f, 0.85f, 1f);

    private CanvasGroup _group;
    private EventManager _events;
    private Coroutine _routine;
    private FeedbackRoutine _iterator;
    private float _startedAt;
    private Vector3 _baseScale;
    private Texture2D _ownedGlowTexture;
    private Sprite _ownedGlowSprite;

    private void Awake()
    {
        if (!ValidateOverrides())
        {
            enabled = false;
            return;
        }
        CreateDefaultUI();
        _baseScale = _feedbackRoot.localScale;
        _iterator = new FeedbackRoutine(this);
        Hide();
    }

    private void OnEnable()
    {
        if (_iterator == null) return;
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
        if (_ownedGlowSprite != null) Destroy(_ownedGlowSprite);
        if (_ownedGlowTexture != null) Destroy(_ownedGlowTexture);
    }

    private void OnGameStateChanged()
    {
        if (GameManager.Instance.CurrentState != Define.EGameState.Playing)
            StopFeedback();
    }

    [ContextMenu("Preview Reflect (Play Mode)")]
    public void PlayFeedback()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || _group == null ||
            GameManager.Instance.CurrentState != Define.EGameState.Playing)
            return;

        // Repeated hits restart the timeline without stacking coroutines or objects.
        _startedAt = Time.unscaledTime;
        ApplyFrame(0f);
        if (_routine == null)
            _routine = StartCoroutine(_iterator);
    }

    private void StopFeedback()
    {
        if (_routine != null) StopCoroutine(_routine);
        _routine = null;
        Hide();
    }

    private void Hide()
    {
        if (_group == null) return;
        _group.alpha = 0f;
        if (_glowImage != null) _glowImage.canvasRenderer.SetAlpha(0f);
        if (_feedbackRoot != null) _feedbackRoot.localScale = _baseScale;
    }

    private bool Tick()
    {
        float elapsed = Time.unscaledTime - _startedAt;
        float end = Mathf.Max(Mathf.Max(0.01f, _duration), Mathf.Max(0.01f, _glowDuration));
        if (elapsed >= end)
        {
            _routine = null;
            Hide();
            return false;
        }
        ApplyFrame(elapsed);
        return true;
    }

    private void ApplyFrame(float elapsed)
    {
        float duration = Mathf.Max(0.01f, _duration);
        float hold = Mathf.Clamp(_holdDuration, 0f, duration - 0.001f);
        float fade = Mathf.Clamp01((elapsed - hold) / (duration - hold));
        _group.alpha = 1f - Mathf.SmoothStep(0f, 1f, fade);
        float glow = 1f - Mathf.Clamp01(elapsed / Mathf.Max(0.01f, _glowDuration));
        _glowImage.canvasRenderer.SetAlpha(glow * glow * _glowIntensity);
        _feedbackRoot.localScale = _baseScale * Mathf.Lerp(1f, _popScale, glow * glow);
    }

    private void CreateDefaultUI()
    {
        if (_feedbackRoot == null)
        {
            _feedbackRoot = NewRect("ReflectFeedback", transform, new Vector2(180f, 42f));
            _feedbackRoot.anchorMin = _feedbackRoot.anchorMax = _screenAnchor;
            _feedbackRoot.anchoredPosition = _offset;
        }
        _group = _feedbackRoot.GetComponent<CanvasGroup>();
        if (_group == null) _group = _feedbackRoot.gameObject.AddComponent<CanvasGroup>();
        _group.interactable = false;
        _group.blocksRaycasts = false;

        if (_glowImage == null)
        {
            RectTransform rect = NewRect("Glow", _feedbackRoot, new Vector2(200f, 64f));
            rect.SetAsFirstSibling();
            _glowImage = rect.gameObject.AddComponent<Image>();
            _glowImage.sprite = CreateGlowSprite();
        }
        _glowImage.color = _glowColor;
        _glowImage.raycastTarget = false;

        if (_label == null && _icon == null)
        {
            RectTransform rect = NewRect("Label", _feedbackRoot, new Vector2(180f, 42f));
            _label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            _label.fontSize = 26f;
            _label.fontStyle = FontStyles.Bold;
            _label.alignment = TextAlignmentOptions.Center;
            _label.color = _textColor;
        }
        if (_label != null)
        {
            _label.text = _message;
            _label.color = _textColor;
            _label.raycastTarget = false;
            // Build the fixed label once, before the first reflection.
            _label.ForceMeshUpdate();
        }
        if (_icon != null) _icon.raycastTarget = false;
    }

    private bool ValidateOverrides()
    {
        bool hasOverrides = _label != null || _icon != null || _glowImage != null;
        if (_feedbackRoot == null && !hasOverrides) return true;

        // Never fade the entire HUD or leave an assigned graphic outside the fading group.
        if (_feedbackRoot == null || _feedbackRoot == transform ||
            !_feedbackRoot.IsChildOf(transform) ||
            (_label != null && !_label.transform.IsChildOf(_feedbackRoot)) ||
            (_icon != null && !_icon.transform.IsChildOf(_feedbackRoot)) ||
            (_glowImage != null && !_glowImage.transform.IsChildOf(_feedbackRoot)) ||
            (_icon != null && _icon == _glowImage))
        {
            Debug.LogError("ReflectFeedback: assign a dedicated child Feedback Root containing " +
                "all UI overrides. Icon and Glow Image must be different images.", this);
            return false;
        }
        return true;
    }

    private static RectTransform NewRect(string objectName, Transform parent, Vector2 size)
    {
        var rect = new GameObject(objectName, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.gameObject.layer = parent.gameObject.layer;
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        return rect;
    }

    private Sprite CreateGlowSprite()
    {
        // One small soft sprite per HUD lifetime; uses the existing default UI material.
        const int size = 32;
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = (x + 0.5f) / size * 2f - 1f;
            float dy = (y + 0.5f) / size * 2f - 1f;
            float alpha = Mathf.Max(0f, 1f - dx * dx - dy * dy);
            pixels[y * size + x] = new Color32(255, 255, 255, (byte)(255f * alpha * alpha));
        }
        _ownedGlowTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        _ownedGlowTexture.name = "ReflectGlow";
        _ownedGlowTexture.wrapMode = TextureWrapMode.Clamp;
        _ownedGlowTexture.SetPixels32(pixels);
        _ownedGlowTexture.Apply(false, true);
        _ownedGlowSprite = Sprite.Create(_ownedGlowTexture, new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f), size);
        return _ownedGlowSprite;
    }

    // Reuse the enumerator; no new C# iterator or WaitForSeconds on each reflection.
    // Unity still creates a Coroutine handle when starting a new, non-overlapping run.
    private sealed class FeedbackRoutine : IEnumerator
    {
        private readonly ReflectFeedback _owner;
        public FeedbackRoutine(ReflectFeedback owner) { _owner = owner; }
        public object Current => null;
        public bool MoveNext() => _owner.Tick();
        public void Reset() { }
    }
}
