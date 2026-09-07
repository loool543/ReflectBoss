using UnityEngine;



public class DevScene : BaseScene
{ 
    protected override void Awake()
    {
        base.Awake();
        SceneType = Define.EScene.DevScene;

        //TODO : 개발용 씬에서 필요한 초기화 코드 넣기!

        //일단 test용으로 바로 DevScene에서 시작하도록 잠깐 넣어줌

        SoundManager.Instance.Play2D(Define.ESound.Bgm, "bgm");
        //SaveManager.Instance.Load();
        //SaveManager.Instance.StartAutoSave();

        //Debug.Log(DataManager.Instance.GameConfig.InitialGold);
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
        //UI
        //UIManager.Instance.ShowSceneUI<UI_DevScene>();


        //foreach (var item in DataManager.Instance.ItemDict.Values)
        //{
        //    Debug.Log($"Item TemplateId: {item.TemplateID}, NameTextId: {item.NameTextID}");
        //}
    

}
 