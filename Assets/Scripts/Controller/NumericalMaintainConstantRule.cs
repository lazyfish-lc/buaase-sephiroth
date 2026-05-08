using System.Collections.Generic;
using UnityEngine;

public class NumericalMaintainConstantRule : NumericalRule {
    private readonly List<SmallObjectProperty> managedProperties = new List<SmallObjectProperty>();
    private readonly List<float> managedWeights = new List<float>();

    private const float Epsilon = 1e-6f;

    private struct PlannedValue {
        public SmallObjectProperty property;
        public float value;
    }

    public void Initialize(List<SmallObjectProperty> properties, List<float> weights) {
        managedProperties.Clear();
        managedWeights.Clear();

        if (properties == null || weights == null || properties.Count != weights.Count) {
            Debug.LogWarning("守恒规则初始化失败：属性和权重数量不一致");
            return;
        }

        HashSet<SmallObjectProperty> seen = new HashSet<SmallObjectProperty>();
        for (int i = 0; i < properties.Count; i++) {
            SmallObjectProperty property = properties[i];
            if (property == null) {
                Debug.LogWarning("守恒规则初始化失败：存在空属性");
                managedProperties.Clear();
                managedWeights.Clear();
                return;
            }

            if (!seen.Add(property)) {
                Debug.LogWarning($"守恒规则初始化失败：属性 {property.name} 重复出现");
                managedProperties.Clear();
                managedWeights.Clear();
                return;
            }

            managedProperties.Add(property);
            managedWeights.Add(weights[i]);
        }
    }

    public override IReadOnlyList<SmallObjectProperty> GetManagedProperties() {
        return managedProperties;
    }

    public override bool CheckValid(NumericalModificationRequest request) {
        return TryBuildPlan(request, out _);
    }

    public override void Apply(NumericalModificationRequest request) {
        if (!TryBuildPlan(request, out List<PlannedValue> plannedValues)) {
            Debug.LogWarning("守恒规则执行失败：请求不满足规则");
            return;
        }

        for (int i = 0; i < plannedValues.Count; i++) {
            plannedValues[i].property.SetValueWithoutNotify(plannedValues[i].value);
        }
    }

    private bool TryBuildPlan(NumericalModificationRequest request, out List<PlannedValue> plannedValues) {
        plannedValues = new List<PlannedValue>();

        if (request == null) {
            Debug.LogWarning("守恒规则拒绝修改：修改请求为空");
            return false;
        }

        if (managedProperties.Count == 0 || managedWeights.Count == 0) {
            Debug.LogWarning("守恒规则拒绝修改：规则未初始化或没有管理属性");
            return false;
        }

        List<int> directIndices = new List<int>();
        for (int i = 0; i < managedProperties.Count; i++) {
            if (request.ContainsProperty(managedProperties[i])) {
                directIndices.Add(i);
            }
        }

        if (directIndices.Count == 0) {
            return true;
        }

        float directWeightedDelta = 0f;
        HashSet<int> directIndexSet = new HashSet<int>(directIndices);
        for (int i = 0; i < directIndices.Count; i++) {
            int index = directIndices[i];
            SmallObjectProperty property = managedProperties[index];
            if (!request.TryGetModification(property, out float requestedValue)) {
                Debug.LogWarning($"守恒规则检查失败：请求中缺少属性 {property.name} 的修改");
                return false;
            }

            float currentValue = property.value;
            float delta = requestedValue - currentValue;
            directWeightedDelta += managedWeights[index] * delta;
        }

        if (Mathf.Abs(directWeightedDelta) < Epsilon) {
            for (int i = 0; i < directIndices.Count; i++) {
                int index = directIndices[i];
                SmallObjectProperty property = managedProperties[index];
                if (!request.TryGetModification(property, out float requestedValue)) {
                    Debug.LogWarning($"守恒规则拒绝修改：请求中缺少属性 {property.name} 的修改");
                    return false;
                }

                if (requestedValue < property.minValue - Epsilon || requestedValue > property.maxValue + Epsilon) {
                    Debug.LogWarning($"守恒规则拒绝修改：属性 {property.name} 新值 {requestedValue} 超出范围 [{property.minValue}, {property.maxValue}]");
                    return false;
                }

                plannedValues.Add(new PlannedValue {
                    property = property,
                    value = requestedValue
                });
            }

            return true;
        }

        float unmodifiedWeightSum = 0f;
        for (int i = 0; i < managedProperties.Count; i++) {
            if (directIndexSet.Contains(i)) {
                continue;
            }

            unmodifiedWeightSum += managedWeights[i];
        }

        if (Mathf.Abs(unmodifiedWeightSum) < Epsilon) {
            Debug.LogWarning("守恒规则拒绝修改：未修改属性权重之和为 0，无法计算补偿");
            return false;
        }

        float compensationDelta = -directWeightedDelta / unmodifiedWeightSum;

        for (int i = 0; i < managedProperties.Count; i++) {
            SmallObjectProperty property = managedProperties[i];
            float plannedValue;

            if (directIndexSet.Contains(i)) {
                if (!request.TryGetModification(property, out plannedValue)) {
                    Debug.LogWarning($"守恒规则拒绝修改：请求中缺少属性 {property.name} 的修改");
                    return false;
                }
            } else if (Mathf.Abs(managedWeights[i]) < Epsilon) {
                plannedValue = property.value;
            } else {
                plannedValue = property.value + compensationDelta;
            }

            if (plannedValue < property.minValue - Epsilon || plannedValue > property.maxValue + Epsilon) {
                Debug.LogWarning($"守恒规则拒绝修改：属性 {property.name} 计算值 {plannedValue} 超出范围 [{property.minValue}, {property.maxValue}]");
                return false;
            }

            plannedValues.Add(new PlannedValue {
                property = property,
                value = plannedValue
            });
        }

        return true;
    }
}