using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 测试脚本：验证 NPCLabel 运行时持久化功能
/// 用法：
///   - K：加载 VillagerLabel 标签
///   - L：卸载 VillagerLabel 标签
///   - U：验证运行时对话副本状态
///   - V：显示当前外观和对话信息
/// </summary>
public class NPCLabelPersistenceTest : MonoBehaviour
{
    [SerializeField] private NPCObject targetNpc;
    private bool isLabelAttached = false;

    private void Awake()
    {
        if (targetNpc == null)
        {
#if UNITY_2023_1_OR_NEWER
            targetNpc = FindFirstObjectByType<NPCObject>();
#else
            targetNpc = FindObjectOfType<NPCObject>();
#endif
        }

        if (targetNpc == null)
        {
            Debug.LogError("NPCLabelPersistenceTest: NPCObject not found in scene");
        }
    }

    private void Update()
    {
        if (targetNpc == null) return;

        // K：加载标签
        if (Input.GetKeyDown(KeyCode.K))
        {
            AttachVillagerLabel();
        }

        // L：卸载标签
        if (Input.GetKeyDown(KeyCode.L))
        {
            DetachVillagerLabel();
        }

        // U：验证状态
        if (Input.GetKeyDown(KeyCode.U))
        {
            VerifyRuntimeState();
        }

        // V：显示外观和对话
        if (Input.GetKeyDown(KeyCode.V))
        {
            DisplayAppearanceAndDialogue();
        }
    }

    private void AttachVillagerLabel()
    {
        if (isLabelAttached)
        {
            Debug.LogWarning("VillagerLabel already attached");
            return;
        }

        var label = new VillagerLabel();
        label.AttachToOwner(targetNpc);
        isLabelAttached = true;

        Debug.Log("[TEST] VillagerLabel attached via manual code");
        VerifyRuntimeState();
    }

    private void DetachVillagerLabel()
    {
        if (!isLabelAttached)
        {
            Debug.LogWarning("VillagerLabel not attached");
            return;
        }

        // 查找并卸载标签
        if (targetNpc.dynamicState?.smallObjectLabels != null)
        {
            var villagerLabels = new List<ObjectLabel>();
            foreach (var label in targetNpc.dynamicState.smallObjectLabels)
            {
                if (label is VillagerLabel)
                {
                    villagerLabels.Add(label);
                }
            }

            foreach (var label in villagerLabels)
            {
                label.Detach();
                targetNpc.dynamicState.smallObjectLabels.Remove(label);
            }

            isLabelAttached = false;
            Debug.Log("[TEST] VillagerLabel detached");
            VerifyRuntimeState();
        }
    }

    private void VerifyRuntimeState()
    {
        Debug.Log("=== NPCLabel Runtime State Verification ===");

        // 检查原始数据（通过公共方法访问）
        var runtimeNodes = targetNpc.GetCurrentDialogueNodes();
        if (runtimeNodes != null)
        {
            Debug.Log($"Runtime Dialogue Nodes: {runtimeNodes.Count}");
            if (runtimeNodes.Count > 0)
            {
                Debug.Log($"  First node content: {runtimeNodes[0].npcContent}");
            }
        }

        // 检查标签状态
        if (targetNpc.dynamicState?.smallObjectLabels != null)
        {
            Debug.Log($"Attached Labels: {targetNpc.dynamicState.smallObjectLabels.Count}");
            foreach (var label in targetNpc.dynamicState.smallObjectLabels)
            {
                Debug.Log($"  - {label.GetType().Name}");
            }
        }
        else
        {
            Debug.Log("No labels attached");
        }

        // 检查外观
        var spriteRenderer = targetNpc.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Debug.Log($"Current Sprite: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "null")}");
        }

        Debug.Log("========================================");
    }

    private void DisplayAppearanceAndDialogue()
    {
        Debug.Log("=== Current NPC Appearance & Dialogue ===");

        // 外观
        var spriteRenderer = targetNpc.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Debug.Log($"Sprite: {(spriteRenderer.sprite != null ? spriteRenderer.sprite.name : "None")}");
        }

        // 动画控制器
        if (targetNpc.view != null && targetNpc.view.animator != null)
        {
            var controller = targetNpc.view.animator.runtimeAnimatorController;
            string controllerName = controller != null ? controller.name : "None";
            Debug.Log($"Animator: {controllerName}");
        }

        // 对话
        var nodes = targetNpc.GetCurrentDialogueNodes();
        if (nodes != null && nodes.Count > 0)
        {
            Debug.Log($"Dialogue ({nodes.Count} nodes):");
            for (int i = 0; i < Mathf.Min(3, nodes.Count); i++)
            {
                Debug.Log($"  [{i}]: {nodes[i].npcContent}");
            }
            if (nodes.Count > 3)
            {
                Debug.Log($"  ... and {nodes.Count - 3} more");
            }
        }
        else
        {
            Debug.Log("No dialogue nodes");
        }

        Debug.Log("=========================================");
    }
}
