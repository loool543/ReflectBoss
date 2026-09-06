using System;
using UnityEngine;
using System.Collections.Generic;


[Serializable]
public class ItemData
{
        public int TemplateID;
        public int ItemType;
        public string NameTextID;
        public string DescriptionTextID;
        public string IconImageID;
        public string PrefabNameID;
}


[Serializable]
public class ItemDataLoader : IDataLoader<int, ItemData>
{
    public List<ItemData> items = new List<ItemData>();

    public Dictionary<int, ItemData> MakeDict()
    {
        Dictionary<int, ItemData> dict = new Dictionary<int, ItemData>();
        foreach(var item in items)
        {
            dict.Add(item.TemplateID, item);
        }
        return dict;
    }

    //데이터 시트가 문제가 없는지를 확인하는게 마지막 단계에서 꼭 필요함
    //ex 만약 없는 prefab을 참조하거나 하면 build단계에서 에러를 내서 찾아야한다!
    public bool Validate()
    {

        return true;
    }
}  