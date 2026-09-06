using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class UI_InventoryPopup : UI_Popup
{
    enum Buttons
    {
        DetailButton,
        CloseButton,
    }

    enum Texts
    {
        DetailButtonText,
        CloseButtonText,
    }

    enum Images
    {
        BG,
    }

    enum GameObjects
    {
        Content,
    }

    Transform _content;
    List<UI_InventoryPopup_Item> _items = new List<UI_InventoryPopup_Item>();   

    protected override void Awake()
    {
        base.Awake();

        BindObjects(typeof(GameObjects));
        BindButtons(typeof(Buttons));
        BindImages(typeof(Images));
        BindTexts(typeof(Texts));

        _content = GetObject((int)GameObjects.Content).transform;

        GetButton((int)Buttons.DetailButton).onClick.AddListener(() =>
        {
            UIManager.Instance.ShowPopupUI<UI_DetailPopup>();
        }
        );

        GetButton((int)Buttons.CloseButton).onClick.AddListener(() =>
        {
            UIManager.Instance.ClosePopupUI();
        }
        );

        SetInfo();

    }

    public void SetInfo()
    {
        //테스트 차원에서 넣어놓은 contents의 child들을 밀어주면서 시작
        _content.DestroyChildren();

        for(int i = 0; i < 10; i++)
        {
            UI_InventoryPopup_Item item = UIManager.Instance.ShowUI<UI_InventoryPopup_Item>();
            item.transform.SetParent(_content);

            item.SetInfo(i);
            
            _items.Add(item);
        }
    }
}
