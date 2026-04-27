using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HardLabel : ObjectLabel, ILabelAffectsProperty {
    private float DEFDelta = 10f;
    public HardLabel() {
        this.labelName = "坚硬";
    }

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
        this.labelName = "易碎";
    }

    public IEnumerable<string> GetAffectedPropertyNames() {
        yield return "DEF";
    }

    public float GetPropertyDelta(string propertyName) {
        if (string.Equals(propertyName, "DEF", StringComparison.OrdinalIgnoreCase)) return DEFDelta;
        return 0f;
    }
}
