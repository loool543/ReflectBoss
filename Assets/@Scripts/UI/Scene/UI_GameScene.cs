using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_GameScene : UI_Scene
{
    [SerializeField, Min(0f)] private float _hpBarSmoothTime = 0.2f;
    private Boss _boss;
    private Color _stage1Color;
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
            ref _playerHPCoroutine);
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
            ref _bossHPCoroutine);
    }

    private void RefreshStage()
    {
        if (_boss == null)
            _boss = FindFirstObjectByType<Boss>();
        var text = GetText((int)Texts.StageText);
        if (_boss == null || text == null)
            return;
        bool stage2 = _boss.CurrentStage == Boss.BossStage.Stage2;
        text.text = stage2 ? "Stage 2" : "Stage 1";
        text.color = stage2 ? Color.red : _stage1Color;
    }

    private static void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;
        slider.wholeNumbers = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;
    }

    private void RefreshHP(Slider slider, int currentHP, int maxHP, ref Coroutine routine)
    {
        StopHPAnimation(ref routine);
        if (slider == null)
            return;
        float target = Mathf.Clamp01(maxHP > 0 ? 1f - (float)currentHP / maxHP : 1f);
        if (_refreshImmediately || !isActiveAndEnabled || _hpBarSmoothTime <= 0f)
            slider.SetValueWithoutNotify(target);
        else
            routine = StartCoroutine(CoRefreshHP(slider, target));
    }

    private IEnumerator CoRefreshHP(Slider slider, float target)
    {
        float start = slider.value;
        float duration = _hpBarSmoothTime;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            slider.SetValueWithoutNotify(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        slider.SetValueWithoutNotify(target);
    }

    private void StopHPAnimation(ref Coroutine routine)
    {
        if (routine == null)
            return;
        StopCoroutine(routine);
        routine = null;
    }
}
