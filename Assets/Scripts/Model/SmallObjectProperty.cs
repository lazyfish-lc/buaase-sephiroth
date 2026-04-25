using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class SmallObjectProperty {
    public string name;
    public float value;
    public float minValue;
    public float maxValue;
    private List<NumericalRule> rules = new List<NumericalRule>();

    public SmallObjectProperty(string name, float value, float minValue, float maxValue) {
        this.name = name;
        this.value = value;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }

    public bool AddNumericalRule(NumericalRule rule)
    {
        if (rule == null) {
            Debug.LogWarning($"属性 {name} 添加规则失败：规则对象为空");
            return false;
        }
        if (rules.Contains(rule)) {
            Debug.LogWarning($"属性 {name} 已经包含该规则，无法重复添加");
            return false;
        }
        rules.Add(rule);
        return true;
    }

    // 正常修改：触发规则传递
    public bool SetValue(float newValue) {
        if (newValue < minValue || newValue > maxValue) {
            Debug.LogWarning($"属性 {name} 修改失败：新值 {newValue} 超出范围 [{minValue}, {maxValue}]");
            return false;
        }
        foreach (var rule in rules) {
            if (!rule.CheckValid(value, newValue)) {
                Debug.LogWarning($"属性 {name} 修改被规则拦截：新值 {newValue} 将导致相关属性越界");
                return false;
            }
        }
        float oldValue = this.value;
        this.value = newValue;
        Debug.Log($"属性 {name} 从 {oldValue} 变为 {newValue}");
        foreach (var rule in rules) {
            rule.Notify(oldValue, newValue);
        }
        return true;
    }

    // 静默修改：由 Rule 调用，防止无限递归（死循环）
    public void SetValueWithoutNotify(float newValue) {
        this.value = newValue;
        Debug.Log($"属性 {name} 静默修改为 {newValue}");
    }
}