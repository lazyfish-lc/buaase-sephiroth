using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 外观与对话的覆盖配置。
/// 当标签挂载到 NPCObject 时，会使用此配置中的内容替换 NPC 的外观（Sprite、Animator）和对话（dialogueNodes、requiredItemGroups）。
/// </summary>
[CreateAssetMenu(fileName = "NPCAppearanceOverride", menuName = "Game/NPCAppearanceOverride")]
public class NPCAppearanceOverride : ScriptableObject {
    
    [Header("外观覆盖")]
    public Sprite overrideSprite;
    public RuntimeAnimatorController overrideAnimatorController;

    [Header("对话覆盖")]
    public List<DialogueNode> overrideDialogueNodes = new List<DialogueNode>();
    public List<NPCStaticData.RequiredItemGroup> overrideRequiredItemGroups = new List<NPCStaticData.RequiredItemGroup>();

    /// <summary>
    /// 深拷贝对话节点列表。
    /// </summary>
    public List<DialogueNode> GetCopiedDialogueNodes() {
        if (overrideDialogueNodes == null) return new List<DialogueNode>();
        
        var copied = new List<DialogueNode>();
        foreach (var node in overrideDialogueNodes) {
            copied.Add(CopyDialogueNode(node));
        }
        return copied;
    }

    /// <summary>
    /// 深拷贝必需物品组列表。
    /// </summary>
    public List<NPCStaticData.RequiredItemGroup> GetCopiedRequiredItemGroups() {
        if (overrideRequiredItemGroups == null) return new List<NPCStaticData.RequiredItemGroup>();
        
        var copied = new List<NPCStaticData.RequiredItemGroup>();
        foreach (var group in overrideRequiredItemGroups) {
            copied.Add(CopyRequiredItemGroup(group));
        }
        return copied;
    }

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

    private NPCStaticData.RequiredItemGroup CopyRequiredItemGroup(NPCStaticData.RequiredItemGroup original) {
        if (original == null) return null;

        return new NPCStaticData.RequiredItemGroup {
            requiredItems = original.requiredItems != null ? new List<string>(original.requiredItems) : new List<string>(),
            startNodeIfHasRequiredItems = original.startNodeIfHasRequiredItems
        };
    }
}
