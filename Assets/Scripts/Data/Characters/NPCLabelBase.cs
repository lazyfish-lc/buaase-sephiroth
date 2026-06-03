using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// NPC 标签基类：用于修改 NPCObject 的外观和对话。
/// 
/// 挂载时：
/// - 缓存当前 NPC 的原始外观（SpriteRenderer、Animator）和对话数据
/// - 将 NPC 的运行时数据替换为该标签提供的覆盖值
/// - 如果同类型标签已存在，先卸载旧标签，再挂载新标签（单一生效规则）
/// 
/// 卸载时：
/// - 恢复 NPC 的原始外观和对话数据
/// - 如果对话正进行中，确保节点索引合法
/// </summary>
[Serializable]
public class NPCLabelBase : ObjectLabel {
    [SerializeField]
    protected NPCAppearanceOverride overrideConfig;

    // 缓存的原始状态
    [System.NonSerialized]
    private Sprite cachedOriginalSprite;
    
    [System.NonSerialized]
    private RuntimeAnimatorController cachedOriginalAnimator;
    
    [System.NonSerialized]
    private List<DialogueNode> cachedOriginalDialogueNodes;
    
    [System.NonSerialized]
    private List<NPCStaticData.RequiredItemGroup> cachedOriginalRequiredItemGroups;

    public NPCLabelBase() { }

    public NPCLabelBase(NPCAppearanceOverride config) {
        overrideConfig = config;
    }

    public override string labelName => this.GetType().Name;

    public override void OnAttach(ILabelOwner owner) {
        base.OnAttach(owner);

        var npc = owner as NPCObject;
        if (npc == null || overrideConfig == null) return;

        // 检查是否已存在同类型的标签，如果存在则先卸载它
        RemoveExistingSameLabelType(npc);

        // 缓存原始状态
        CacheOriginalState(npc);

        // 应用覆盖
        ApplyOverride(npc);
    }

    public override void OnDetach(ILabelOwner owner) {
        base.OnDetach(owner);

        var npc = owner as NPCObject;
        if (npc == null) return;

        // 恢复原始状态
        RestoreOriginalState(npc);
    }

    /// <summary>
    /// 移除该 NPC 上类型相同的其他标签（单一生效规则）
    /// </summary>
    private void RemoveExistingSameLabelType(NPCObject npc) {
        if (npc.dynamicState == null || npc.dynamicState.smallObjectLabels == null) return;

        string thisLabelType = this.GetType().Name;
        var labelsToRemove = new List<ObjectLabel>();

        foreach (var label in npc.dynamicState.smallObjectLabels) {
            if (label != null && label.GetType().Name == thisLabelType && label != this) {
                labelsToRemove.Add(label);
            }
        }

        foreach (var label in labelsToRemove) {
            npc.RemoveLabel(label);
        }
    }

    /// <summary>
    /// 缓存 NPC 的原始外观和对话数据
    /// </summary>
    private void CacheOriginalState(NPCObject npc) {
        // 缓存原始 Sprite
        var spriteRenderer = npc.gameObject.GetComponent<SpriteRenderer>();
        cachedOriginalSprite = spriteRenderer != null ? spriteRenderer.sprite : null;

        // 缓存原始 Animator Controller
        var npcData = npc.GetNPCData();
        cachedOriginalAnimator = npcData != null ? npcData.animatorController : null;

        // 缓存原始对话列表（深拷贝）
        if (npcData != null && npcData.dialogueNodes != null) {
            cachedOriginalDialogueNodes = new List<DialogueNode>();
            foreach (var node in npcData.dialogueNodes) {
                cachedOriginalDialogueNodes.Add(CopyDialogueNode(node));
            }
        }

        // 缓存原始 RequiredItemGroups（深拷贝）
        if (npcData != null && npcData.requiredItemGroups != null) {
            cachedOriginalRequiredItemGroups = new List<NPCStaticData.RequiredItemGroup>();
            foreach (var group in npcData.requiredItemGroups) {
                cachedOriginalRequiredItemGroups.Add(CopyRequiredItemGroup(group));
            }
        }
    }

    /// <summary>
    /// 应用覆盖配置到 NPC
    /// </summary>
    private void ApplyOverride(NPCObject npc) {
        // 应用 Sprite 覆盖
        if (overrideConfig.overrideSprite != null) {
            var spriteRenderer = npc.gameObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                spriteRenderer.sprite = overrideConfig.overrideSprite;
            }
        }

        // 应用 Animator 覆盖
        if (overrideConfig.overrideAnimatorController != null && npc.view != null) {
            npc.view.SetController(overrideConfig.overrideAnimatorController);
        }

        // 应用对话覆盖
        npc.SetRuntimeDialogueNodes(overrideConfig.GetCopiedDialogueNodes());
        npc.SetRuntimeRequiredItemGroups(overrideConfig.GetCopiedRequiredItemGroups());

        // 验证当前节点索引的合法性
        if (npc.IsInConversation()) {
            int currentNodeIndex = npc.GetCurrentNodeIndex();
            int maxIndex = overrideConfig.overrideDialogueNodes.Count > 0 
                ? overrideConfig.overrideDialogueNodes.Count - 1 
                : 0;
            if (currentNodeIndex > maxIndex) {
                npc.SetCurrentNodeIndex(Mathf.Min(currentNodeIndex, maxIndex));
            }
        }

        // 无论是否在对话中，都触发 UI 刷新以显示最新的外观和对话
        // 这确保从 Awake 或存档加载时，UI 都被正确更新
        npc.TriggerNodeChanged();
    }

    /// <summary>
    /// 恢复 NPC 的原始状态
    /// </summary>
    private void RestoreOriginalState(NPCObject npc) {
        // 恢复原始 Sprite
        var spriteRenderer = npc.gameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.sprite = cachedOriginalSprite;
        }

        // 恢复原始 Animator Controller
        if (npc.view != null && cachedOriginalAnimator != null) {
            npc.view.SetController(cachedOriginalAnimator);
        }

        // 恢复原始对话列表
        npc.SetRuntimeDialogueNodes(cachedOriginalDialogueNodes ?? new List<DialogueNode>());
        npc.SetRuntimeRequiredItemGroups(cachedOriginalRequiredItemGroups ?? new List<NPCStaticData.RequiredItemGroup>());

        // 验证当前节点索引的合法性
        if (npc.IsInConversation()) {
            int currentNodeIndex = npc.GetCurrentNodeIndex();
            int maxIndex = (cachedOriginalDialogueNodes != null && cachedOriginalDialogueNodes.Count > 0)
                ? cachedOriginalDialogueNodes.Count - 1
                : 0;
            if (currentNodeIndex > maxIndex) {
                npc.SetCurrentNodeIndex(Mathf.Min(currentNodeIndex, maxIndex));
            }
        }

        // 无论是否在对话中，都触发 UI 刷新
        // 这确保卸载标签时，UI 也被正确更新为恢复后的状态
        npc.TriggerNodeChanged();

        // 清空缓存
        cachedOriginalSprite = null;
        cachedOriginalAnimator = null;
        cachedOriginalDialogueNodes = null;
        cachedOriginalRequiredItemGroups = null;
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
