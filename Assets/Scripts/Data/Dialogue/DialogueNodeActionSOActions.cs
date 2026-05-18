using System;
using UnityEngine;

public static class DialogueNodeActionSOActions {
    public static void Execute(this DialogueNodeActionSO action, NPCObject npc) {
        switch (action.kind) {
            case DialogueActionKind.TriggerReceiver:
                ExecuteTriggerReceiver(action, npc);
                break;
            case DialogueActionKind.GiveItem:
                ExecuteGiveItem(action, npc);
                break;
            case DialogueActionKind.RemoveItems:
                ExecuteRemoveItems(action, npc);
                break;
            default:
                Debug.LogWarning($"未知的 DialogueActionKind: {action.kind}");
                break;
        }
    }

    private static void ExecuteTriggerReceiver(DialogueNodeActionSO action, NPCObject npc) {
        if (string.IsNullOrWhiteSpace(action.receiverId)) {
            Debug.LogWarning("DialogueNodeActionSO TriggerReceiver 缺少 receiverId");
            return;
        }

        var behaviours = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        bool triggered = false;
        foreach (var behaviour in behaviours) {
            if (behaviour is not IDialogueActionReceiver receiver) continue;
            if (!string.Equals(receiver.DialogueActionId, action.receiverId, StringComparison.Ordinal)) continue;

            receiver.ReceiveDialogueAction();
            triggered = true;
        }

        if (!triggered) Debug.LogWarning($"未找到可响应对话动作的对象，receiverId = {action.receiverId}");
    }

    private static void ExecuteGiveItem(DialogueNodeActionSO action, NPCObject npc) {
        if (npc == null || npc.CurrentInteractingPlayer == null) {
            Debug.LogWarning("DialogueNodeActionSO GiveItem 执行失败：没有可用的交互玩家");
            return;
        }

        if (string.IsNullOrWhiteSpace(action.itemName) || action.amount <= 0) {
            Debug.LogWarning("DialogueNodeActionSO GiveItem 配置无效：itemName 为空或 amount 非法");
            return;
        }

        for (int i = 0; i < action.amount; i++) {
            npc.CurrentInteractingPlayer.AddItemToBackpack(new Item { itemName = action.itemName });
        }

        Debug.Log($"对话动作发放物品：{action.itemName} x{action.amount}");
    }

    private static void ExecuteRemoveItems(DialogueNodeActionSO action, NPCObject npc) {
        if (npc == null || npc.CurrentInteractingPlayer == null) {
            Debug.LogWarning("DialogueNodeActionSO RemoveItems 执行失败：没有可用的交互玩家");
            return;
        }

        var player = npc.CurrentInteractingPlayer;
        if (player.playerState == null || player.playerState.itemBackpack == null) {
            Debug.LogWarning("玩家背包为空，无法移除物品");
            return;
        }

        foreach (var name in action.itemsToRemove) {
            if (string.IsNullOrWhiteSpace(name)) continue;

            bool removed = false;
            for (int i = 0; i < player.playerState.itemBackpack.Count; i++) {
                var it = player.playerState.itemBackpack[i];
                if (it != null && string.Equals(it.itemName, name, StringComparison.Ordinal)) {
                    player.RemoveItemFromBackpack(name, 1);
                    removed = true;
                    Debug.Log($"从玩家背包移除了物品: {name}");
                    break;
                }
            }

            if (!removed) Debug.LogWarning($"尝试移除物品但未发现：{name}");
        }
    }
}
