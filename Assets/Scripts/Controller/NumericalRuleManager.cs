using UnityEngine;
using System.Collections.Generic;

public static class NumericalRuleManager {
    private static List<NumericalRule> rules = new List<NumericalRule>();

    /// <summary>
    /// 注册规则列表。
    /// </summary>
    public static void RegisterRules(List<NumericalRule> rules) {
        if (rules == null || rules.Count == 0) {
            Debug.LogWarning("注册规则失败：规则列表为空");
            return;
        }
        foreach (var rule in rules) {
            RegisterRule(rule);
        }
    }

    /// <summary>
    /// 注册单条规则。
    /// </summary>
    public static void RegisterRule(NumericalRule rule) {
        if (rule == null) {
            Debug.LogWarning("注册规则失败：规则对象为空");
            return;
        }

        var managedProperties = rule.GetManagedProperties();
        if (managedProperties == null || managedProperties.Count == 0) {
            Debug.LogWarning("注册规则失败：规则没有管理任何属性");
            return;
        }

        for (int i = 0; i < rules.Count; i++) {
            if (RuleScopesOverlap(rule, rules[i])) {
                Debug.LogWarning("注册规则失败：该规则与已存在规则存在重叠管理范围");
                return;
            }
        }

        rules.Add(rule);
    }

    public static void BuildAndRegisterRules(string ruleString) {
        if (string.IsNullOrWhiteSpace(ruleString)) {
            Debug.LogWarning("规则字符串为空，无法构建和注册规则");
            return;
        }
        if (NumericalRuleFactory.BuildRule(ruleString, out List<NumericalRule> rules)) {
            RegisterRules(rules);
        } else {
            Debug.LogWarning($"规则构建失败，无法注册规则：{ruleString}");
        }
    }

    public static bool TryModifyProperty(SmallObjectProperty property, float newValue) {
        if (property == null) {
            Debug.LogWarning("属性修改失败：属性为空");
            return false;
        }

        NumericalModificationRequest request = new NumericalModificationRequest();
        if (!request.AddModification(property, newValue)) {
            return false;
        }

        return TryApplyModificationRequest(request);
    }

    public static bool TryApplyModificationRequest(NumericalModificationRequest request) {
        if (request == null || request.Count == 0) {
            Debug.LogWarning("修改请求失败：请求为空");
            return false;
        }

        if (!ValidateRequestUnique(request)) {
            Debug.LogWarning("修改请求失败：请求内存在重复属性");
            return false;
        }

        if (HasOverlappingRuleScopes()) {
            Debug.LogWarning("修改请求失败：存在重叠的规则管理范围");
            return false;
        }

        for (int i = 0; i < request.Count; i++) {
            var modification = request.Modifications[i];
            if (modification.property == null) {
                Debug.LogWarning("修改请求失败：存在空属性");
                return false;
            }

            if (modification.newValue < modification.property.minValue || modification.newValue > modification.property.maxValue) {
                Debug.LogWarning($"属性 {modification.property.name} 修改失败：新值 {modification.newValue} 超出范围 [{modification.property.minValue}, {modification.property.maxValue}]");
                return false;
            }
        }

        for (int i = 0; i < rules.Count; i++) {
            if (!rules[i].CheckValid(request)) {
                Debug.LogWarning("修改请求失败：有规则拒绝了这次修改");
                return false;
            }
        }

        HashSet<SmallObjectProperty> managedProperties = CollectManagedProperties();
        for (int i = 0; i < request.Count; i++) {
            var modification = request.Modifications[i];
            if (managedProperties.Contains(modification.property)) {
                continue;
            }

            modification.property.SetValueWithoutNotify(modification.newValue);
        }

        for (int i = 0; i < rules.Count; i++) {
            rules[i].Apply(request);
        }

        return true;
    }

    private static bool ValidateRequestUnique(NumericalModificationRequest request) {
        for (int i = 0; i < request.Count; i++) {
            var left = request.Modifications[i].property;
            for (int j = i + 1; j < request.Count; j++) {
                if (ReferenceEquals(left, request.Modifications[j].property)) {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool HasOverlappingRuleScopes() {
        for (int i = 0; i < rules.Count; i++) {
            for (int j = i + 1; j < rules.Count; j++) {
                if (RuleScopesOverlap(rules[i], rules[j])) {
                    return true;
                }
            }
        }

        return false;
    }

    private static HashSet<SmallObjectProperty> CollectManagedProperties() {
        HashSet<SmallObjectProperty> result = new HashSet<SmallObjectProperty>();
        for (int i = 0; i < rules.Count; i++) {
            var managedProperties = rules[i].GetManagedProperties();
            if (managedProperties == null) {
                continue;
            }

            for (int j = 0; j < managedProperties.Count; j++) {
                if (managedProperties[j] != null) {
                    result.Add(managedProperties[j]);
                }
            }
        }

        return result;
    }

    private static bool RuleScopesOverlap(NumericalRule leftRule, NumericalRule rightRule) {
        if (leftRule == null || rightRule == null) {
            return false;
        }

        var leftProperties = leftRule.GetManagedProperties();
        var rightProperties = rightRule.GetManagedProperties();
        if (leftProperties == null || rightProperties == null) {
            return false;
        }

        for (int i = 0; i < leftProperties.Count; i++) {
            SmallObjectProperty leftProperty = leftProperties[i];
            if (leftProperty == null) {
                continue;
            }

            for (int j = 0; j < rightProperties.Count; j++) {
                if (ReferenceEquals(leftProperty, rightProperties[j])) {
                    return true;
                }
            }
        }

        return false;
    }
}
