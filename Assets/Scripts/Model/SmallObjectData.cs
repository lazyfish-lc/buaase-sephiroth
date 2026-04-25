using System;
using UnityEngine;
using System.Collections.Generic;

// 静态数据 - 存档在项目资源中
[CreateAssetMenu(fileName = "SmallObjectData", menuName = "Game/SmallObjectData")]
public class SmallObjectStaticData : ScriptableObject {
    public string objectName;
    public BigObject ownerBigObject;
    public List<PropertyDefinition> propertyBlueprints = new List<PropertyDefinition>() {
        new PropertyDefinition() { name = "Health", initialValue = 100f, minValue = 0f, maxValue = 99999f },
        new PropertyDefinition() { name = "ATK", initialValue = 30f, minValue = 0f, maxValue = 99999f },
        new PropertyDefinition() { name = "DEF", initialValue = 10f, minValue = 0f, maxValue = 99999f },
        new PropertyDefinition() { name = "AttackSpeed", initialValue = 1f, minValue = 0f, maxValue = 20f }
    };
}

[Serializable]
public class SmallObjectDynamicState {
    // 可以根据需要添加更多动态状态字段

    public bool isDestroyed = false;
    public List<ObjectLabel> smallObjectLabels = new List<ObjectLabel>();
    public Dictionary<string, SmallObjectProperty> propertyMap = new Dictionary<string, SmallObjectProperty>();
}

[Serializable]
public class PropertyDefinition {
    public string name;
    public float initialValue;
    public float minValue;
    public float maxValue;
}