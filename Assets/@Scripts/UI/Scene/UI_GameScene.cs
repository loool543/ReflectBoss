using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    [SerializeField, Min(0f)] private float _hpBarSmoothTime = 0.2f;
    [SerializeField, Min(0.01f)] private float _hitFlashTime = 0.12f;
    [SerializeField] private Color _stage2Color = new Color(1f, 0.48f, 0.28f);
    private Boss _boss;
    private Color _stage1Color;
    private Color _playerFillColor, _bossFillColor;
    private Coroutine _stageCoroutine;
    private int _lastStage = -1;
    private Coroutine _playerHPCoroutine;
    private Coroutine _bossHPCoroutine;
    private bool _refreshImmediately;
    private UI_Popup _resultPopup;
    enum Texts
    {
        StageText,
    }

    enum Images
    {
        PlayerImage,
        BossImage,
    }

    enum Sliders
    {
        PlayerSlider,
        BossSlider,
    }

    protected override void Awake()
    {
        base.Awake();

        BindImages(typeof(Images));
        BindTexts(typeof(Texts));
        BindSliders(typeof(Sliders));
        _boss = FindFirstObjectByType<Boss>();
        if (GetText((int)Texts.StageText) != null)
            _stage1Color = GetText((int)Texts.StageText).color;
        ConfigureSlider(GetSlider((int)Sliders.PlayerSlider));
        ConfigureSlider(GetSlider((int)Sliders.BossSlider));
        _playerFillColor = GetSlider((int)Sliders.PlayerSlider).fillRect.GetComponent<Image>().color;
        _bossFillColor = GetSlider((int)Sliders.BossSlider).fillRect.GetComponent<Image>().color;
    }

    private void OnEnable()
    {

        EventManager.Instance.AddEvent(Define.EEventType.HPChanged, RefreshPlayerHP);
        EventManager.Instance.AddEvent(Define.EEventType.BossHPChanged, RefreshBossHP);
        EventManager.Instance.AddEvent(Define.EEventType.BossStageChanged, RefreshStage);
        EventManager.Instance.AddEvent(Define.EEventType.GameStateChanged, RefreshResult);
        RefreshUI();
    }

    private void OnDisable()
    {

        EventManager.Instance.RemoveEvent(Define.EEventType.HPChanged, RefreshPlayerHP);
        EventManager.Instance.RemoveEvent(Define.EEventType.BossHPChanged, RefreshBossHP);
        EventManager.Instance.RemoveEvent(Define.EEventType.BossStageChanged, RefreshStage);
        EventManager.Instance.RemoveEvent(Define.EEventType.GameStateChanged, RefreshResult);
        StopHPAnimation(ref _playerHPCoroutine);
        StopHPAnimation(ref _bossHPCoroutine);
        if (_stageCoroutine != null) StopCoroutine(_stageCoroutine);
        _stageCoroutine = null;
        GetText((int)Texts.StageText).rectTransform.localScale = Vector3.one;
        GetSlider((int)Sliders.PlayerSlider).fillRect.GetComponent<Image>().color = _playerFillColor;
        GetSlider((int)Sliders.BossSlider).fillRect.GetComponent<Image>().color = _bossFillColor;
    }

    //View
    public override void RefreshUI()
    {
        // UI_Base.Start also calls this after scene Awake/OnEnable initialization.
        _refreshImmediately = true;
        RefreshPlayerHP();
        RefreshBossHP();
        RefreshStage();
        _refreshImmediately = false;
    }

    private void RefreshPlayerHP()
    {
        GameManager game = GameManager.Instance;
        RefreshHP(GetSlider((int)Sliders.PlayerSlider), game.HP, game.GameData.MaxHP,
            _playerFillColor, ref _playerHPCoroutine);
    }

    private void RefreshResult()
    {
        Define.EGameState state = GameManager.Instance.CurrentState;
        if (state == Define.EGameState.Playing || _resultPopup != null)
            return;
        Time.timeScale = 0f;
        if (state == Define.EGameState.Success)
            _resultPopup = UIManager.Instance.ShowPopupUI<UI_Success>();
        else if (state == Define.EGameState.Fail)
            _resultPopup = UIManager.Instance.ShowPopupUI<UI_Fail>();
    }

    private void RefreshBossHP()
    {
        if (_boss == null)
            _boss = FindFirstObjectByType<Boss>();
        if (_boss == null)
            return;
        RefreshHP(GetSlider((int)Sliders.BossSlider), _boss.CurrentHP, _boss.MaxHP,
            _bossFillColor, ref _bossHPCoroutine);
    }

    private void RefreshStage()
    {
        if (_boss == null)
            _boss = FindFirstObjectByType<Boss>();
        var text = GetText((int)Texts.StageText);
        if (_boss == null || text == null)
            return;
        bool stage2 = _boss.CurrentStage == Boss.BossStage.Stage2;
        int stage = stage2 ? 2 : 1;
        text.text = stage2 ? "STAGE 02" : "STAGE 01";
        text.color = stage2 ? _stage2Color : _stage1Color;
        if (!_refreshImmediately && _lastStage >= 0 && stage != _lastStage)
        {
            if (_stageCoroutine != null) StopCoroutine(_stageCoroutine);
            _stageCoroutine = StartCoroutine(CoStagePulse(text.rectTransform));
        }
        _lastStage = stage;
    }

    private IEnumerator CoStagePulse(RectTransform target)
    {
        const float duration = 0.25f;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            target.localScale = Vector3.one * Mathf.Lerp(1.12f, 1f, elapsed / duration);
            yield return null;
        }
        target.localScale = Vector3.one;
        _stageCoroutine = null;
    }

    private static void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;
        slider.wholeNumbers = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;
        slider.transition = Selectable.Transition.None;
    }

    private void RefreshHP(Slider slider, int currentHP, int maxHP, Color baseColor, ref Coroutine routine)
    {
        StopHPAnimation(ref routine);
        if (slider == null)
            return;
        float target = Mathf.Clamp01(maxHP > 0 ? (float)currentHP / maxHP : 0f);
        Image fill = slider.fillRect.GetComponent<Image>();
        fill.color = baseColor;
        if (_refreshImmediately || !isActiveAndEnabled || _hpBarSmoothTime <= 0f)
            slider.SetValueWithoutNotify(target);
        else
            routine = StartCoroutine(CoRefreshHP(slider, target, fill, baseColor));
    }

    private IEnumerator CoRefreshHP(Slider slider, float target, Image fill, Color baseColor)
    {
        float start = slider.value;
        bool damaged = target < start;
        float duration = Mathf.Max(_hpBarSmoothTime, _hitFlashTime);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            slider.SetValueWithoutNotify(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / _hpBarSmoothTime)));
            fill.color = Color.Lerp(baseColor, Color.white,
                damaged ? 0.7f * (1f - Mathf.Clamp01(elapsed / _hitFlashTime)) : 0f);
            yield return null;
        }
        slider.SetValueWithoutNotify(target);
        fill.color = baseColor;
    }

    private void StopHPAnimation(ref Coroutine routine)
    {
        if (routine == null)
            return;
        StopCoroutine(routine);
        routine = null;
    }
}
