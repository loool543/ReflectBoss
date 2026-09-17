using System;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceLoader
{
    void LoadAll(Action<float> onProgress = null, Action onComplete = null);
    T Get<T>(string key) where T : UnityEngine.Object; // 메모리에 있는 것 꺼내쓰기
    GameObject Instantiate(string key, Transform parent = null); // 메모리에 있는 것 복제해서 쓰기
    void Destroy(GameObject go);
    void ReleaseAll();


}

public class ResourceManager : Singleton<ResourceManager>
{

    IResourceLoader _loader = new ResourcesLoader();

    public void LoadAll(Action<float> onProgress = null, Action onComplete = null)
    {
        _loader.LoadAll(onProgress, onComplete);
    }

    public T Get<T>(string key) where T : UnityEngine.Object
    {
        return _loader.Get<T>(key);
    }

    public GameObject Instantiate(string key, Transform parent = null)
    {
        return _loader.Instantiate(key, parent);
    }
    
    public void Destroy(GameObject go)
    {
        _loader.Destroy(go);
    }

    public void ReleaseAll()
    {
        _loader.ReleaseAll();
    }

}



//지금은 이부분이 Resources라는 전통(무식)한 방법으로 구현하지만 나중에 이부분만 필요에 따라
//바꾸면 된다!
public class ResourcesLoader : IResourceLoader
{
    private Dictionary<string, UnityEngine.Object> _resources = new Dictionary<string, UnityEngine.Object>();

    public void LoadAll(Action<float> onProgress = null, Action onComplete = null)
    {
        List<string> paths = new List<string> { "Prefabs", "Sounds", "Config" };

        //Resources를 이용한 방법과 Action으로 진행상황 콜백이 잘 어울리진 않지만 일단
        int totalPaths = paths.Count;
        int loadedPaths = 0;


        foreach (string path in paths)
        {
            //TODO : Load All
            UnityEngine.Object[] resources = Resources.LoadAll(path);
            foreach (UnityEngine.Object resource in resources)
            {
                string fullkey = $"{resource.name}_{resource.GetType().Name}";
                if(!_resources.ContainsKey(fullkey))
                {
                    _resources.Add(fullkey, resource);
                }
            }

            //로딩 과정 알려주기
            loadedPaths++;
            float progress = (float)loadedPaths / totalPaths;
            onProgress?.Invoke(progress);
        
            if (loadedPaths >= totalPaths)
            {
                onComplete?.Invoke();
            }
        }
    }

    public T Get<T>(string key) where T : UnityEngine.Object
    {
        string fullkey = $"{key}_{typeof(T).Name}";

        if (_resources.TryGetValue(fullkey, out UnityEngine.Object resource))
            return resource as T;

        return null;
    }

    public GameObject Instantiate(string key, Transform parent = null)
    {
        GameObject prefab = Get<GameObject>(key);
        if (prefab == null)
            return null;

        GameObject instance = UnityEngine.Object.Instantiate(prefab, parent);
        instance.name = prefab.name; //복제된 오브젝트 이름에서 (Clone) 제거
        return instance;

    }

    public void Destroy(GameObject go)
    {
        if (go == null)
            return;
        
        UnityEngine.Object.Destroy(go);
    }


    public void ReleaseAll()
    {
        foreach(UnityEngine.Object resource in _resources.Values)
            Resources.UnloadAsset(resource);

        _resources.Clear();
        Resources.UnloadUnusedAssets();
    }
}