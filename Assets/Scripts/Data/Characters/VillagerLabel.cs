using System;
using UnityEngine;

/// <summary>
/// 示例 NPC 标签：村民变装标签
/// 
/// 这是一个继承 NPCLabelBase 的示例，展示如何使用基类来修改 NPC 的外观和对话。
/// 所有 VillagerLabel 实例均指向同一个 NPCAppearanceOverride 对象（VillagerConfig）。
/// 该配置资产应放在 Resources/NPCAppearanceOverrides/ 路径下。
/// </summary>
[Serializable]
public class VillagerLabel : NPCLabelBase {
    
    // 缓存加载的配置，确保所有实例指向同一对象
    private static NPCAppearanceOverride cachedConfig;
    
    /// <summary>
    /// 首选 Resources 路径（运行时加载），备选编辑器路径用于直接加载测试资产
    /// </summary>
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/VillagerConfig";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/LabelTest/VillagerOverride.asset";

    public VillagerLabel() {
        // 首次创建实例时加载配置，后续复用
        if (cachedConfig == null) {
            // 1) 尝试从 Resources（运行时常用）加载
            cachedConfig = Resources.Load<NPCAppearanceOverride>(CONFIG_RESOURCES_PATH);

            // 2) 若未找到且在编辑器中，则尝试通过 AssetDatabase 从项目路径加载（便于测试资产放在任意位置）
#if UNITY_EDITOR
            if (cachedConfig == null) {
                try {
                    var config = UnityEditor.AssetDatabase.LoadAssetAtPath<NPCAppearanceOverride>(CONFIG_EDITOR_ASSET_PATH);
                    if (config != null) cachedConfig = config;
                } catch (System.Exception ex) {
                    Debug.LogWarning($"VillagerLabel: AssetDatabase load failed: {ex.Message}");
                }
            }
#endif

            if (cachedConfig == null) {
                Debug.LogError($"VillagerLabel: Failed to load NPCAppearanceOverride. Tried Resources('{CONFIG_RESOURCES_PATH}') and Editor path ('{CONFIG_EDITOR_ASSET_PATH}').");
            }
        }

        // 通过反射设置 overrideConfig（因为是 protected 字段）
        var field = typeof(NPCLabelBase).GetField("overrideConfig",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null) {
            field.SetValue(this, cachedConfig);
        }
    }

    public override string labelName => "Villager";
}
