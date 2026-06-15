using System;
using UnityEngine;

/// <summary>
/// 第三章妻子"内向的"标签。
/// 
/// 用于表示妻子的性格特征，可能影响其对话内容或玩家互动的成功率。
/// 当被移除时，触发相关章节事件。
/// </summary>
[Serializable]
public class IntrovertedLabel : Chapter3ClueNPCLabelBase {
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/IntrovertedLabelData";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/NPCLabelData/IntrovertedLabelData.asset";

    private static NPCAppearanceOverride cachedConfig;

    public IntrovertedLabel() {
        InitializeOverrideConfig(
            ref cachedConfig,
            CONFIG_RESOURCES_PATH,
            CONFIG_EDITOR_ASSET_PATH,
            nameof(IntrovertedLabel)
        );
    }

    public override string labelName => "Introverted";

    protected override void OnDetachFromClueObject(SmallObject smallObject) {
        Debug.Log($"IntrovertedLabel: detached from {smallObject.name}");
        // 妻子标签剥离仅恢复 NPC 原始外观/对话（由 NPCLabelBase.OnDetach 处理），
        // 不触发日记相关事件。日记线索应由助手日记上的 VainLabel 剥离触发。
    }
}
