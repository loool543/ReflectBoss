using UnityEngine;

public class DevScene : BaseScene
{ 
    protected override void Awake()
    {
        base.Awake();
        SceneType = Define.EScene.DevScene;

        SoundManager.Instance.Play2D(Define.ESound.Bgm, "bgm");
    }

    private Boss _boss;

    private void OnEnable()
    {
        EventManager.Instance.AddEvent(Define.EEventType.BossStageChanged, RefreshBgm);
        EventManager.Instance.AddEvent(Define.EEventType.GameStateChanged, OnGameStateChanged);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveEvent(Define.EEventType.BossStageChanged, RefreshBgm);
        EventManager.Instance.RemoveEvent(Define.EEventType.GameStateChanged, OnGameStateChanged);
    }

    private void RefreshBgm()
    {
        if (GameManager.Instance.CurrentState != Define.EGameState.Playing)
            return;
        if (_boss == null)
            _boss = FindFirstObjectByType<Boss>();
        bool stage2 = _boss != null && _boss.CurrentStage == Boss.BossStage.Stage2;
        SoundManager.Instance.Play2D(Define.ESound.Bgm, stage2 ? "bgm2" : "bgm");
    }

    private void OnGameStateChanged()
    {
        Define.EGameState state = GameManager.Instance.CurrentState;
        if (state == Define.EGameState.Playing)
        {
            RefreshBgm();
            return;
        }
        SoundManager.Instance.Stop(Define.ESound.Bgm);
        if (state == Define.EGameState.Fail)
            SoundManager.Instance.Play2D(Define.ESound.Fail, "fail");
        else if (state == Define.EGameState.Success)
            SoundManager.Instance.Play2D(Define.ESound.Success, "success");
    }
    

}
 