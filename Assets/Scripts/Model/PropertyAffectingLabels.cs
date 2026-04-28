using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HardLabel : ObjectLabel, ILabelAffectsProperty {
    private float DEFDelta = 10f;
    public HardLabel() {
    }

    // 自定义类名显示：同类所有实例共享该名称
    public override string labelName => "Hard";

    public IEnumerable<string> GetAffectedPropertyNames() {
        yield return "DEF";
    }

    public float GetPropertyDelta(string propertyName) {
        if (string.Equals(propertyName, "DEF", StringComparison.OrdinalIgnoreCase)) return DEFDelta;
        return 0f;
    }
}

[Serializable]
public class FragileLabel : ObjectLabel, ILabelAffectsProperty {
    private float DEFDelta = -10f;
    public FragileLabel() {
    }

    // 自定义类名显示：同类所有实例共享该名称
    public override string labelName => "Fragile";

    public IEnumerable<string> GetAffectedPropertyNames() {
        yield return "DEF";
    }

    public float GetPropertyDelta(string propertyName) {
        if (string.Equals(propertyName, "DEF", StringComparison.OrdinalIgnoreCase)) return DEFDelta;
        return 0f;
    }
}
