using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;



public class DevScene : BaseScene
{ 
    protected override void Awake()
    {
        base.Awake();
        SceneType = Define.EScene.DevScene;

        //TODO : 개발용 씬에서 필요한 초기화 코드 넣기!

        //일단 test용으로 바로 DevScene에서 시작하도록 잠깐 넣어줌
        ResourceManager.Instance.LoadAll();
        DataManager.Instance.LoadData();

        SoundManager.Instance.Play2D(Define.ESound.Bgm, "bgm");

        StartCoroutine(CoPlaySound());

        SaveManager.Instance.Load();
        SaveManager.Instance.StartAutoSave();

        //Debug.Log(DataManager.Instance.GameConfig.InitialGold);
    }

    IEnumerator CoPlaySound()
        {
            while (true)
            {
                yield return new WaitForSeconds(5);
                SoundManager.Instance.Play3D("dropSound", gameObject);
            }
        }
        //UI
        //UIManager.Instance.ShowSceneUI<UI_DevScene>();


        //foreach (var item in DataManager.Instance.ItemDict.Values)
        //{
        //    Debug.Log($"Item TemplateId: {item.TemplateID}, NameTextId: {item.NameTextID}");
        //}
    

}
 