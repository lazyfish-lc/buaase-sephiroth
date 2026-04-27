using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ActionUI : MonoBehaviour
{
    public CanvasGroup ActionCanvas;
    public static ActionUI Instance;

    public TMP_Text ActionText;
    public Image ActionImage;

    public Button Button1;
    public Button Button2;
    public Button Button3;
    public Button Button4;
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
        ActionCanvas.alpha = 0;
        ActionCanvas.interactable = false;
        ActionCanvas.blocksRaycasts = false;
    }
    public void OpenAndClose()
    {
        
        if(ActionCanvas.alpha == 0)
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
    }
    public void Close()
    {
        ActionCanvas.alpha = 0;
        ActionCanvas.interactable = false;
        ActionCanvas.blocksRaycasts = false;
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

        // 4. 通过接口获取数据并显示
        ActionText.text = currentNPC.GetCurrentContent();
        Sprite npcSprite = currentNPC.GetCurrentSprite();
        if (npcSprite != null) {
            ActionImage.sprite = npcSprite;
            ActionImage.gameObject.SetActive(true);
        } else {
            ActionImage.gameObject.SetActive(false);
        }
        // 5. 处理选项生成
        ClearOptions();
        if (currentNPC.GetCurrentOptions().Count > 0) {
            var options = currentNPC.GetCurrentOptions();
            for (int i = 0; i < options.Count; i++) {
                int index = i; // 闭包陷阱处理
                
                //绑定按钮1-4的点击事件
                switch (index)
                {
                    case 0:
                        Button1.onClick.RemoveAllListeners();
                        Button1.onClick.AddListener(() => currentNPC.SelectOption(index));
                        Button1.gameObject.SetActive(true);
                        break;
                    case 1:
                        Button2.onClick.RemoveAllListeners();
                        Button2.onClick.AddListener(() => currentNPC.SelectOption(index));
                        Button2.gameObject.SetActive(true);
                        break;
                    case 2:
                        Button3.onClick.RemoveAllListeners();
                        Button3.onClick.AddListener(() => currentNPC.SelectOption(index));
                        Button3.gameObject.SetActive(true);
                        break;
                    case 3:
                        Button4.onClick.RemoveAllListeners();
                        Button4.onClick.AddListener(() => currentNPC.SelectOption(index));
                        Button4.gameObject.SetActive(true);
                        break;
                }
            }
        }
        else {
            // 没有选项时隐藏按钮
            Button1.gameObject.SetActive(false);
            Button2.gameObject.SetActive(false);
            Button3.gameObject.SetActive(false);
            Button4.gameObject.SetActive(false);
        }
    }

    // 6. 处理非选项节点的点击翻页
    public void OnBackgroundClick() {
        if (currentNPC != null && currentNPC.GetCurrentOptions().Count == 0) {
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
        Button1.gameObject.SetActive(false);
        Button2.gameObject.SetActive(false);
        Button3.gameObject.SetActive(false);
        Button4.gameObject.SetActive(false);
    }
    public void Button1Click()
    {
        Debug.Log("Button 1 Clicked");
        // 在这里添加按钮1的功能逻辑，例如与当前NPC交互
        // if (currentNPC != null) {
        //     currentNPC.Interact();
        // }
    }
    public void Button2Click()
    {
        Debug.Log("Button 2 Clicked");
        // 在这里添加按钮2的功能逻辑，例如打开一个新的UI界面
    }
    public void Button3Click()
    {
        Debug.Log("Button 3 Clicked");
        // 在这里添加按钮3的功能逻辑，例如执行一个特殊技能
    }
    public void Button4Click()
    {
        Debug.Log("Button 4 Clicked");
        // 在这里添加按钮4的功能逻辑，例如显示当前任务信息
    }
    
    
    
}
