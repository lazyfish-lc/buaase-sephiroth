using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using UnityEngine.PlayerLoop;
using System.Linq;
public class LabelUI : FatherUI
{
    public static LabelUI Instance;
    public CanvasGroup LabelCanvas;
    public GameObject[] SlotList; // 存放物品槽的父对象，假设有 10个子对象命名为 "Slot1", "Slot2", ..., "Slot35"
    public TMP_Text PageNumber;
    public int currentPage;
    public int totalPages;
    public int itemsPerPage ; // 每页显示的物品数量
    public Image DisplayImage; // 显示物品图片的UI组件,初始为透明
    public TMP_Text DisplayName;// 显示物品名称的UI组件
    public TMP_Text DisplayDescription; // 显示物品描述的UI组件
    private Dictionary<string, int> LabelCount = new Dictionary<string, int>();// 物品标签及其数量的字典
    private String currentDisplayType = "Label"; // 当前显示类型，"Label" 或 "Item"
    public static bool isOpen = false;
    private SmallObject currentSmallObject; // 当前显示标签的物体引用
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
        //不可见且不可交互
        LabelCanvas.alpha = 0;
        LabelCanvas.interactable = false;
        LabelCanvas.blocksRaycasts = false;
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
        LabelCanvas.alpha = 1;
        LabelCanvas.interactable = true;
        LabelCanvas.blocksRaycasts = true;
        ShowLabel(currentDisplayType);
        
    }
    public void Close()
    {
        PlayCloseSFX();
        isOpen = false;
        LabelCanvas.alpha = 0;
        LabelCanvas.interactable = false;
        LabelCanvas.blocksRaycasts = false;
    }
    public void RightPage()
    {
        PlayClickSFX();
        if(currentPage < totalPages)
        {
            currentPage++;
            ShowLabel(currentDisplayType);
        }
    }
    public void LeftPage()
    {
        PlayClickSFX();
        if(currentPage > 1)
        {
            currentPage--;
            ShowLabel(currentDisplayType);
            
        }
    }
    public void UpdatePageNumber()
    {
        PageNumber.text = $"PAGE: {currentPage}/{totalPages}";
    }
    private void OnEnable() {
        // 1. 订阅场景中所有SmallObject的事件
        // 这里假设SmallObject有一个事件OnShowLabel，当需要显示标签时触发，传递SmallObject的引用
        var Smalls = FindObjectsByType<SmallObject>(FindObjectsSortMode.None);
        foreach (var Small in Smalls) {
            Small.OnShowLabel += HandleLabel;
        }
    }
    private void OnDisable() {
        var Smalls = FindObjectsByType<SmallObject>(FindObjectsSortMode.None);
        foreach (var Small in Smalls) {
            Small.OnShowLabel -= HandleLabel;
        }
    }
    private void HandleLabel(SmallObject Small) {
        // 2. 捕获SmallObject引用
        currentSmallObject = Small;
        Debug.Log("HandleLabel(");
        Open();
    }
    //背包物品显示方法
    public void UpdateLabelDisplay()
    {
        // 更新页码
        UpdatePageNumber();
        // 获取交互对象标签列表
        List<ObjectLabel> Labels = currentSmallObject.dynamicState.smallObjectLabels;
        // 输出列表
        Debug.Log("标签列表:");
        foreach (var label in Labels)
        {
            Debug.Log($"  {label.labelName}");
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
        // 测试数据，实际使用时从 player 的状态中获取物品标签和数量
        //LabelCount.Add("Hard", 5);
        //LabelCount.Add("Lock", 3);
        //LabelCount.Add("Fragile", 1);
    }
    public void ShowLabel(String Type)
    {
        UpdateLabelDisplay();
        if(Type == "Label")
        {
            ShowLabelWith(Type, LabelCount);
        }
    }
    public bool TryReceiveLabelFromPlayer(ObjectLabel label)
    {
        if (!isOpen || currentSmallObject == null || label == null)
        {
            return false;
        }
        if (currentSmallObject.dynamicState == null)
        {
            currentSmallObject.dynamicState = new SmallObjectDynamicState();
        }
        if (currentSmallObject.dynamicState.smallObjectLabels == null)
        {
            currentSmallObject.dynamicState.smallObjectLabels = new List<ObjectLabel>();
        }
        if (!currentSmallObject.dynamicState.smallObjectLabels.Contains(label))
        {
            currentSmallObject.dynamicState.smallObjectLabels.Add(label);
        }
        label.AttachToOwner(currentSmallObject);
        PlayClickSFX();
        return true;
    }
    public bool TryTransferLabelToPlayer(string labelName, PlayerSmallObject player)
    {
        if (!isOpen || currentSmallObject == null || player == null)
        {
            return false;
        }
        var labels = currentSmallObject.dynamicState?.smallObjectLabels;
        if (labels == null)
        {
            return false;
        }
        var label = labels.Find(lbl => lbl != null && lbl.labelName == labelName);
        if (label == null)
        {
            return false;
        }
        label.Detach();
        player.playerState.labelBackpack.Add(label);
        PlayClickSFX();
        return true;
    }
    public void ShowLabelWith(String Type,Dictionary<string, int> Count)
    {
        currentDisplayType = Type;
        Debug.Log("标签：显示" + Type);
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
            Debug.Log($"显示标签: {item.Key} x{item.Value}");
            if (i - startIndex < SlotList.Length) // 确保不超过格子数量
            {
                LinkSlot(i - startIndex, item.Key, item.Value);
            }
        }
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
            Debug.Log($"点击了格子 [{index}]：物品 = {clickedInfo.name}");
            DisplayImage.color = new Color(1, 1, 1, 1); // 设置为不透明
            DisplayImage.sprite = clickedInfo.icon;
            DisplayName.text = clickedInfo.name;
            DisplayDescription.text = clickedInfo.description;
            // 这里可以触发信息面板显示、使用物品等逻辑
        }
        else
        {
            Debug.Log($"格子 [{index}] 为空");
            cleanDisplay();
        }
    }
    void cleanDisplay()
    {
        DisplayImage.color = new Color(1, 1, 1, 0); // 初始为透明
        DisplayName.text = "";
        DisplayDescription.text = "";
    }
}
