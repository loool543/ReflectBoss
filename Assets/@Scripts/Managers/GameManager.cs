using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameData
{
    public int Gold;
    public int Level;
    public int HP ;
    public int MaxHP = 100;
}

public class GameManager : Singleton<GameManager>
{

    [SerializeField]
    private GameData _gameData = new GameData();
    public GameData GameData
    {
        get { return _gameData; }
        set { _gameData = value; }

    }

    public int Gold
    {
        get { return GameData.Gold; }
        set
        {
            GameData.Gold = value; 
            EventManager.Instance.TriggerEvent(Define.EEventType.GoldChanged);
            // TODO : 골드가 변경 될 떄마다 모두에게 전파

        }
    }

    public int HP
    {
        get { return GameData.HP; }
        set
        {
            GameData.HP = value;
            EventManager.Instance.TriggerEvent(Define.EEventType.HPChanged);
            // TODO : HP가 변경 될 떄마다 모두에게 전파

        }
    }

    public void InitializeNewGame()
    {
        GameConfig config = DataManager.Instance.GameConfig;

        GameData = new GameData()
        {
            Gold = config.InitialGold,
            Level = config.InitialLevel,
            HP = config.InitialHP,
            MaxHP = config.InitialHP
        };

        EventManager.Instance.TriggerEvent(Define.EEventType.HPChanged);
    }


}
