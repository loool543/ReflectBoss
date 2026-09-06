using UnityEngine;

public class UI_InventoryPopup_Item : UI_Base
{
    enum Buttons
    {
        UpgradeButton,
    }

    enum Texts
    {
        UpgradeButtonText,
        ItemNameText,
    }

    enum Images
    {

    }

    enum GameObjects
    {

    }

    int _templateID;

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));


        GetButton((int)Buttons.UpgradeButton).onClick.AddListener(() =>
        {
            Debug.Log("Upgrade Button Clicked");
        }
        );

    }

    //초기값 -> 해당 UI가 표시해야하는 데이터를 넘겨주는 기능
    public void SetInfo(int templateID)
    {
        _templateID = templateID;
        RefreshUI();
    }

    public void RefreshUI()
    {
        GetText((int)Texts.ItemNameText).text = $"Item {_templateID}";
    }

}
