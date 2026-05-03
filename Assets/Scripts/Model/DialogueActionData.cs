using System.Collections.Generic;
using UnityEngine;

// 能够被对话动作触发的对象统一实现此接口。
// 该接口的动作方法无参数、无返回值，保证调用形式统一。
public interface IDialogueActionReceiver {
    string DialogueActionId { get; }
    void ReceiveDialogueAction();
}

public enum DialogueActionKind {
    TriggerReceiver = 0,
    GiveItem = 1
}

// 单一的可配置对话动作 SO（避免使用继承，改用枚举区分类型）
[CreateAssetMenu(fileName = "DialogueNodeAction", menuName = "Game/Dialogue/Node Action")]
public class DialogueNodeActionSO : ScriptableObject {
    public DialogueActionKind kind = DialogueActionKind.TriggerReceiver;

    [Header("Trigger Receiver (when kind == TriggerReceiver)")]
    public string receiverId;

    [Header("Give Item (when kind == GiveItem)")]
    public string itemName;
    public int amount = 1;

    // 执行动作：由枚举分支决定行为
    public void Execute(NPCObject npc) {
        switch (kind) {
            case DialogueActionKind.TriggerReceiver:
                ExecuteTriggerReceiver(npc);
                break;
            case DialogueActionKind.GiveItem:
                ExecuteGiveItem(npc);
                break;
            default:
                Debug.LogWarning($"未知的 DialogueActionKind: {kind}");
                break;
        }
    }

    private void ExecuteTriggerReceiver(NPCObject npc) {
        if (string.IsNullOrWhiteSpace(receiverId)) {
            Debug.LogWarning("DialogueNodeActionSO TriggerReceiver 缺少 receiverId");
            return;
        }

        var behaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool triggered = false;
        foreach (var behaviour in behaviours) {
            if (behaviour is not IDialogueActionReceiver receiver) continue;
            if (!string.Equals(receiver.DialogueActionId, receiverId, System.StringComparison.Ordinal)) continue;

            receiver.ReceiveDialogueAction();
            triggered = true;
        }

        if (!triggered) Debug.LogWarning($"未找到可响应对话动作的对象，receiverId = {receiverId}");
    }

    private void ExecuteGiveItem(NPCObject npc) {
        if (npc == null || npc.CurrentInteractingPlayer == null) {
            Debug.LogWarning("DialogueNodeActionSO GiveItem 执行失败：没有可用的交互玩家");
            return;
        }

        if (string.IsNullOrWhiteSpace(itemName) || amount <= 0) {
            Debug.LogWarning("DialogueNodeActionSO GiveItem 配置无效：itemName 为空或 amount 非法");
            return;
        }

        for (int i = 0; i < amount; i++) {
            npc.CurrentInteractingPlayer.AddItemToBackpack(new Item { itemName = itemName });
        }

        Debug.Log($"对话动作发放物品：{itemName} x{amount}");
    }
}