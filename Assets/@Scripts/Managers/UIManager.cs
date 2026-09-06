using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    Transform _root;
    Transform Root
    {
        get
        {
            if (_root == null)
            {
                _root = new GameObject("@UI_Root").transform;
            }
            return _root;   
        }
    }

    #region Scene UI
    private UI_Scene _sceneUI;
    public UI_Scene SceneUI
    {
        get
        {
            if(_sceneUI == null)
                _sceneUI = FindFirstObjectByType<UI_Scene>();

            return _sceneUI;
        }
    }

    //UI를 runtime에 새로 띄우는 경우
    public T ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (_sceneUI != null)
            return _sceneUI as T;

        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        T sceneUI = FindFirstObjectByType<T>();
        if(sceneUI == null)
        {
            GameObject go = ResourceManager.Instance.Instantiate(name);
            sceneUI = Utils.GetOrAddComponent<T>(go);
        }

        sceneUI.transform.SetParent(Root);
        _sceneUI = sceneUI;

        return sceneUI;
    }
    #endregion


    #region Popup UI
    Transform _popupRoot;
    Transform PopupRoot
    {
        get
        {
            if (_popupRoot == null)
            {
                GameObject go = new GameObject("@Popup_Root");
                go.transform.SetParent(Root);
                _popupRoot = go.transform;
            }
            return _popupRoot;
        }
    }

    //popup의 order 관리하는데 0번부터는 일반 scene 100번부터는 popup이 쓰도록
    private int _popupOrder = 100;
    private Stack<UI_Popup> _popupStack = new Stack<UI_Popup>();
    private Dictionary<string, UI_Popup> _popups = new Dictionary<string, UI_Popup>();


    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;
        if (_popups.TryGetValue(name, out UI_Popup popup) == false)
        {
            GameObject go = ResourceManager.Instance.Instantiate(name);
            popup = Utils.GetOrAddComponent<T>(go);
            _popups[name] = popup;
        }
        _popupStack.Push(popup);

        popup.transform.SetParent(PopupRoot);
        popup.gameObject.SetActive(true);
        _popupOrder++;

        popup.GetComponent<Canvas>().sortingOrder = _popupOrder;

        return popup as T;
    }

    public T GetLastPopupUI<T>() where T : UI_Popup
    {
        if (_popupStack.Count == 0)
            return null;

        return _popupStack.Peek() as T;
    }

    public void ClosePopupUI()
    {
        if (_popupStack.Count == 0)
            return;

        UI_Popup popup = _popupStack.Pop();
        popup.gameObject.SetActive(false);
        _popupOrder--;
    }

    public void CloseAllPopupUI()
    {
        while (_popupStack.Count > 0)
            ClosePopupUI();
    }

    #endregion


    public T ShowUI<T>(string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
            name = typeof(T).Name;

        GameObject go = ResourceManager.Instance.Instantiate(name);
        return  go.GetOrAddComponent<T>();
    }


    public void Clear()
    {
        CloseAllPopupUI();
        _popups.Clear();

        Root.DestroyChildren();

        _sceneUI = null;
    }



}
