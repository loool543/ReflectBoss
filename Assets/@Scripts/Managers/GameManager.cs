using System;

using UnityEngine;

[Serializable]
public class GameData
{
    public int HP;
    public int MaxHP = 100;
}

public class GameManager : Singleton<GameManager>
{
    public Define.EGameState CurrentState { get; private set; } = Define.EGameState.Playing;

    public void ChangeGameState(Define.EGameState newState)
    {
        if (CurrentState != Define.EGameState.Playing ||
            (newState != Define.EGameState.Success && newState != Define.EGameState.Fail))
            return;
        CurrentState = newState;
        EventManager.Instance.TriggerEvent(Define.EEventType.GameStateChanged);
    }

    [SerializeField]
    private GameData _gameData = new GameData();
    public GameData GameData
    {
        get { return _gameData; }
        set { _gameData = value; }

    }

    public int HP
    {
        get { return GameData.HP; }
        set
        {
            GameData.HP = value;
            EventManager.Instance.TriggerEvent(Define.EEventType.HPChanged);

        }
    }

    public void InitializeNewGame()
    {
        bool stateChanged = CurrentState != Define.EGameState.Playing;
        CurrentState = Define.EGameState.Playing;
        GameConfig config = DataManager.Instance.GameConfig;

        GameData = new GameData()
        {
            HP = config.InitialHP,
            MaxHP = config.InitialHP
        };

        EventManager.Instance.TriggerEvent(Define.EEventType.HPChanged);
        if (stateChanged)
            EventManager.Instance.TriggerEvent(Define.EEventType.GameStateChanged);
    }

}
