using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ActionUI : FatherUI
{
    public static ActionUI Instance;
    public CanvasGroup ActionCanvas;
    public static bool isOpen = false;
    public TMP_Text ActionText;
    public Image ActionImage;
    public Button[] Buttons; // 存放选项按钮的数组，假设有4个按钮命名为 "Button1", "Button2", "Button3", "Button4"

    // ─── NPC 对话模式 ──────────────────────────────────────
    private NPCObject currentNPC;

    // ─── 结局指认模式 ──────────────────────────────────────
    private bool _isEndingMode;
    private int _currentEndingStage = -1;

    private static readonly string[] EndingStageDescriptions = {
        "助手已自首。你是否相信他就是真凶？\n\n（选择「指认」进入结局A；选择「继续调查」寻找更多线索）",
        "妻子支支吾吾，似乎有所隐瞒。你是否要指认她是凶手？\n\n（选择「指认」进入结局B；选择「继续调查」寻找更多线索）",
        "你已发现凶器是冰弩箭，时间也被动过手脚。现在指认助手吗？\n\n（选择「指认」进入结局C；选择「继续调查」寻找动机）",
        "你找到了助手的原稿和日记。动机似乎清楚了，指认他吗？\n\n（选择「指认」进入结局D；选择「继续调查」挖掘更深真相）",
        "你撕下了「虚伪的」标签，看到了真正的动机。真相只有一个——指认凶手吧。\n\n（选择「指认」进入结局E——真相结局）"
    };

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
        PlayOpenSFX();
        ActionCanvas.alpha = 1;
        ActionCanvas.interactable = true;
        ActionCanvas.blocksRaycasts = true;
        isOpen = true;
    }
    public void Close()
    {
        PlayCloseSFX();
        ActionCanvas.alpha = 0;
        ActionCanvas.interactable = false;
        ActionCanvas.blocksRaycasts = false;
        isOpen = false;
    }
    
    private void OnEnable() {
        // 订阅 NPC 对话事件
        var npcs = FindObjectsByType<NPCObject>(FindObjectsSortMode.None);
        foreach (var npc in npcs) {
            npc.OnDialogueStarted += HandleDialogueStart;
            npc.OnDialogueEnded += HandleDialogueEnd;
        }

        // 订阅第三章结局指认事件
        if (Chapter3_SceneController.Instance != null) {
            Chapter3_SceneController.Instance.EndingChoiceAvailable += ShowEndingChoice;
            Chapter3_SceneController.Instance.EndingTriggered += OnEndingTriggered;
        }
    }
    private void OnDisable() {
        var npcs = FindObjectsByType<NPCObject>(FindObjectsSortMode.None);
        foreach (var npc in npcs) {
            npc.OnDialogueStarted -= HandleDialogueStart;
            npc.OnDialogueEnded -= HandleDialogueEnd;
        }

        if (Chapter3_SceneController.Instance != null) {
            Chapter3_SceneController.Instance.EndingChoiceAvailable -= ShowEndingChoice;
            Chapter3_SceneController.Instance.EndingTriggered -= OnEndingTriggered;
        }
    }

    // ─── NPC 对话模式 ──────────────────────────────────────
    private void HandleDialogueStart(NPCObject npc) {
        if (_isEndingMode) return;

        currentNPC = npc;
        Open();
        currentNPC.OnNodeChanged += RefreshUI;
        RefreshUI();
    }
    private void RefreshUI() {
        if (currentNPC == null) return;
        if (ActionText == null) {
            Debug.LogError("ActionUI 缺少 ActionText 引用，无法刷新对话文本");
            return;
        }
        ActionText.text = currentNPC.GetCurrentContent();
        Sprite npcSprite = currentNPC.GetCurrentSprite();
        if (ActionImage != null && npcSprite != null) {
            ActionImage.preserveAspect = true;
            ActionImage.sprite = npcSprite;
            ActionImage.gameObject.SetActive(true);
        } else if (ActionImage != null) {
            ActionImage.gameObject.SetActive(false);
        }
        ClearOptions();
        List<DialogueOption> options = currentNPC.GetCurrentOptions() ?? new List<DialogueOption>();
        if (options.Count > 0) {
            for (int i = 0; i < options.Count; i++) {
                int index = i;
                if (index < Buttons.Length) {
                    Buttons[index].GetComponentInChildren<TMP_Text>().text = options[index].text;
                    Buttons[index].gameObject.SetActive(true);
                    Buttons[index].onClick.RemoveAllListeners();
                    Buttons[index].onClick.AddListener(() => {
                        currentNPC.SelectOption(index);
                    });
                }
            }
        }
        else {
            for (int i = 0; i < Buttons.Length; i++) {
                if (Buttons[i] != null) {
                    Buttons[i].gameObject.SetActive(false);
                }
            }
        }
    }
    public void OnBackgroundClick() {
        if (_isEndingMode) return;
        List<DialogueOption> options = currentNPC != null ? (currentNPC.GetCurrentOptions() ?? new List<DialogueOption>()) : null;
        if (currentNPC != null && options.Count == 0) {
            currentNPC.AdvanceToNextNode();
            PlayClickSFX();
        }
    }
    private void HandleDialogueEnd() {
        // 结局指认模式由自己管理 UI 状态，不在此处关闭
        if (_isEndingMode) return;
        if (currentNPC != null) {
            currentNPC.OnNodeChanged -= RefreshUI;
        }
        currentNPC = null;
        Close();
    }

    /// <summary>
    /// 强制结束 NPC 对话模式（不触发 EndConversation，只是清理 UI 层状态）
    /// </summary>
    private void ForceEndDialogueMode() {
        if (currentNPC != null) {
            currentNPC.OnNodeChanged -= RefreshUI;
            currentNPC = null;
        }
        Close();
    }
    private void ClearOptions() {
        for (int i = 0; i < Buttons.Length; i++) {
            if (Buttons[i] != null) {
                Buttons[i].gameObject.SetActive(false);
            }
        }
    }

    // ─── 结局指认模式（复用同一套 ActionUI 面板） ──────────
    public void ShowEndingChoice(int stage) {
        if (_isEndingMode) return;
        if (stage < 0 || stage >= EndingStageDescriptions.Length) return;

        // 如果 NPC 对话正在打开，先强制结束对话模式
        // （时序说明：退出对话节点的 exit action 触发此事件时，
        //  EndConversation() 尚未执行，所以对话仍处于打开状态）
        if (isOpen && currentNPC != null) {
            Debug.Log("ActionUI: 对话中收到结局指认请求，强制结束对话模式以显示结局选择。");
            ForceEndDialogueMode();
        }
        if (isOpen) return;

        _isEndingMode = true;
        _currentEndingStage = stage;

        if (ActionText != null) {
            ActionText.text = EndingStageDescriptions[stage];
        }
        if (ActionImage != null) {
            ActionImage.gameObject.SetActive(false);
        }

        ClearOptions();
        if (Buttons.Length >= 2) {
            Buttons[0].GetComponentInChildren<TMP_Text>().text = "指认凶手";
            Buttons[0].gameObject.SetActive(true);
            Buttons[0].onClick.RemoveAllListeners();
            Buttons[0].onClick.AddListener(OnAccuseClicked);

            // 最后一个结局（stage 4 = 真相）不允许继续调查
            bool isFinalStage = stage == 4;
            if (!isFinalStage) {
                Buttons[1].GetComponentInChildren<TMP_Text>().text = "继续调查";
                Buttons[1].gameObject.SetActive(true);
                Buttons[1].onClick.RemoveAllListeners();
                Buttons[1].onClick.AddListener(OnContinueClicked);
            } else {
                Buttons[1].gameObject.SetActive(false);
            }
        }

        Open();
        Time.timeScale = 0f;
    }

    private void OnAccuseClicked() {
        if (!_isEndingMode || _currentEndingStage < 0) return;
        if (Chapter3_SceneController.Instance == null) return;
        Chapter3_SceneController.Instance.AccuseAtStage(_currentEndingStage);
    }

    private void OnContinueClicked() {
        if (!_isEndingMode || _currentEndingStage < 0) return;
        if (Chapter3_SceneController.Instance == null) return;
        Chapter3_SceneController.Instance.ContinueInvestigation(_currentEndingStage);
        CloseEndingMode();
    }

    private void CloseEndingMode() {
        _isEndingMode = false;
        _currentEndingStage = -1;
        Close();
        Time.timeScale = 1f;
    }

    private void OnEndingTriggered(int endingIndex) {
        CloseEndingMode();
        // TODO：根据 endingIndex 显示结局画面或过场动画
        Debug.Log($"ActionUI: 结局 {endingIndex} 已触发，准备显示结局内容。");
        EndUI.PendingEndingId = endingIndex;
        // 切换到结局展示场景（假设场景名为 "EndingScene"）
        UnityEngine.SceneManagement.SceneManager.LoadScene("EndScene");

    }
}
