using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// 编辑器工具：生成测试用的 NPCAppearanceOverride 资产
/// 
/// 使用方法：在编辑器菜单中选择 Tools > Generate NPC Test Data > Create Villager Config
/// </summary>
#if UNITY_EDITOR
public class NPCTestDataGenerator {
    
    private const string RESOURCE_FOLDER = "Assets/Resources/NPCAppearanceOverrides";
    private const string VILLAGER_CONFIG_NAME = "VillagerConfig";
    
    [MenuItem("Tools/Generate NPC Test Data/Create Villager Config")]
    public static void CreateVillagerConfig() {
        // 确保文件夹存在
        if (!AssetDatabase.IsValidFolder(RESOURCE_FOLDER)) {
            string parentFolder = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(parentFolder)) {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            AssetDatabase.CreateFolder(parentFolder, "NPCAppearanceOverrides");
        }

        // 创建 NPCAppearanceOverride 资产
        var config = ScriptableObject.CreateInstance<NPCAppearanceOverride>();
        
        // 配置测试数据
        config.overrideSprite = null;  // 实际使用时在编辑器中设置
        config.overrideAnimatorController = null;  // 实际使用时在编辑器中设置
        
        // 创建示例对话节点
        var dialogueOption1 = new DialogueOption {
            text = "我是村民。",
            targetNodeIndex = 1,
            rewardProperty = "",
            rewardAmount = 0
        };
        
        var dialogueOption2 = new DialogueOption {
            text = "告诉我关于这个地方的事情。",
            targetNodeIndex = 2,
            rewardProperty = "",
            rewardAmount = 0
        };
        
        var dialogueNode0 = new DialogueNode {
            npcContent = "欢迎来到村庄！你是新来的吗？",
            hasOptions = true,
            nextNodeIndex = -1,
            options = new System.Collections.Generic.List<DialogueOption> { dialogueOption1, dialogueOption2 },
            enterActions = new System.Collections.Generic.List<DialogueNodeActionSO>(),
            exitActions = new System.Collections.Generic.List<DialogueNodeActionSO>()
        };
        
        var dialogueNode1 = new DialogueNode {
            npcContent = "是的，我是这个村子里的一个普通村民。",
            hasOptions = false,
            nextNodeIndex = 3,
            options = new System.Collections.Generic.List<DialogueOption>(),
            enterActions = new System.Collections.Generic.List<DialogueNodeActionSO>(),
            exitActions = new System.Collections.Generic.List<DialogueNodeActionSO>()
        };
        
        var dialogueNode2 = new DialogueNode {
            npcContent = "这是一个美好的地方，有许多有趣的人和事。",
            hasOptions = false,
            nextNodeIndex = 3,
            options = new System.Collections.Generic.List<DialogueOption>(),
            enterActions = new System.Collections.Generic.List<DialogueNodeActionSO>(),
            exitActions = new System.Collections.Generic.List<DialogueNodeActionSO>()
        };
        
        var dialogueNode3 = new DialogueNode {
            npcContent = "有什么我可以帮你的吗？",
            hasOptions = false,
            nextNodeIndex = -1,
            options = new System.Collections.Generic.List<DialogueOption>(),
            enterActions = new System.Collections.Generic.List<DialogueNodeActionSO>(),
            exitActions = new System.Collections.Generic.List<DialogueNodeActionSO>()
        };
        
        config.overrideDialogueNodes = new System.Collections.Generic.List<DialogueNode> {
            dialogueNode0,
            dialogueNode1,
            dialogueNode2,
            dialogueNode3
        };
        
        // 创建示例必需物品组
        var requiredItemGroup = new NPCStaticData.RequiredItemGroup {
            requiredItems = new System.Collections.Generic.List<string>(),
            startNodeIfHasRequiredItems = -1
        };
        
        config.overrideRequiredItemGroups = new System.Collections.Generic.List<NPCStaticData.RequiredItemGroup> {
            requiredItemGroup
        };
        
        // 保存资产
        string assetPath = Path.Combine(RESOURCE_FOLDER, VILLAGER_CONFIG_NAME + ".asset");
        AssetDatabase.CreateAsset(config, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✓ VillagerConfig 资产已创建: {assetPath}");
        EditorGUIUtility.PingObject(config);
    }
    
    [MenuItem("Tools/Generate NPC Test Data/Delete Villager Config")]
    public static void DeleteVillagerConfig() {
        string assetPath = Path.Combine(RESOURCE_FOLDER, VILLAGER_CONFIG_NAME + ".asset");
        if (AssetDatabase.LoadAssetAtPath<NPCAppearanceOverride>(assetPath) != null) {
            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.Refresh();
            Debug.Log($"✓ VillagerConfig 资产已删除: {assetPath}");
        } else {
            Debug.LogWarning($"✗ VillagerConfig 资产不存在: {assetPath}");
        }
    }
    
    [MenuItem("Tools/Generate NPC Test Data/Open Villager Config")]
    public static void OpenVillagerConfig() {
        string assetPath = Path.Combine(RESOURCE_FOLDER, VILLAGER_CONFIG_NAME + ".asset");
        var config = AssetDatabase.LoadAssetAtPath<NPCAppearanceOverride>(assetPath);
        if (config != null) {
            AssetDatabase.OpenAsset(config);
            Debug.Log($"✓ 打开 VillagerConfig: {assetPath}");
        } else {
            Debug.LogError($"✗ VillagerConfig 资产不存在: {assetPath}");
        }
    }
}
#endif
