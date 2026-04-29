using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
public class ActionUI : MonoBehaviour
{
    public CanvasGroup ActionCanvas;
    public bool isOpen = false;
    public static ActionUI Instance;

    public TMP_Text ActionText;
    public Image ActionImage;

    public Button[] Buttons; // 存放选项按钮的数组，假设有4个按钮命名为 "Button1", "Button2", "Button3", "Button4"
    // 重点：UI 内部持有的当前 NPC 引用
    private NPCObject currentNPC;
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
        ActionCanvas.alpha = 0;
        ActionCanvas.interactable = false;
        ActionCanvas.blocksRaycasts = false;
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
        ActionCanvas.alpha = 1;
        ActionCanvas.interactable = true;
        ActionCanvas.blocksRaycasts = true;
        isOpen = true;
    }
    public void Close()
    {
        ActionCanvas.alpha = 0;
        ActionCanvas.interactable = false;
        ActionCanvas.blocksRaycasts = false;
        isOpen = false;
    }
    
    private void OnEnable() {
        // 1. 订阅场景中所有 NPC 的实例事件
        // 当任何 NPC 触发对话时，这个方法会被调用，且参数就是那个 NPC
        var npcs = FindObjectsByType<NPCObject>(FindObjectsSortMode.None);
        foreach (var npc in npcs) {
            npc.OnDialogueStarted += HandleDialogueStart;
            npc.OnDialogueEnded += HandleDialogueEnd;
        }
    }

    private void OnDisable() {
        var npcs = FindObjectsByType<NPCObject>(FindObjectsSortMode.None);
        foreach (var npc in npcs) {
            npc.OnDialogueStarted -= HandleDialogueStart;
            npc.OnDialogueEnded -= HandleDialogueEnd;
        }
    }

    private void HandleDialogueStart(NPCObject npc) {
        // 2. 捕获 NPC 引用
        currentNPC = npc;
        Open();

        // 3. 订阅该特定 NPC 的节点变化事件
        currentNPC.OnNodeChanged += RefreshUI;
        
        RefreshUI();
    }

    private void RefreshUI() {
        if (currentNPC == null) return;

        if (ActionText == null) {
            Debug.LogError("ActionUI 缺少 ActionText 引用，无法刷新对话文本");
            return;
        }

        // 4. 通过接口获取数据并显示
        ActionText.text = currentNPC.GetCurrentContent();
        Sprite npcSprite = currentNPC.GetCurrentSprite();
        if (ActionImage != null && npcSprite != null) {
            ActionImage.sprite = npcSprite;
            ActionImage.gameObject.SetActive(true);
        } else if (ActionImage != null) {
            ActionImage.gameObject.SetActive(false);
        }
        // 5. 处理选项生成
        ClearOptions();
        List<DialogueOption> options = currentNPC.GetCurrentOptions() ?? new List<DialogueOption>();
        if (options.Count > 0) {
            for (int i = 0; i < options.Count; i++) {
                int index = i; // 闭包陷阱处理
                
                //绑定按钮1-4的点击事件
                if (index < Buttons.Length) {
                    Buttons[index].GetComponentInChildren<TMP_Text>().text = options[index].text; // 设置按钮文本
                    Buttons[index].gameObject.SetActive(true);
                    Buttons[index].onClick.RemoveAllListeners();
                    Buttons[index].onClick.AddListener(() => {
                        currentNPC.SelectOption(index);
                    });
                }
            }
        }
        else {
            // 没有选项时隐藏按钮
            for (int i = 0; i < Buttons.Length; i++) {
                if (Buttons[i] != null) {
                    Buttons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    // 6. 处理非选项节点的点击翻页
    public void OnBackgroundClick() {
        List<DialogueOption> options = currentNPC != null ? (currentNPC.GetCurrentOptions() ?? new List<DialogueOption>()) : null;
        if (currentNPC != null && options.Count == 0) {
            currentNPC.AdvanceToNextNode();
        }
    }

    private void HandleDialogueEnd() {
        if (currentNPC != null) {
            currentNPC.OnNodeChanged -= RefreshUI;
        }
        currentNPC = null;
        Close();
    }

    private void ClearOptions() {
        for (int i = 0; i < Buttons.Length; i++) {
            if (Buttons[i] != null) {
                Buttons[i].gameObject.SetActive(false);
            }
        }
    }
    
}
