using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public bool Pooling { get; set; } = false;


    public virtual void Awake()
    {
        Init();
    }
    //Pooling이 필요한 객체들은 Init()함수에서 초기화 작업을 해주도록 한다.
    //예전 데이터가 남은채로 와서 문제가 발생할 수 있기 때문
    public virtual void Init()
    {

    }

}
