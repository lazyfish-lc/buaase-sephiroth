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

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new NPCDynamicState();
    }

    protected override void Awake() {
        base.Awake();

        // 2. 核心修改：从静态数据初始化动画表现
        if (staticData != null && view != null) {
            view.SetController(NPCData.animatorController);
        }
    }

    // --- 给 UI 调用的数据接口 (Getter) ---
    public string GetCurrentContent() => NPCData.dialogueNodes[NPCState.currentNodeIndex].npcContent;

    public Sprite GetCurrentSprite() => gameObject.GetComponent<SpriteRenderer>()?.sprite;
    public List<DialogueOption> GetCurrentOptions() => NPCData.dialogueNodes[NPCState.currentNodeIndex].options;
    public bool CurrentNodeHasOptions() => NPCData.dialogueNodes[NPCState.currentNodeIndex].hasOptions;
    public bool IsInConversation() => NPCState.isInConversation;
    public int GetCurrentNodeIndex() => NPCState.currentNodeIndex;

    public void ApplySaveState(bool isInConversation, int nodeIndex, PlayerSmallObject player) {
        NPCState.isInConversation = isInConversation;

        int maxIndex = 0;
        if (NPCData != null && NPCData.dialogueNodes != null && NPCData.dialogueNodes.Count > 0) {
            maxIndex = NPCData.dialogueNodes.Count - 1;
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
        if (NPCData != null && NPCData.requiredItemGroups != null && NPCData.requiredItemGroups.Count > 0) {
            var player = NPCState.currentInteractingPlayer;
            foreach (var group in NPCData.requiredItemGroups) {
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

        int next = NPCData.dialogueNodes[NPCState.currentNodeIndex].nextNodeIndex;
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
            NPCState.currentInteractingPlayer.AddItemToBackpack(new Item { itemName = option.rewardProperty });
        }

        TransitionToNode(option.targetNodeIndex);
    }

    protected void TransitionToNode(int nodeIndex) {
        if (nodeIndex == -1) {
            EndConversation();
        } else {
            NPCState.currentNodeIndex = nodeIndex;
            ExecuteCurrentNodeActions();
            OnNodeChanged?.Invoke();
        }
    }

    protected void ExecuteCurrentNodeActions() {
        if (NPCData == null || NPCData.dialogueNodes == null) return;
        if (NPCState.currentNodeIndex < 0 || NPCState.currentNodeIndex >= NPCData.dialogueNodes.Count) return;

        DialogueNode node = NPCData.dialogueNodes[NPCState.currentNodeIndex];
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
}