using System;
using UnityEngine;

[Serializable]
public class MeltLabel : Chapter3ClueNPCLabelBase {
    private const string CONFIG_RESOURCES_PATH = "NPCAppearanceOverrides/MeltConfig";
    private const string CONFIG_EDITOR_ASSET_PATH = "Assets/Data/NPCLabelData/MeltLabelData.asset";

    private static NPCAppearanceOverride cachedConfig;

    public MeltLabel() {
        InitializeOverrideConfig(
            ref cachedConfig,
            CONFIG_RESOURCES_PATH,
            CONFIG_EDITOR_ASSET_PATH,
            nameof(MeltLabel)
        );
    }

    public override string labelName => "Melt";

    protected override void OnDetachFromClueObject(SmallObject smallObject) {
        Debug.Log($"MeltLabel: detached from {smallObject.name}");

        var clueTarget = smallObject.GetComponent<Chapter3ClueTarget>();
        if (clueTarget != null) {
            clueTarget.RevealIceWeapon();
            return;
        }

        Chapter3Events.RaiseIceWeaponRevealed();
    }
}
