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
                _instance = FindFirstObjectByType<T>();
                _init = true;

                if (_instance == null)
                 {
                    //Make sure the name of the GameObject is unique to avoid conflicts in the hierarchy
                    GameObject go = new GameObject($"@{typeof(T).Name}");
                    _instance = go.AddComponent<T>();
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
    }
}
