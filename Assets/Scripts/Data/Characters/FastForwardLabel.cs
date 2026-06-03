using System;
using UnityEngine;

[Serializable]
public class FastForwardLabel : Chapter3ClueNPCLabelBase {
    public override string labelName => "FastForward";

    protected override void OnDetachFromClueObject(SmallObject smallObject) {
        Debug.Log($"FastForwardLabel: detached from {smallObject.name}");

        var clueTarget = smallObject.GetComponent<Chapter3ClueTarget>();
        if (clueTarget != null) {
            clueTarget.CorrectClock();
            return;
        }

        Chapter3Events.RaiseClockCorrected();
    }
}
