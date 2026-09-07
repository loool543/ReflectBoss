using UnityEngine;
using UnityEngine.UI;


public class UI_Popup : UI_Base
{
    protected void RetryGame()
    {
        SceneManager.Instance.RetryCurrentScene();
    }
    protected override void Awake()
    {
        base.Awake();
    }
}
