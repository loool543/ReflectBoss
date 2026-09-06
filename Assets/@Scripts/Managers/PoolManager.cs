using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class Pool
{
    private GameObject _prefab; // 어떤 타입의 객체를 관리할지
    private ObjectPool<GameObject> _objectPool; // 실제 객체 풀

    private Transform _root;
    private Transform Root
    {
        get
        {
            return Utils.GetRootTransform(ref _root, $"@{_prefab.name}Pool");
        }
    }



    public Pool(GameObject prefab)
    {
        _prefab = prefab;
        _objectPool = new ObjectPool<GameObject>(OnCreate, OnGet, OnRelease, OnDestroy);
    }


    //초반에 많은 객체 pop하면 렉걸리는 현상이 있을 수 있어서 충분한 갯수의 instance 미리 만들어놓는 함수
    public void Reserve(int count)
    {
        List<GameObject> objects = new List<GameObject>();

        for (int i = 0; i < count; i++)
            objects.Add(Pop());

        for (int i = 0;i < count; i++)
            Push(objects[i]);
    }


    //외부에서 사용할떄는 Push, Pop으로 사용하도록 한다.
    public void Push(GameObject go)  
    {
        if (go.activeSelf)
            _objectPool.Release(go);
    }
    
    public GameObject Pop()
    {
        return _objectPool.Get();
    }


    //내부적으로 작동하는 함수들
    #region Funcs
    private GameObject OnCreate()
    {
        GameObject go = ResourceManager.Instance.Instantiate(_prefab.name);
        go.transform.SetParent(Root);
        go.name = _prefab.name;
        return go;
    }

    private void OnGet(GameObject go)
    {
        go.transform.SetParent(null); //Root에 연결되어 있던 것 연결 끊어줌!
        go.SetActive(true);
    }

    private void OnRelease(GameObject go)
    {
        go.transform.SetParent(Root); //Root에다가 다시 연결!
        go.SetActive(false);
    }

    private void OnDestroy(GameObject go)
    {
        ResourceManager.Instance.Destroy(go);
    }

    #endregion
}


public class PoolManager : Singleton<PoolManager>
{
    private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();


    //Reserve는 Option
    public void Reserve(string prefabName, int count)
    {
        GameObject prefab = ResourceManager.Instance.Get<GameObject>(prefabName);
        Reserve(prefab, count);
    }
    public void Reserve(GameObject prefab, int count)
    {
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);
        _pools[prefab.name].Reserve(count);
    }


    public GameObject Pop(string prefabName)
    {
        
        GameObject prefab = ResourceManager.Instance.Get<GameObject>(prefabName);

        return Pop(prefab);
    }

    public GameObject Pop(GameObject prefab)
    {
        if (_pools.ContainsKey(prefab.name) == false)
            CreatePool(prefab);

        return _pools[prefab.name].Pop();
    }

    public bool Push(GameObject go)
    {
        if (_pools.ContainsKey(go.name) == false)
            return false;

        _pools[go.name].Push(go);
        return true;
    }

    public void Clear()
    {
        _pools.Clear();
    }

    private void CreatePool(GameObject original)
    {
        Pool pool = new Pool(original);
        _pools.Add(original.name, pool);
    }


}
