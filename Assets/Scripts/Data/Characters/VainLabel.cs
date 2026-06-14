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
        } else {
            Chapter3Events.RaiseDiaryRevealed();
        }

        // VainLabel 在日记上代表"虚伪的"语义 — 同时通知真相揭露
        Chapter3Events.RaiseTrueMotiveRevealed();
        Chapter3_SceneController.NotifyTrueMotiveRevealed();
    }
}
