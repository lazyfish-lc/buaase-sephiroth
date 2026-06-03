using UnityEngine;

/// <summary>
/// VillagerLabel 使用示例脚本
/// 
/// 挂载到场景中任何 GameObject 上，运行时可以通过按键来测试 VillagerLabel 的功能。
/// 
/// 操作说明：
/// - 按 K 键：为当前 NPC 挂载 VillagerLabel
/// - 按 L 键：为当前 NPC 卸载 VillagerLabel
/// - 按 U 键：列出当前 NPC 上的所有标签
/// </summary>
public class VillagerLabelDemo : MonoBehaviour {
    
    private NPCObject targetNpc;
    private VillagerLabel currentLabel;
    
    private void Start() {
        // 自动查找场景中的第一个 NPCObject
#if UNITY_2023_1_OR_NEWER
        targetNpc = FindFirstObjectByType<NPCObject>();
#else
        targetNpc = FindObjectOfType<NPCObject>();
#endif
        if (targetNpc == null) {
            Debug.LogError("场景中未找到 NPCObject，请先添加一个 NPC");
        }
    }
    
    private void Update() {
        if (targetNpc == null) return;
        
        // K 键：挂载 VillagerLabel
        if (Input.GetKeyDown(KeyCode.K)) {
            MountVillagerLabel();
        }
        
        // L 键：卸载 VillagerLabel
        if (Input.GetKeyDown(KeyCode.L)) {
            UnmountVillagerLabel();
        }
        
        // U 键：列出标签列表
        if (Input.GetKeyDown(KeyCode.U)) {
            ListLabels();
        }
    }
    
    /// <summary>
    /// 挂载 VillagerLabel
    /// </summary>
    private void MountVillagerLabel() {
        if (currentLabel != null) {
            Debug.LogWarning("VillagerLabel 已经挂载，请先卸载");
            return;
        }
        
        // 创建标签
        currentLabel = new VillagerLabel();
        
        // 挂载到 NPC
        targetNpc.AddLabel(currentLabel);
        
        Debug.Log($"✓ VillagerLabel 已挂载到 {targetNpc.name}");
        DebugNPCState();
    }
    
    /// <summary>
    /// 卸载 VillagerLabel
    /// </summary>
    private void UnmountVillagerLabel() {
        if (currentLabel == null) {
            Debug.LogWarning("VillagerLabel 未挂载");
            return;
        }
        
        // 卸载标签
        targetNpc.RemoveLabel(currentLabel);
        currentLabel = null;
        
        Debug.Log($"✓ VillagerLabel 已从 {targetNpc.name} 卸载");
        DebugNPCState();
    }
    
    /// <summary>
    /// 列出当前 NPC 上的所有标签
    /// </summary>
    private void ListLabels() {
        if (targetNpc.dynamicState?.smallObjectLabels == null || targetNpc.dynamicState.smallObjectLabels.Count == 0) {
            Debug.Log($"{targetNpc.name} 上没有标签");
            return;
        }
        
        Debug.Log($"========== {targetNpc.name} 上的标签 ==========");
        for (int i = 0; i < targetNpc.dynamicState.smallObjectLabels.Count; i++) {
            var label = targetNpc.dynamicState.smallObjectLabels[i];
            Debug.Log($"  [{i}] {label.labelName} ({label.GetType().Name})");
        }
        Debug.Log("========================================");
    }
    
    /// <summary>
    /// 调试打印 NPC 当前状态
    /// </summary>
    private void DebugNPCState() {
        if (targetNpc == null) return;
        
        Debug.Log("-------- NPC 状态 --------");
        Debug.Log($"Name: {targetNpc.name}");
        Debug.Log($"在对话中: {targetNpc.IsInConversation()}");
        Debug.Log($"标签数: {targetNpc.dynamicState?.smallObjectLabels?.Count ?? 0}");
        
        // 获取当前对话内容
        if (targetNpc.GetCurrentDialogueNodes().Count > 0) {
            string content = targetNpc.GetCurrentContent();
            Debug.Log($"当前对话: {content}");
        }
        
        Debug.Log("-----------------------");
    }
    
    private void OnGUI() {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("VillagerLabel 演示工具", GUI.skin.box);
        
        GUILayout.Label("操作说明：", GUI.skin.label);
        GUILayout.Label("• K: 挂载 VillagerLabel", GUI.skin.label);
        GUILayout.Label("• L: 卸载 VillagerLabel", GUI.skin.label);
        GUILayout.Label("• U: 列出标签列表", GUI.skin.label);
        
        GUILayout.Space(10);
        
        if (targetNpc != null) {
            GUILayout.Label($"目标 NPC: {targetNpc.name}", GUI.skin.label);
        } else {
            GUILayout.Label("未找到 NPC", GUI.skin.label, GUILayout.Height(30));
        }
        
        GUILayout.EndArea();
    }
}
