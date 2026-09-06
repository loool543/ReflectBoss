using UnityEngine;

public class UI_DetailPopup: UI_Popup
{
    enum Buttons
    {
        CloseButton,
    }

    enum Texts
    {
        CloseButtonText,
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


        GetButton((int)Buttons.CloseButton).onClick.AddListener(() =>
        {
            UIManager.Instance.ClosePopupUI();
        }
        );

    }
}
