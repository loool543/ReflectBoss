using UnityEngine;
using UnityEngine.UI;

public class UI_Notify : UI_Scene
{
    private Button _startButton;
    enum Buttons
    {
        StartButton,
    }



    protected override void Awake()
    {
        base.Awake();

        BindButtons(typeof(Buttons));
        _startButton = GetButton((int)Buttons.StartButton);
        if (_startButton == null)
        {
            Debug.LogError("UI_Notify: StartButton binding is missing.", this);
            return;
        }
        _startButton.interactable = false;


        _startButton.onClick.AddListener(() =>
        {
            if (!_startButton.interactable)
                return;
            _startButton.interactable = false;
            SceneManager.Instance.LoadScene(Define.EScene.DevScene);
        }
);

    }

    public void SetStartButtonInteractable(bool interactable)
    {
        if (_startButton != null)
            _startButton.interactable = interactable;
    }
}
