using System.Collections.Generic;
using UnityEngine;

// 单一的可配置对话动作 SO（避免使用继承，改用枚举区分类型）
[CreateAssetMenu(fileName = "DialogueNodeAction", menuName = "Game/Dialogue/Node Action")]
public class DialogueNodeActionSO : ScriptableObject {
    public DialogueActionKind kind = DialogueActionKind.TriggerReceiver;

    [Header("Trigger Receiver (when kind == TriggerReceiver)")]
    public string receiverId;

    [Header("Give Item (when kind == GiveItem)")]
    public string itemName;
    public  ItemType itemType;
    [Header("Recover Item Specific (when itemType == Recover)")]
    public float recoverAmount; // 仅当 itemType == ItemType.Recover 时有效
    public int amount = 1;
    [Header("Remove Items (when kind == RemoveItems)")]
    public List<string> itemsToRemove = new List<string>();

    [Header("UI Actions (when kind == UIActions)")]
    public string uiActionId;
    public TutorialContent tutorialContent = new TutorialContent();
    public bool hasShown;
}