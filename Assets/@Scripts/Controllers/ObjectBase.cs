using UnityEngine;

public class ObjectBase : MonoBehaviour
{
    public virtual void Awake()
    {
        Init();
    }

    public virtual void Init()
    {
    }
}
