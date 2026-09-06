using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{

    static T _instance;
    static bool _init = false;
    public static T Instance
    {
        get
        {
            if (_instance == null && _init == false)
            {
                //혹시 다른 곳에서 실수로 drag&drop로 붙여넣기 한 경우를 대비해서 씬에서 찾아본다.
                _instance = FindFirstObjectByType<T>();
                _init = true;

                if (_instance == null)
                 {
                    //씬에 없으면 새로 만든다.
                    GameObject go = new GameObject($"@{typeof(T).Name}");
                    _instance = go.AddComponent<T>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
}
