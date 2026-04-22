using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SmallObjectProperty {
    public string name;
    public float value;
    public List<NumericalRule> rules = new List<NumericalRule>();

    public SmallObjectProperty(string name, float value) {
        this.name = name;
        this.value = value;
    }

    // 正常修改：触发规则传递
    public void OnChanged(float newValue) {
        float oldValue = this.value;
        this.value = newValue;
        Debug.Log($"属性 {name} 从 {oldValue} 变为 {newValue}");
        foreach (var rule in rules) {
            rule.Notify(oldValue, newValue);
        }

        
    }

    // 静默修改：由 Rule 调用，防止无限递归（死循环）
    public void OnChangedWithoutNotify(float newValue) {
        this.value = newValue;
        Debug.Log($"属性 {name} 静默修改为 {newValue}");
    }
}