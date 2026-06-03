using System;
using UnityEngine;

[Serializable]
public class VainLabel : Chapter3ClueNPCLabelBase {
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/VainConfig";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/NPCLabelData/VainOverride.asset";

    private static NPCAppearanceOverride cachedConfig;

    public VainLabel() {
        InitializeOverrideConfig(
            ref cachedConfig,
            CONFIG_RESOURCES_PATH,
            CONFIG_EDITOR_ASSET_PATH,
            nameof(VainLabel)
        );
    }

    public override string labelName => "Vain";

    protected override void OnDetachFromClueObject(SmallObject smallObject) {
        Debug.Log($"VainLabel: detached from {smallObject.name}");
        Chapter3Events.RaiseDiaryRevealed();
    }
}
