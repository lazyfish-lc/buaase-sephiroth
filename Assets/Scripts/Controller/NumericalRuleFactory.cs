using UnityEngine;

using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

public static class NumericalRuleFactory {
    private struct WeightedPropertyRef {
        public SmallObjectProperty property;
        public float weight;
    }

    /// <summary>
    /// 将规则字符串解析为 Rule 对象，并建立属性映射关系
    /// </summary>
    public static bool BuildRule(string ruleString, out List<NumericalRule> rules) {
        rules = new List<NumericalRule>();
        if (BuildMaintainConstantRule(ruleString, out List<NumericalMaintainConstantRule> maintainRules)) {
            rules = new List<NumericalRule>(maintainRules);
            return true;
        }

        return false;
    }

    /// <summary>
    /// 解析格式：
    /// <ConstantValue> * <BigObjectName>.<SmallObjectName>.<PropertyName>
    /// { + <ConstantValue> * <BigObjectName>.<SmallObjectName>.<PropertyName> } = CONST
    /// 为整条恒等式生成一条 NumericalMaintainConstantRule
    /// </summary>
    public static bool BuildMaintainConstantRule(string ruleString, out List<NumericalMaintainConstantRule> rules) {
        rules = new List<NumericalMaintainConstantRule>();
        
        if (string.IsNullOrWhiteSpace(ruleString)) {
            Debug.LogWarning("规则字符串为空，无法构建守恒规则");
            return false;
        }

        int equalIndex = ruleString.IndexOf('=');
        if (equalIndex < 0) {
            Debug.LogWarning($"规则格式错误（缺少 '='）：{ruleString}");
            return false;
        }

        string leftExpression = ruleString.Substring(0, equalIndex).Trim();
        if (string.IsNullOrWhiteSpace(leftExpression)) {
            Debug.LogWarning($"规则格式错误（左侧表达式为空）：{ruleString}");
            return false;
        }

        List<WeightedPropertyRef> terms = ParseWeightedTerms(leftExpression);
        if (terms.Count < 2) {
            Debug.LogWarning($"守恒规则至少需要两个属性项：{ruleString}");
            return false;
        }

        List<SmallObjectProperty> properties = new List<SmallObjectProperty>();
        List<float> weights = new List<float>();
        for (int i = 0; i < terms.Count; i++) {
            properties.Add(terms[i].property);
            weights.Add(terms[i].weight);
        }

        NumericalMaintainConstantRule rule = new NumericalMaintainConstantRule();
        rule.Initialize(properties, weights);
        if (rule.GetManagedProperties() == null || rule.GetManagedProperties().Count == 0) {
            Debug.LogWarning($"守恒规则初始化失败：{ruleString}");
            return false;
        }

        rules.Add(rule);
        return true;
    }

    private static List<WeightedPropertyRef> ParseWeightedTerms(string leftExpression) {
        List<WeightedPropertyRef> result = new List<WeightedPropertyRef>();
        string normalized = Regex.Replace(leftExpression, "\\s+", "");
        string[] rawTerms = normalized.Split('+');

        for (int i = 0; i < rawTerms.Length; i++) {
            string rawTerm = rawTerms[i];
            if (string.IsNullOrWhiteSpace(rawTerm)) {
                continue;
            }

            string[] parts = rawTerm.Split('*');
            if (parts.Length != 2) {
                Debug.LogWarning($"规则项格式错误（应为 weight*BigObject.SmallObject.Property）：{rawTerm}");
                continue;
            }

            if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float weight)) {
                Debug.LogWarning($"规则项权重解析失败：{parts[0]}");
                continue;
            }

            string propertyToken = parts[1];
            SmallObjectProperty property = ResolveProperty(propertyToken);
            if (property == null) {
                Debug.LogWarning($"规则项属性不存在：{propertyToken}");
                continue;
            }

            result.Add(new WeightedPropertyRef {
                property = property,
                weight = weight
            });
        }

        return result;
    }

    private static SmallObjectProperty ResolveProperty(string propertyToken) {
        string[] segments = propertyToken.Split('.');
        if (segments.Length != 3) {
            Debug.LogWarning($"属性路径格式错误（应为 BigObject.SmallObject.Property）：{propertyToken}");
            return null;
        }

        string bigObjectName = segments[0];
        string smallObjectName = segments[1];
        string propertyName = segments[2];

        return GameObjectManager.GetProperty(bigObjectName, smallObjectName, propertyName);
    }
}
