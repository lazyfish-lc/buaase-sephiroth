using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Reflection.Emit;
public class DisplayTable : MonoBehaviour
{
    public LabelDisplayTable labelDisplayTable;
    public ItemDisplayTable itemDisplayTable;
    public SmallObjectDisplayTable smallObjectDisplayTable;
    public static DisplayTable Instance;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public DisplayInfo GetDisplayInfo(string name,string type)
    {
        // 去除-后的内容
        string classname = name.Split('-')[0];
        if(type == "Item")
        {
            return GetItemInfo(classname);
        }
        else if(type == "Label")
        {
            return GetLabelInfo(classname);
        }
        else if(type == "SmallObject")
        {
            return GetSmallObjectInfo(classname);
        }
        else
        {
            Debug.LogWarning($"未知的类型 {type}，无法获取显示信息");
            return null;
        }
    }

    private ItemInfo GetItemInfo(string itemName)
    {
        return itemDisplayTable.itemInfos.Find(info => info.name == itemName);
    }
    private LabelInfo GetLabelInfo(string labelName)
    {
        return labelDisplayTable.labelInfos.Find(info => info.name == labelName);
    }
    private SmallObjectInfo GetSmallObjectInfo(string smallObjectName)
    {
        return smallObjectDisplayTable.smallObjectInfos.Find(info => info.name == smallObjectName);
    }
    
}