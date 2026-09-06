using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : Singleton<ObjectManager>
{
    #region Roots
    private Transform _playerRoot;
    public Transform PlayerRoot
    {
        get
        {
            return Utils.GetRootTransform(ref _playerRoot, "@Players");
        }
    }

    private Transform _monsterRoot;
    public Transform MonsterRoot
    {
        get
        {
            return Utils.GetRootTransform(ref _monsterRoot, "@Monsters");
        }
    }

    private Transform _npcRoot;
    public Transform NPCRoot
    {
        get
        {
            return Utils.GetRootTransform(ref _npcRoot, "@NPCs");
        }
    }
    #endregion


    private HashSet<ObjectBase> _objects = new HashSet<ObjectBase>(); //통합

    //개별적으로도 관리하도록 해서 2중으로 관리하도록 한다. (필요에 따라)
    private HashSet<Player> _players = new HashSet<Player>();
    private HashSet<Monster> _monsters = new HashSet<Monster>();

    public Player SpawnPlayer(string prefab = "Player", bool pooling = false)
    {
        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);

        go.name = prefab;
        go.transform.SetParent(PlayerRoot);

        Player player = go.GetOrAddComponent<Player>();
        _objects.Add(player);
        _players.Add(player);

        player.Pooling = pooling;

        return player;
    }

    public Monster SpawnMonster(string prefab = "Monster", bool pooling = false)
    {

        GameObject go = null;
        if (pooling)
            go = PoolManager.Instance.Pop(prefab);
        else
            go = ResourceManager.Instance.Instantiate(prefab);


        go.name = prefab;
        go.transform.SetParent(MonsterRoot);

        Monster monster = go.GetOrAddComponent<Monster>();
        _objects.Add(monster);
        _monsters.Add(monster);

        monster.Pooling = pooling;

        return monster;
    }


    public void Despawn(ObjectBase obj) 
    {
        if (obj == null)
            return;

        _objects.Remove(obj);

        if (obj is Player player)
            _players.Remove(player);
        else if (obj is Monster monster)
            _monsters.Remove(monster);

        if (obj.Pooling)
        {
            obj.Init();
            PoolManager.Instance.Push(obj.gameObject);
        }
        else
            ResourceManager.Instance.Destroy(obj.gameObject);

    }
}
