using UnityEngine;
using System.Collections.Generic;
using System;

public class NPCObject : SmallObject {
    public static NPCObject ActiveNPC { get; private set; }

    // 供 UI 监听的事件
    public event Action<NPCObject> OnDialogueStarted;
    public event Action OnDialogueEnded;
    public event Action OnNodeChanged;

    private NPCStaticData NPCData => (NPCStaticData)staticData;
    private NPCDynamicState NPCState => (NPCDynamicState)dynamicState;

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new NPCDynamicState();
    }

    // --- 给 UI 调用的数据接口 (Getter) ---
    public string GetCurrentContent() => NPCData.dialogueNodes[NPCState.currentNodeIndex].npcContent;
    public List<DialogueOption> GetCurrentOptions() => NPCData.dialogueNodes[NPCState.currentNodeIndex].options;
    public bool CurrentNodeHasOptions() => NPCData.dialogueNodes[NPCState.currentNodeIndex].hasOptions;
    public bool IsInConversation() => NPCState.isInConversation;

    // --- 核心交互逻辑入口 ---
    public override void OnInteractAction(InputEventData data) {
        // 假设 InputConfig 映射：Interact 对应键盘E，value == 1 对应鼠标右键
        if (data.actionType == InputActionType.Interact) {
            float dist = Vector3.Distance(transform.position, IOSubsystem.Instance.playerObject.transform.position);
            if (dist <= NPCData.interactionRange) {
                Debug.Log("玩家尝试与NPC交互，距离合法，进入对话");
                if (!NPCState.isInConversation) StartConversation();
            } else {
                Debug.Log("玩家尝试与NPC交互，但距离过远");
            }
        }
    }

    private void StartConversation() {
        NPCState.isInConversation = true;
        NPCState.currentNodeIndex = 0;
        NPCState.currentInteractingPlayer = IOSubsystem.Instance.playerObject;
        ActiveNPC = this;
        
        OnDialogueStarted?.Invoke(this);
        OnNodeChanged?.Invoke();
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
    public void SelectOption(int optionIndex) {
        if (!NPCState.isInConversation) return;

        DialogueOption option = GetCurrentOptions()[optionIndex];

        // 核心玩法1：物品/数值赠予 (利用数值守恒系统)
        if (!string.IsNullOrEmpty(option.rewardProperty)) {
            NPCState.currentInteractingPlayer.AddItemToBackpack(new Item { itemName = option.rewardProperty });
        }

        TransitionToNode(option.targetNodeIndex);
    }

    private void TransitionToNode(int nodeIndex) {
        if (nodeIndex == -1) {
            EndConversation();
        } else {
            NPCState.currentNodeIndex = nodeIndex;
            OnNodeChanged?.Invoke();
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
        if (NPCState.isInConversation) {
            float dist = Vector3.Distance(transform.position, NPCState.currentInteractingPlayer.transform.position);
            if (dist > NPCData.interactionRange) {
                EndConversation();
            }
        }
    }
}