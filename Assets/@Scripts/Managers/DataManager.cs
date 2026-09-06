using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public interface IValidate
{
    bool Validate();
}

public interface IDataLoader<Key, Value> : IValidate
{
    Dictionary<Key, Value> MakeDict();
}
public class DataManager : Singleton<DataManager>
{

    private HashSet<IValidate> _loaders = new HashSet<IValidate>();

    public GameConfig GameConfig { get; private set; }
    public LocalizationConfig LocalizationConfig {  get; private set; }

    public Dictionary<string, TextData> TextDict { get; private set; } = new Dictionary<string, TextData>();
    public Dictionary<int, ItemData> ItemDict { get; private set; }
    = new Dictionary<int, ItemData>();

    public void LoadData()
    {
        GameConfig = LoadScriptableObject<GameConfig>("GameConfig");

        LocalizationConfig = LoadScriptableObject<LocalizationConfig>("LocalizationConfig");

        TextDict = LoadJson<TextDataLoader, string, TextData>("TextData").MakeDict();
        ItemDict = LoadJson<ItemDataLoader, int, ItemData>("ItemData").MakeDict();
        //TODO : 앞으로 추가할 데이터를 계속 늘려가면 됨!

        if (!Validate())
        {
            Debug.LogError("Data validation failed.");
        }
    }

    private T LoadScriptableObject<T>(string path) where T : ScriptableObject
    {
        T asset = ResourceManager.Instance.Get<T>(path);
        if (asset == null)
            Debug.LogError($"Failed to load ScriptableObject at path: {path}");

        return asset;
    }


    private Loader LoadJson<Loader, Key, Value>(string path) where Loader : IDataLoader<Key, Value>
    {
        TextAsset textAsset = ResourceManager.Instance.Get<TextAsset>(path);

        if (textAsset == null)
        {
            Debug.LogError($"Failed to load JSON: {path}");
            return default;
        }

        Loader loader = JsonConvert.DeserializeObject<Loader>(textAsset.text);

        if (loader == null)
        {
            Debug.LogError($"Failed to deserialize JSON: {path}");
            return default;
        }

        _loaders.Add(loader);
        Debug.Log(path);

        return loader;
    }

    private bool Validate()
    {
        bool success = true;

        foreach (IValidate loader in _loaders)
        {
            if (loader.Validate() == false)
                success = false;
        }
        _loaders.Clear();

        return success;

    }
}
