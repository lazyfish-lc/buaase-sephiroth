using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class NPCObject : SmallObject {
    public static NPCObject ActiveNPC { get; private set; }

    // 供 UI 监听的事件
    public event Action<NPCObject> OnDialogueStarted;
    public event Action OnDialogueEnded;
    public event Action OnNodeChanged;

    protected NPCStaticData NPCData => (NPCStaticData)staticData;
    protected NPCDynamicState NPCState => (NPCDynamicState)dynamicState;
    public PlayerSmallObject CurrentInteractingPlayer => NPCState.currentInteractingPlayer;

    public NPCView view;

    // 运行时对话副本（标签挂载时会覆盖这些值）
    private List<DialogueNode> runtimeDialogueNodes;
    private List<NPCStaticData.RequiredItemGroup> runtimeRequiredItemGroups;

    /// <summary>
    /// 供外部（如标签）访问原始 NPCData
    /// </summary>
    public NPCStaticData GetNPCData() {
        return NPCData;
    }

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new NPCDynamicState();
    }

    protected override void Awake() {
        // 1. 先初始化运行时对话副本（只在未初始化时）
        // 这确保在标签附加之前，运行时副本已存在
        if (runtimeDialogueNodes == null) {
            InitializeRuntimeDialogueData();
        }

        // 2. 调用 base.Awake 加载并附加标签
        // 标签的 OnAttach() 会调用 SetRuntimeDialogueNodes() 覆盖运行时副本
        base.Awake();

        // 3. 初始化动画表现（如果未被标签覆盖）
        if (staticData != null && view != null) {
            view.SetController(NPCData.animatorController);
        }

        // 4. 确保所有标签加载完成后，UI 被正确刷新
        // 标签已在 OnAttach() 中调用 ApplyOverride()，ApplyOverride() 现在总是调用 TriggerNodeChanged()
        // 这里再次确保外观状态完整无误
        if (dynamicState?.smallObjectLabels != null && dynamicState.smallObjectLabels.Count > 0) {
            // 再次触发以确保 UI 完全同步
            TriggerNodeChanged();
        }
    }

    /// <summary>
    /// 初始化运行时对话副本（从 NPCData 深拷贝）
    /// 只在未初始化时进行初始化，避免覆盖已应用的标签覆盖
    /// </summary>
    private void InitializeRuntimeDialogueData() {
        // 只在未初始化时才执行初始化
        if (runtimeDialogueNodes != null || runtimeRequiredItemGroups != null) {
            return;
        }

        // 深拷贝对话节点
        if (NPCData != null && NPCData.dialogueNodes != null) {
            runtimeDialogueNodes = new List<DialogueNode>();
            foreach (var node in NPCData.dialogueNodes) {
                runtimeDialogueNodes.Add(CopyDialogueNode(node));
            }
        } else {
            runtimeDialogueNodes = new List<DialogueNode>();
        }

        // 深拷贝必需物品组
        if (NPCData != null && NPCData.requiredItemGroups != null) {
            runtimeRequiredItemGroups = new List<NPCStaticData.RequiredItemGroup>();
            foreach (var group in NPCData.requiredItemGroups) {
                runtimeRequiredItemGroups.Add(CopyRequiredItemGroup(group));
            }
        } else {
            runtimeRequiredItemGroups = new List<NPCStaticData.RequiredItemGroup>();
        }
    }

    /// <summary>
    /// 获取当前使用的对话列表（运行时副本）
    /// </summary>
    public List<DialogueNode> GetCurrentDialogueNodes() {
        return runtimeDialogueNodes ?? new List<DialogueNode>();
    }

    /// <summary>
    /// 获取当前使用的必需物品组列表（运行时副本）
    /// </summary>
    private List<NPCStaticData.RequiredItemGroup> GetCurrentRequiredItemGroups() {
        return runtimeRequiredItemGroups ?? new List<NPCStaticData.RequiredItemGroup>();
    }

    /// <summary>
    /// 供标签调用：设置运行时对话列表
    /// </summary>
    public void SetRuntimeDialogueNodes(List<DialogueNode> nodes) {
        runtimeDialogueNodes = nodes != null ? new List<DialogueNode>(nodes) : new List<DialogueNode>();
    }

    /// <summary>
    /// 供标签调用：设置运行时必需物品组列表
    /// </summary>
    public void SetRuntimeRequiredItemGroups(List<NPCStaticData.RequiredItemGroup> groups) {
        runtimeRequiredItemGroups = groups != null ? new List<NPCStaticData.RequiredItemGroup>(groups) : new List<NPCStaticData.RequiredItemGroup>();
    }

    /// <summary>
    /// 供标签调用：设置当前节点索引
    /// </summary>
    public void SetCurrentNodeIndex(int index) {
        NPCState.currentNodeIndex = index;
    }

    /// <summary>
    /// 供标签调用：触发节点变化事件（用于 UI 刷新）
    /// </summary>
    public void TriggerNodeChanged() {
        OnNodeChanged?.Invoke();
    }

    // --- 给 UI 调用的数据接口 (Getter) ---
    public string GetCurrentContent() {
        var nodes = GetCurrentDialogueNodes();
        if (NPCState.currentNodeIndex < 0 || NPCState.currentNodeIndex >= nodes.Count) return "";
        return nodes[NPCState.currentNodeIndex].npcContent;
    }

    public Sprite GetCurrentSprite() => gameObject.GetComponent<SpriteRenderer>()?.sprite;
    
    public List<DialogueOption> GetCurrentOptions() {
        var nodes = GetCurrentDialogueNodes();
        if (NPCState.currentNodeIndex < 0 || NPCState.currentNodeIndex >= nodes.Count) return new List<DialogueOption>();
        return nodes[NPCState.currentNodeIndex].options ?? new List<DialogueOption>();
    }
    
    public bool CurrentNodeHasOptions() {
        var nodes = GetCurrentDialogueNodes();
        if (NPCState.currentNodeIndex < 0 || NPCState.currentNodeIndex >= nodes.Count) return false;
        return nodes[NPCState.currentNodeIndex].hasOptions;
    }
    public bool IsInConversation() => NPCState.isInConversation;
    public int GetCurrentNodeIndex() => NPCState.currentNodeIndex;

    public void ApplySaveState(bool isInConversation, int nodeIndex, PlayerSmallObject player) {
        NPCState.isInConversation = isInConversation;

        int maxIndex = 0;
        var nodes = GetCurrentDialogueNodes();
        if (nodes != null && nodes.Count > 0) {
            maxIndex = nodes.Count - 1;
        }
        NPCState.currentNodeIndex = Mathf.Clamp(nodeIndex, 0, maxIndex);

        NPCState.currentInteractingPlayer = isInConversation ? player : null;
        ActiveNPC = isInConversation ? this : null;
    }

    // --- 核心交互逻辑入口 ---
    public override void OnInteractAction(InputEventData data) {
        // 假设 InputConfig 映射：Interact 对应键盘E，value == 1 对应鼠标右键
        if (data.actionType == InputActionType.Interact) {
            TryStartConversation();
        }
    }

    protected virtual bool TryStartConversation() {
        if (NPCState.isInConversation) return false;

        float dist = Vector3.Distance(transform.position, IOSubsystem.Instance.playerObject.transform.position);
        if (dist > NPCData.interactionRange) {
            Debug.Log("玩家尝试与NPC交互，但距离过远");
            return false;
        }

        Debug.Log("玩家尝试与NPC交互，距离合法，进入对话");
        StartConversation();
        return true;
    }

    protected virtual void StartConversation() {
        NPCState.isInConversation = true;
        NPCState.currentInteractingPlayer = IOSubsystem.Instance.playerObject;
        ActiveNPC = this;

        NPCState.currentNodeIndex = ResolveStartNodeIndex();

        OnDialogueStarted?.Invoke(this);
        ExecuteCurrentNodeActions();
        OnNodeChanged?.Invoke();
    }

    protected virtual int ResolveStartNodeIndex() {
        // 如果配置了 requiredItems，则检查玩家是否满足条件以决定起始节点
        int startIndex = 0;
        var groups = GetCurrentRequiredItemGroups();
        if (groups != null && groups.Count > 0) {
            var player = NPCState.currentInteractingPlayer;
            foreach (var group in groups) {
                if (group == null || group.requiredItems == null || group.requiredItems.Count == 0) continue;

                bool hasAll = true;
                if (player != null) {
                    foreach (var req in group.requiredItems) {
                        if (!player.HasItemInBackpack(req)) {
                            hasAll = false;
                            break;
                        }
                    }
                } else {
                    hasAll = false;
                }

                if (hasAll && group.startNodeIfHasRequiredItems >= 0) startIndex = group.startNodeIfHasRequiredItems;
            }
        }

        return startIndex;
    }

    // --- 给 UI 调用的逻辑接口 (Setter/Trigger) ---

    /// <summary>
    /// UI在无选项状态下，玩家点击屏幕时调用
    /// </summary>
    public void AdvanceToNextNode() {
        if (!NPCState.isInConversation || CurrentNodeHasOptions()) return;

        var nodes = GetCurrentDialogueNodes();
        if (NPCState.currentNodeIndex < 0 || NPCState.currentNodeIndex >= nodes.Count) return;

        int next = nodes[NPCState.currentNodeIndex].nextNodeIndex;
        TransitionToNode(next);
    }

    /// <summary>
    /// UI在有选项状态下，玩家点击具体按钮时调用
    /// </summary>
    public virtual void SelectOption(int optionIndex) {
        if (!NPCState.isInConversation) return;

        DialogueOption option = GetCurrentOptions()[optionIndex];

        // 核心玩法1：物品/数值赠予 (利用数值守恒系统)
        if (!string.IsNullOrEmpty(option.rewardProperty)) {
            NPCState.currentInteractingPlayer.AddItemToBackpack(Item.Create(option.rewardProperty));
        }

        TransitionToNode(option.targetNodeIndex);
    }

    protected void TransitionToNode(int nodeIndex) {
        var nodes = GetCurrentDialogueNodes();
        if (NPCState.currentNodeIndex >= 0 && NPCState.currentNodeIndex < nodes.Count) {
            foreach (var action in nodes[NPCState.currentNodeIndex].exitActions) {
                action?.Execute(this);
            }
        }
        
        if (nodeIndex == -1) {
            EndConversation();
        } else {
            NPCState.currentNodeIndex = nodeIndex;
            ExecuteCurrentNodeActions();
            OnNodeChanged?.Invoke();
        }
    }

    protected void ExecuteCurrentNodeActions() {
        var nodes = GetCurrentDialogueNodes();
        if (nodes == null) return;
        if (NPCState.currentNodeIndex < 0 || NPCState.currentNodeIndex >= nodes.Count) return;

        DialogueNode node = nodes[NPCState.currentNodeIndex];
        if (node == null || node.enterActions == null) return;

        foreach (var action in node.enterActions) {
            action?.Execute(this);
        }
    }

    private void EndConversation() {
        if (!NPCState.isInConversation) return;
        NPCState.isInConversation = false;
        NPCState.currentInteractingPlayer = null;
        OnDialogueEnded?.Invoke();
    }

    // --- 距离检测：玩家走远自动关闭 ---
    private void Update() {
        if (NPCState.isInConversation && NPCState.currentInteractingPlayer != null) {
            float dist = Vector3.Distance(transform.position, NPCState.currentInteractingPlayer.transform.position);
            if (dist > NPCData.interactionRange) {
                EndConversation();
            }
        }
    }

    /// <summary>
    /// 深拷贝一个对话节点
    /// </summary>
    private DialogueNode CopyDialogueNode(DialogueNode original) {
        if (original == null) return null;

        return new DialogueNode {
            npcContent = original.npcContent,
            hasOptions = original.hasOptions,
            nextNodeIndex = original.nextNodeIndex,
            options = original.options != null ? new List<DialogueOption>(original.options) : new List<DialogueOption>(),
            enterActions = original.enterActions != null ? new List<DialogueNodeActionSO>(original.enterActions) : new List<DialogueNodeActionSO>(),
            exitActions = original.exitActions != null ? new List<DialogueNodeActionSO>(original.exitActions) : new List<DialogueNodeActionSO>()
        };
    }

    /// <summary>
    /// 深拷贝一个必需物品组
    /// </summary>
    private NPCStaticData.RequiredItemGroup CopyRequiredItemGroup(NPCStaticData.RequiredItemGroup original) {
        if (original == null) return null;

        return new NPCStaticData.RequiredItemGroup {
            requiredItems = original.requiredItems != null ? new List<string>(original.requiredItems) : new List<string>(),
            startNodeIfHasRequiredItems = original.startNodeIfHasRequiredItems
        };
    }
}