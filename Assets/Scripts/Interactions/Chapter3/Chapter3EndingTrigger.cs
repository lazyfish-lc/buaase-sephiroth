using UnityEngine;

/// <summary>
/// 第三章结局/事件触发点 — 实现 IDialogueActionReceiver，
/// 通过 DialogueNodeActionSO.TriggerReceiver 机制被对话节点调用。
/// 
/// 用法：
///   "chapter3/ending_choice_{0-4}" → 弹出对应阶段的指认选择
///   "chapter3/diary_talked"        → 触发日记对话完毕事件（推进阶段 3/4）
/// </summary>
public class Chapter3EndingTrigger : MonoBehaviour, IDialogueActionReceiver {
    [Header("结局触发配置")]
    [SerializeField, Tooltip("对应的调查阶段 (0=大厅, 1=妻子, 2=作家, 3=助手, 4=真相)\n仅在 ending_choice 模式下使用")]
    private int stageIndex;

    [SerializeField, Tooltip("receiverId，格式:\n  chapter3/ending_choice_{0-4}  弹出指认选择\n  chapter3/diary_talked        触发日记事件")]
    private string receiverId;

    public string DialogueActionId => receiverId;

    private void Awake() {
        if (string.IsNullOrWhiteSpace(receiverId)) {
            receiverId = $"chapter3/ending_choice_{stageIndex}";
        }
    }

    public void ReceiveDialogueAction() {
        if (Chapter3_SceneController.Instance == null) {
            Debug.LogWarning($"Chapter3EndingTrigger: 场景中不存在 Chapter3_SceneController");
            return;
        }

        // ─── 模式1: diary_talked — 触发日记对话完毕事件 ────
        if (receiverId == "chapter3/diary_talked") {
            Debug.Log("Chapter3EndingTrigger: 触发 diary_talked 事件。");
            Chapter3Events.RaiseDiaryTalked();
            return;
        }

        // ─── 模式2: ending_choice — 弹出指认选择 ──────────
        if (Chapter3_SceneController.Instance.HasEndingTriggered) {
            Debug.Log("Chapter3EndingTrigger: 结局已触发，跳过。");
            return;
        }

        Debug.Log($"Chapter3EndingTrigger: 被触发，请求阶段 {stageIndex} 的指认选择。");
        Chapter3_SceneController.Instance.TryOfferEndingChoiceExternal(stageIndex);
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (string.IsNullOrWhiteSpace(receiverId)) {
            receiverId = $"chapter3/ending_choice_{stageIndex}";
        }
    }
#endif
}
