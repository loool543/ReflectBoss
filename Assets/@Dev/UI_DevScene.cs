using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DevScene : UI_Scene
{
    enum Buttons
    {
        ClickButton,
    }


    enum Texts
    {
        GoldText,
        ClickButtonText,
    }

    enum Images
    {
        BG,
    }   


    
    protected override void Awake()
    {
        base.Awake();

        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        GetButton((int)Buttons.ClickButton).onClick.AddListener(OnClickButton);

    }

    private void OnEnable()
    {
        EventManager.Instance.AddEvent(Define.EEventType.GoldChanged, RefreshUI);
    }

    private void OnDisable()
    {
        EventManager.Instance.RemoveEvent(Define.EEventType.GoldChanged, RefreshUI);
    }

    //Controller
    public void OnClickButton()
    {
        GameManager.Instance.Gold++;
        RefreshUI();
    }

    //View
    public void RefreshUI()
    {
        GetText((int)Texts.GoldText).text = $"Gold : {GameManager.Instance.Gold}";
    }

}
