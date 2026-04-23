using UnityEngine;
using System.Collections.Generic;

public static class NumericalRuleManager {
    private static List<NumericalRule> rules = new List<NumericalRule>();

     /// <summary>
     /// 注册规则：将规则添加到输入属性的规则列表中
     /// 由 NumericalRuleFactory.BuildRule 调用
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
     /// 注册单条规则
     /// 由 NumericalRuleFactory.BuildRule 调用
     /// </summary>
    public static void RegisterRule(NumericalRule rule) {
        if (rule == null) {
            Debug.LogWarning("注册规则失败：规则对象为空");
            return;
        }
        if (rule.inputProperty == null) {
            Debug.LogWarning("注册规则失败：输入属性未指定");
            return;
        }
        if (!rule.inputProperty.AddNumericalRule(rule)) {
            Debug.LogWarning($"属性 {rule.inputProperty.name} 添加规则失败，规则注册失败");
        }
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
}
