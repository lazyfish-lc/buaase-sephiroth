using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;
public class BackpackUI : FatherUI
{
    public static BackpackUI Instance;
    public CanvasGroup BackpackCanvas;
    public GameObject[] SlotList; // 存放物品槽的父对象，假设有 28 个子对象命名为 "Slot1", "Slot2", ..., "Slot35"
    public TMP_Text PageNumber;
    public int currentPage;
    public int totalPages;
    public int itemsPerPage ; // 每页显示的物品数量
    public Image DisplayImage; // 显示物品图片的UI组件,初始为透明
    public TMP_Text DisplayName;// 显示物品名称的UI组件
    public TMP_Text DisplayDescription; // 显示物品描述的UI组件
    public Button LabelButton; // 显示标签的按钮
    public Button ItemButton; // 显示物品的按钮
    public Button UseItemButton; // 使用物品的按钮
    private Dictionary<string, int> LabelCount = new Dictionary<string, int>();// 物品标签及其数量的字典
    private Dictionary<string, int> ItemCount = new Dictionary<string, int>();// 物品及其数量的字典
    private Item currentSelectedItem; // 当前选中（大图展示）的物品，null 表示没有选中
    private String currentDisplayType = "Label"; // 当前显示类型，"Label" 或 "Item"
    public static bool isOpen = false;
    public bool IsLabelModeOpen => isOpen && currentDisplayType == "Label";
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
        if (UseItemButton != null)
            UseItemButton.gameObject.SetActive(false);
    }
    public void OpenAndClose()
    {
        
        if(!isOpen)
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
        PlayOpenSFX();
        cleanDisplay();
        isOpen = true;
        BackpackCanvas.gameObject.SetActive(true);
        ShowBackpack(currentDisplayType);
        
    }
    public void Close()
    {
        PlayCloseSFX();
        isOpen = false;
        BackpackCanvas.gameObject.SetActive(false);
    }
    public void RightPage()
    {
        PlayClickSFX();
        if(currentPage < totalPages)
        {
            currentPage++;
            ShowBackpack(currentDisplayType);
        }
    }
    public void LeftPage()
    {
        PlayClickSFX();
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
        List<ObjectLabel> Labels = UIManager.Instance.player.playerState.labelBackpack;
        // 输出列表
        Debug.Log("标签列表:");
        foreach (var label in Labels)
        {
            Debug.Log($"  {label.labelName}");
        }
        List<Item> Items = UIManager.Instance.player.playerState.itemBackpack;
        // 输出列表
        Debug.Log("物品列表:");
        foreach (var item in Items)
        {
            Debug.Log($"  {item.itemName}");
        }
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
            ShowBackpackwith(Type, LabelCount);
        }
        else if(Type == "Item")
        {
            ShowBackpackwith(Type, ItemCount);
        }
    }
    public void ShowBackpackwith(String Type,Dictionary<string, int> Count)
    {
        currentDisplayType = Type;
        Debug.Log("背包：显示" + Type);
        //使用字典显示对应页码的物品标签和数量
        //每页显示 itemsPerPage 个物品，根据 currentPage 计算显示范围
        var ordered = Count.OrderBy(pair => pair.Key, StringComparer.Ordinal).ToList();
        int startIndex = (currentPage - 1) * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, ordered.Count);
        Debug.Log($"显示: currentPage={currentPage}, startIndex={startIndex}, endIndex={endIndex}, totalItems={ordered.Count}");
        for (int i = 0; i < SlotList.Length; i++)
        {
            // 子对象命名是 "Slot1", "Slot2", ..., "Slot35"，根据索引清空显示
            CleanSlot(i);
        }   
        for (int i = startIndex; i < endIndex; i++)
        {
            var item = ordered[i];
            Debug.Log($"显示物品: {item.Key} x{item.Value}");
            if (i - startIndex < SlotList.Length) // 确保不超过格子数量
            {
                LinkSlot(i - startIndex, item.Key, item.Value);
            }
        }
    }
    public void SwitchToLabel()
    {
        PlayClickSFX();
        currentPage = 1; // 切换显示类型时重置页码
        cleanDisplay();
        ShowBackpack("Label");
    }
    public void SwitchToItem()
    {
        PlayClickSFX();
        currentPage = 1; // 切换显示类型时重置页码
        cleanDisplay();
        ShowBackpack("Item");
    }
    void CleanSlot(int index)
    {
        SlotList[index].GetComponentInChildren<TMP_Text>().text = "";
        SlotList[index].GetComponentInChildren<Slot>().Clean("", currentDisplayType, index, OnSlotClicked);
    }
    void LinkSlot(int index, string name, int count)
    {
        SlotList[index].GetComponentInChildren<TMP_Text>().text = $"{count}";
        SlotList[index].GetComponentInChildren<Slot>().Setup(name, currentDisplayType, index, OnSlotClicked);
    }
    // 点击回调，参数为被点击格子的索引
    void OnSlotClicked(int index)
    {
        PlayClickSFX();
        DisplayInfo clickedInfo = SlotList[index].GetComponentInChildren<Slot>().GetCurrentData();
        if (clickedInfo != null)
        {
            
            Debug.Log($"点击了格子 [{index}]：物品 = {clickedInfo.name}/{clickedInfo.displayname}");
            DisplayImage.color = new Color(1, 1, 1, 1); // 设置为不透明
            DisplayImage.sprite = clickedInfo.icon;
            DisplayName.text = clickedInfo.displayname;
            DisplayDescription.text = clickedInfo.description;

            // 在 Item 模式下，从背包中找到实际的 Item 对象，判断是否可使用的恢复类物品
            if (currentDisplayType == "Item")
            {
                currentSelectedItem = FindItemInBackpack(clickedInfo.name);
                bool canUse = currentSelectedItem != null && currentSelectedItem.itemType == ItemType.Recover;
                if (UseItemButton != null)
                    UseItemButton.gameObject.SetActive(canUse);
            }
            else
            {
                currentSelectedItem = null;
                if (UseItemButton != null)
                    UseItemButton.gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.Log($"格子 [{index}] 为空");
            cleanDisplay();
        }
    }

    /// <summary>
    /// 从玩家背包中根据物品名查找第一个匹配的 Item。DisplayTable 使用类名（- 前的部分），故需模糊匹配。
    /// </summary>
    private Item FindItemInBackpack(string clickedName)
    {
        var backpack = UIManager.Instance.player?.playerState?.itemBackpack;
        if (backpack == null) return null;

        foreach (var item in backpack)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.itemName)) continue;
            if (string.Equals(item.itemName, clickedName, StringComparison.Ordinal))
                return item;
        }

        // 回退：DisplayTable 有时用 - 前的类名，所以再次尝试包含匹配
        foreach (var item in backpack)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.itemName)) continue;
            if (item.itemName.Contains(clickedName) || clickedName.Contains(item.itemName))
                return item;
        }

        return null;
    }
    void cleanDisplay()
    {
        DisplayImage.color = new Color(1, 1, 1, 0); // 初始为透明
        DisplayName.text = "";
        DisplayDescription.text = "";
        currentSelectedItem = null;
        if (UseItemButton != null)
            UseItemButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// 使用当前选中的物品（仅 RecoverItem 可调用，由 UseItemButton 绑定）。
    /// 使用后刷新背包显示。
    /// </summary>
    public void UseCurrentItem()
    {
        PlayClickSFX();
        if (currentSelectedItem == null || currentSelectedItem.itemType != ItemType.Recover)
        {
            Debug.LogWarning("[BackpackUI] UseCurrentItem — 当前没有可使用的物品");
            return;
        }

        var player = UIManager.Instance?.player;
        if (player == null)
        {
            Debug.LogWarning("[BackpackUI] UseCurrentItem — player 为空");
            return;
        }

        string usedName = currentSelectedItem.itemName;
        player.UseRecoverItem(currentSelectedItem as RecoverItem);
        cleanDisplay();

        // 刷新背包以反映物品数量变化
        ShowBackpack(currentDisplayType);
    }
}
