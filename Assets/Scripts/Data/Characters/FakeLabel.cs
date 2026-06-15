using System;
using UnityEngine;

/// <summary>
/// 「虚伪的」标签 — 用于第三章助手日记和计划书。
/// 
/// 当该标签被移除时，触发 Chapter3Events.TrueMotiveRevealed 事件，
/// 揭露助手的真正动机。
/// 
/// 可挂载在 SmallObject（日记/计划书）上作为可剥离的线索标签，
/// 也可通过 NPCAppearanceOverride 覆写 NPC 对话来展示不同内容。
/// </summary>
[Serializable]
public class FakeLabel : Chapter3ClueNPCLabelBase {
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/FakeLabelData";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/NPCLabelData/FakeLabelData.asset";

    private static NPCAppearanceOverride cachedConfig;

    public FakeLabel() {
        InitializeOverrideConfig(
            ref cachedConfig,
            CONFIG_RESOURCES_PATH,
            CONFIG_EDITOR_ASSET_PATH,
            nameof(FakeLabel)
        );
    }

    public override string labelName => "Fake";

    protected override void OnDetachFromClueObject(SmallObject smallObject) {
        Debug.Log($"FakeLabel: 从 {smallObject.name} 上剥离「虚伪的」标签，真相揭露！");
        Chapter3Events.RaiseTrueMotiveRevealed();
        Chapter3_SceneController.NotifyTrueMotiveRevealed();
    }
}
