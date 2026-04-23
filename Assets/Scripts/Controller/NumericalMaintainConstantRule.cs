using System.Collections.Generic;
using UnityEngine;

public class NumericalMaintainConstantRule : NumericalRule {
    public float inputWeight;
    public List<float> outputWeights = new List<float>();

    private const float Epsilon = 1e-6f;

    public override bool CheckValid(float oldValue, float newValue) {
        if (outputProperties == null || outputProperties.Count == 0) {
            return true;
        }

        float totalOutputWeight = 0f;
        for (int i = 0; i < outputWeights.Count; i++) {
            totalOutputWeight += outputWeights[i];
        }

        if (Mathf.Abs(totalOutputWeight) < Epsilon) {
            Debug.LogWarning("守恒规则无效：输出权重和为 0，无法分配变化量");
            return false;
        }

        float delta = newValue - oldValue;
        float weightedDelta = inputWeight * delta;

        int count = Mathf.Min(outputProperties.Count, outputWeights.Count);
        for (int i = 0; i < count; i++) {
            SmallObjectProperty output = outputProperties[i];
            float outputDelta = -weightedDelta * outputWeights[i] / totalOutputWeight;
            float candidate = output.value + outputDelta;

            if (Mathf.Abs(candidate - output.minValue) < Epsilon) {
                candidate = output.minValue;
            } else if (Mathf.Abs(candidate - output.maxValue) < Epsilon) {
                candidate = output.maxValue;
            }
            
            if (candidate - output.minValue < -Epsilon || candidate - output.maxValue > Epsilon) {
                return false;
            }
        }

        return true;
    }

    public override void Notify(float oldValue, float newValue) {
        if (!CheckValid(oldValue, newValue)) {
            Debug.LogWarning("守恒规则拦截：本次修改将导致属性越界，已取消联动修改");
            return;
        }

        if (outputProperties == null || outputProperties.Count == 0) {
            return;
        }

        float totalOutputWeight = 0f;
        for (int i = 0; i < outputWeights.Count; i++) {
            totalOutputWeight += outputWeights[i];
        }

        if (Mathf.Abs(totalOutputWeight) < Epsilon) {
            return;
        }

        float delta = newValue - oldValue;
        float weightedDelta = inputWeight * delta;

        int count = Mathf.Min(outputProperties.Count, outputWeights.Count);
        for (int i = 0; i < count; i++) {
            SmallObjectProperty output = outputProperties[i];
            float outputDelta = -weightedDelta * outputWeights[i] / totalOutputWeight;
            output.SetValueWithoutNotify(output.value + outputDelta);
        }
    }
}