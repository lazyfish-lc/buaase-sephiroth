using System;
using UnityEngine;

[Serializable]
public class FastForwardLabel : Chapter3ClueNPCLabelBase {
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/FastForwardLabelData";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/NPCLabelData/FastForwardLabelData.asset";

    private static NPCAppearanceOverride cachedConfig;

    public FastForwardLabel() {
        InitializeOverrideConfig(
            ref cachedConfig,
            CONFIG_RESOURCES_PATH,
            CONFIG_EDITOR_ASSET_PATH,
            nameof(FastForwardLabel)
        );
    }

    public override string labelName => "FastForward";

    protected override void OnDetachFromClueObject(SmallObject smallObject) {
        Debug.Log($"FastForwardLabel: detached from {smallObject.name}");

        var clueTarget = smallObject.GetComponent<Chapter3ClueTarget>();
        if (clueTarget != null) {
            clueTarget.CorrectClock();
        } else {
            Chapter3Events.RaiseClockCorrected();
        }

        // 直达路径：不依赖事件订阅
        Chapter3_SceneController.NotifyClockCorrected();
    }
}
