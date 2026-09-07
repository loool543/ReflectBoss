using UnityEngine;


// Run after the existing UI has bound and disabled StartButton.
[DefaultExecutionOrder(100)]
public class LoadingScene : BaseScene
{

    private UI_Notify _notifyUI;

    protected override void Awake()
    {
        base.Awake();
        SceneType = Define.EScene.LoadingScene;

        //TODO : 로딩하는 코드!
        _notifyUI = FindFirstObjectByType<UI_Notify>();
        if (_notifyUI == null)
            Debug.LogError("LoadingScene: UI_Notify is missing.", this);
        else
            _notifyUI.SetStartButtonInteractable(false);
        ResourceManager.Instance.LoadAll(OnProgress, OnComplete);

    }


    void OnProgress(float value)
    {
        Debug.Log($"Loading Progress : {value * 100}%");
    }


    void OnComplete()
    {
        Debug.Log("Loading Complete!");
        DataManager.Instance.LoadData();
        GameManager.Instance.InitializeNewGame();
        if (_notifyUI != null)
            _notifyUI.SetStartButtonInteractable(true);
    }
}
