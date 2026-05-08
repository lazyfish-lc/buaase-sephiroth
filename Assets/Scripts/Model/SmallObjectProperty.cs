using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SmallObjectProperty {
    public string name;
    public float value;
    public float minValue;
    public float maxValue;

    public SmallObjectProperty(string name, float value, float minValue, float maxValue) {
        this.name = name;
        this.value = value;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    public bool SetValue(float newValue) {
        return NumericalRuleManager.TryModifyProperty(this, newValue);
    }

    public void SetValueWithoutNotify(float newValue) {
        this.value = newValue;
        Debug.Log($"属性 {name} 静默修改为 {newValue}");
    }
}