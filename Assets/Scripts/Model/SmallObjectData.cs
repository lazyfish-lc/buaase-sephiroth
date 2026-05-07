using System;
using UnityEngine;
using System.Collections.Generic;

// 静态数据 - 存档在项目资源中
[CreateAssetMenu(fileName = "SmallObjectData", menuName = "Game/SmallObjectData")]
public class SmallObjectStaticData : ScriptableObject {
    public string objectName;
    public List<PropertyDefinition> propertyBlueprints = new List<PropertyDefinition>() {
        new PropertyDefinition() { name = "Health", initialValue = 100f, minValue = 0f, maxValue = 99999f },
        new PropertyDefinition() { name = "ATK", initialValue = 30f, minValue = 0f, maxValue = 99999f },
        new PropertyDefinition() { name = "DEF", initialValue = 10f, minValue = 0f, maxValue = 99999f },
        new PropertyDefinition() { name = "AttackSpeed", initialValue = 1f, minValue = 0f, maxValue = 20f }
    };
    // 在资源中记录该 SmallObject 初始化时需挂载的标签描述（通过 LabelFactory 创建）
    public List<string> labelBlueprints = new List<string>();
}

[Serializable]
public class SmallObjectDynamicState {
    // 可以根据需要添加更多动态状态字段

    public bool isDestroyed = false;
    public List<ObjectLabel> smallObjectLabels = new List<ObjectLabel>();
    public Dictionary<string, SmallObjectProperty> propertyMap = new Dictionary<string, SmallObjectProperty>();
    
    public bool isHurt = false; // 是否处于受击状态，受击状态下可能无法移动或攻击

    // 获取属性的最终值：基础值 + 所有挂载在该对象上的 Label 对该属性的增量影响
    public float GetPropertyValueWithAffect(string propertyName) {
        if (!propertyMap.ContainsKey(propertyName)) {
            Debug.LogWarning($"尝试读取不存在的属性 {propertyName}");
            return 0f;
        }

        float baseValue = propertyMap[propertyName].value;
        float delta = 0f;
        if (smallObjectLabels != null) {
            foreach (var label in smallObjectLabels) {
                if (label is ILabelAffectsProperty affector) {
                    try {
                        delta += affector.GetPropertyDelta(propertyName);
                    } catch (System.Exception ex) {
                        Debug.LogWarning($"Label 对属性增量计算出错: {ex}");
                    }
                }
            }
        }

        return baseValue + delta;
    }
}

[Serializable]
public class PropertyDefinition {
    public string name;
    public float initialValue;
    public float minValue;
    public float maxValue;
}