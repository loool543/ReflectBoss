using UnityEngine;

public class SceneManager : Singleton<SceneManager>
{
    private bool _retrying;

    public void RetryCurrentScene()
    {
        if (_retrying)
            return;
        _retrying = true;
        Time.timeScale = 1f;
        UIManager.Instance.Clear();
        GameManager.Instance.InitializeNewGame();
        SoundManager.Instance.Clear();
        LoadScene(Define.EScene.DevScene);
        _retrying = false;
    }
    private BaseScene _currentScene;
    public BaseScene CurrentScene
    {
        get
        {
            if (_currentScene == null)
            {
                _currentScene = FindFirstObjectByType<BaseScene>();
            }
            return _currentScene;
        }
    }

    public Define.EScene CurrentSceneType
    {
        get
        {
            if (CurrentScene == null)
                return Define.EScene.Unknown;
            return CurrentScene.SceneType;
        }
    }   


    public void LoadScene(Define.EScene sceneType)
    {
        string sceneName = sceneType.ToString();
#if UNITY_EDITOR
        // DevScene can be opened directly without a Build Settings entry.
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!Application.CanStreamedLevelBeLoaded(sceneName) && activeScene.name == sceneName)
        {
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(activeScene.path,
                new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            _currentScene = null;
            return;
        }
#endif
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        _currentScene = null; // Reset current scene to be found again
    }

}


