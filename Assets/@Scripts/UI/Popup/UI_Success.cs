using UnityEngine;

public class UI_Success : UI_Popup
{
    enum Buttons
    {
        RetryButton,
    }

    enum Texts
    {
        RetryButtonText,
        SuccessText,
    }

    enum Images
    {
        SuccessBG,
    }



    protected override void Awake()
    {
        base.Awake();

        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));


        GetButton((int)Buttons.RetryButton).onClick.AddListener(() =>
        {
            RetryGame();
        }
        );

    }
}

