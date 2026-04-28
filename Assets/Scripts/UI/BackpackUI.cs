using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.PlayerLoop;
public class BackpackUI : MonoBehaviour
{
    public CanvasGroup BackpackCanvas;
    public static BackpackUI Instance;

    public PlayerSmallObject player;

    public GameObject[] SlotList; // 存放物品槽的父对象，假设有 35 个子对象命名为 "Slot1", "Slot2", ..., "Slot35"

    public TMP_Text PageNumber;

    public int currentPage;
    public int totalPages;

    public int itemsPerPage ; // 每页显示的物品数量

    public Image ItemImage; // 显示物品图片的UI组件
    public TMP_Text ItemName; // 显示物品名称的UI组件

    public TMP_Text ItemDescription; // 显示物品描述的UI组件


    public Button LabelButton; // 显示标签的按钮
    public Button ItemButton; // 显示物品的按钮

    private Dictionary<string, int> LabelCount = new Dictionary<string, int>();// 物品标签及其数量的字典
    private Dictionary<string, int> ItemCount = new Dictionary<string, int>();// 物品及其数量的字典

    private String currentDisplayType = "Label"; // 当前显示类型，"Label" 或 "Item"

    public bool isOpen = false;
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
    void Start()
    {
        BackpackCanvas.gameObject.SetActive(false);
        UpdateBackpack();
    }
    public void OpenAndClose()
    {
        
        if(BackpackCanvas.gameObject.activeSelf == false)
        {
            Open();
        }
        else
        {
            Close();
        }
        
    }
    public void Open()
    {
        isOpen = true;
        BackpackCanvas.gameObject.SetActive(true);
        ShowBackpack(currentDisplayType);
        
    }
    public void Close()
    {
        isOpen = false;
        BackpackCanvas.gameObject.SetActive(false);
    }
    public void RightPage()
    {
        if(currentPage < totalPages)
        {
            currentPage++;
            ShowBackpack(currentDisplayType);
        }
    }
    public void LeftPage()
    {
        if(currentPage > 1)
        {
            currentPage--;
            ShowBackpack(currentDisplayType);
            
        }
    }
        public void UpdatePageNumber()
    {
        PageNumber.text = $"PAGE: {currentPage}/{totalPages}";
    }

    //背包物品显示方法
    public void UpdateBackpack()
    {
        // 更新页码
        UpdatePageNumber();
        // 获取物品标签和物品列表
        List<ObjectLabel> Labels = new List<ObjectLabel>();// = player.playerState.smallObjectLabels;
        List<Item> Items = new List<Item>();// = player.playerState.smallObjectItems;
        //分页显示逻辑,将列表分成多页，每页显示 itemsPerPage 个物品，相同标签记录数量
        LabelCount.Clear();
        foreach (var label in Labels)
        {
            if (LabelCount.ContainsKey(label.labelName))
            {
                LabelCount[label.labelName]++;
            }
            else
            {
                LabelCount[label.labelName] = 1;
            }
        }
        ItemCount.Clear();
        foreach (var item in Items)        {
            if (ItemCount.ContainsKey(item.itemName))
            {
                ItemCount[item.itemName]++;
            }
            else
            {
                ItemCount[item.itemName] = 1;
            }
        }
        // 测试数据，实际使用时从 player 的状态中获取物品标签和数量
        //LabelCount.Add("Health Potion", 5);
        //LabelCount.Add("Mana Potion", 3);
        //LabelCount.Add("Sword", 1);
        //LabelCount.Add("Shield", 1);
    }
    public void ShowBackpack(String Type)
    {
        UpdateBackpack();
        if(Type == "Label")
        {
            ShowBackpackLabel();
        }
        else if(Type == "Item")
        {
            ShowBackpackItem();
        }

    }
    public void ShowBackpackItem()
    {
        currentDisplayType = "Item";
        Debug.Log("背包：显示物品");
        //使用字典显示对应页码的物品标签和数量
        //每页显示 itemsPerPage 个物品，根据 currentPage 计算显示范围
        int startIndex = (currentPage - 1) * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, ItemCount.Count);
        Debug.Log($"显示: currentPage={currentPage}, startIndex={startIndex}, endIndex={endIndex}, totalItems={ItemCount.Count}");
        for (int i = 0; i < SlotList.Length; i++)
        {
            // 子对象命名是 "Slot1", "Slot2", ..., "Slot35"，根据索引清空显示
            SlotList[i].GetComponentInChildren<TMP_Text>().text = "";
        }   
        for (int i = startIndex; i < endIndex; i++)
        {
            var item = new List<KeyValuePair<string, int>>(ItemCount)[i];
            Debug.Log($"显示物品: {item.Key} x{item.Value}");
            SlotList[i].GetComponentInChildren<TMP_Text>().text = $"{item.Value}";
        }

    }
    public void ShowBackpackLabel()
    {
        currentDisplayType = "Label";
        Debug.Log("背包：显示标签");
        //使用字典显示对应页码的物品标签和数量
        //每页显示 itemsPerPage 个物品，根据 currentPage 计算显示范围
        int startIndex = (currentPage - 1) * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, LabelCount.Count);
        Debug.Log($"显示: currentPage={currentPage}, startIndex={startIndex}, endIndex={endIndex}, totalItems={LabelCount.Count}");
        for (int i = 0; i < SlotList.Length; i++)
        {
            // 子对象命名是 "Slot1", "Slot2", ..., "Slot35"，根据索引清空显示
            SlotList[i].GetComponentInChildren<TMP_Text>().text = "";
        }
        for (int i = startIndex; i < endIndex; i++)
        {
            var item = new List<KeyValuePair<string, int>>(LabelCount)[i];
            Debug.Log($"显示物品: {item.Key} x{item.Value}");
            SlotList[i].GetComponentInChildren<TMP_Text>().text = $"{item.Value}";
        }
    }
    public void SwitchToLabel()
    {
        currentPage = 1; // 切换显示类型时重置页码
        ShowBackpack("Label");
    }
    public void SwitchToItem()
    {
        currentPage = 1; // 切换显示类型时重置页码
        ShowBackpack("Item");
    }
}


