using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.PlayerLoop;
public class TagUI : MonoBehaviour
{
    public CanvasGroup TagCanvas;
    public static TagUI Instance;

    public PlayerSmallObject player;

    public GameObject[] SlotList; // 存放物品槽的父对象，假设有 35 个子对象命名为 "Slot1", "Slot2", ..., "Slot35"

    public TMP_Text PageNumber;

    public int currentPage;
    public int totalPages;

    public int itemsPerPage ; // 每页显示的物品数量

    public Image ItemImage; // 显示物品图片的UI组件
    public TMP_Text ItemName; // 显示物品名称的UI组件

    public TMP_Text ItemDescription; // 显示物品描述的UI组件

    private Dictionary<string, int> labelCount = new Dictionary<string, int>();// 物品标签及其数量的字典

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
        TagCanvas.gameObject.SetActive(false);
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
        isOpen = true;
        TagCanvas.gameObject.SetActive(true);
        ShowTag();
    }
    public void Close()
    {
        isOpen = false;
        TagCanvas.gameObject.SetActive(false);
    }
    public void RightPage()
    {
        if(currentPage < totalPages)
        {
            currentPage++;
            ShowTag();
        }
    }
    public void LeftPage()
    {
        if(currentPage > 1)
        {
            currentPage--;
            ShowTag();
        }
    }

    public void UpdatePageNumber()
    {
        PageNumber.text = $"PAGE: {currentPage}/{totalPages}";
    }
    //标签显示方法
    public void UpdateTag()
    {
        UpdatePageNumber();
        List<ObjectLabel> labels = player.playerState.smallObjectLabels;
        // 分页显示逻辑,将列表分成多页，每页显示 itemsPerPage 个物品，相同标签记录数量
        labelCount.Clear();
        foreach (var label in labels)
        {
            if (labelCount.ContainsKey(label.labelName))
            {
                labelCount[label.labelName]++;
            }
            else
            {
                labelCount[label.labelName] = 1;
            }
        }
    }
    public void ShowTag()
    {
        UpdateTag();
        //使用字典显示对应页码的物品标签和数量
        //每页显示 itemsPerPage 个物品，根据 currentPage 计算显示范围
        int startIndex = (currentPage - 1) * itemsPerPage;
        int endIndex = Mathf.Min(startIndex + itemsPerPage, labelCount.Count);
        Debug.Log($"显示: currentPage={currentPage}, startIndex={startIndex}, endIndex={endIndex}, totalItems={labelCount.Count}");
        for (int i = 0; i < SlotList.Length; i++)
        {
            // 子对象命名是 "Slot1", "Slot2", ..., "Slot35"，根据索引清空显示
            SlotList[i].GetComponentInChildren<TMP_Text>().text = "";
        }
        for (int i = startIndex; i < endIndex; i++)
        {
            var item = new List<KeyValuePair<string, int>>(labelCount)[i];
            Debug.Log($"显示物品: {item.Key} x{item.Value}");
            SlotList[i].GetComponentInChildren<TMP_Text>().text = $"{item.Value}";
        }

    }



}


