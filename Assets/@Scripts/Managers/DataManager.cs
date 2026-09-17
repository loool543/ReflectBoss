using UnityEngine;

public class DataManager : Singleton<DataManager>
{
    public GameConfig GameConfig { get; private set; }

    public void LoadData()
    {
        GameConfig = ResourceManager.Instance.Get<GameConfig>("GameConfig");
        if (GameConfig == null)
            Debug.LogError("DataManager: GameConfig is missing from loaded resources.", this);
    }
}
