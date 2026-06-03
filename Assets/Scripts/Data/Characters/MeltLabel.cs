using System;
using UnityEngine;

[Serializable]
public class MeltLabel : Chapter3ClueNPCLabelBase {
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
