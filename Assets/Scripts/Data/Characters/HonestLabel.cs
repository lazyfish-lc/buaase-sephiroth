using System;
using UnityEngine;

/// <summary>
/// 诚实标签 — 用于第三章疯女仆。挂载时通过 NPCAppearanceOverride 覆写 NPC 对话，
/// 使 NPC 提供真实的证词（用于确认妻子的不在场证明）。
/// 当被移除时，触发相关章节事件。
/// </summary>
[Serializable]
public class HonestLabel : Chapter3ClueNPCLabelBase {
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/HonestConfig";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/NPCLabelData/HonestLabelData.asset";

    private static NPCAppearanceOverride cachedConfig;

    public HonestLabel() {
        InitializeOverrideConfig(
            ref cachedConfig,
            CONFIG_RESOURCES_PATH,
            CONFIG_EDITOR_ASSET_PATH,
            nameof(HonestLabel)
        );
    }

    public override string labelName => "Honest";

    protected override void OnDetachFromClueObject(SmallObject smallObject) {
        Debug.Log($"HonestLabel: detached from {smallObject.name}");
        Chapter3Events.RaiseDiaryRevealed();
    }
}
