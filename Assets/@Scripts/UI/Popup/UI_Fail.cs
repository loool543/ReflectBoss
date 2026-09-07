using UnityEngine;

public class UI_Fail : UI_Popup
{
    enum Buttons
    {
        RetryButton,
    }

    enum Texts
    {
        RetryButtonText,
        FailText,
    }

    enum Images
    {
        FailBG,
    }



    protected override void Awake()
    {
        base.Awake();

        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));


        GetButton((int)Buttons.RetryButton).onClick.AddListener(() =>
        {
            // Regame
        }
        );

    }
}

