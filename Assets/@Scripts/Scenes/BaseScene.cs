using UnityEngine;

public class BaseScene : MonoBehaviour
{
    public Define.EScene SceneType { get; protected set; } = Define.EScene.Unknown;

    protected virtual void Awake()
    {
        //모든 씬에서 초기화 하고 싶은 공통부분 넣어주면 됨!

    } 
}
