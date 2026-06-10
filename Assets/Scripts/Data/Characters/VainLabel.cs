using System;
using UnityEngine;

[Serializable]
public class VainLabel : Chapter3ClueNPCLabelBase {
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/VainConfig";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/NPCLabelData/VainLabelData.asset";

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

        var clueTarget = smallObject.GetComponent<Chapter3ClueTarget>();
        if (clueTarget != null) {
            clueTarget.RevealDiary();
            return;
        }

        Chapter3Events.RaiseDiaryRevealed();
    }
}
