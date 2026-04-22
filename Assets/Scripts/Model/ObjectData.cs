using System;
using UnityEngine;
using System.Collections.Generic;

// 静态数据 - 存档在项目资源中
[CreateAssetMenu(fileName = "ObjectData", menuName = "Game/ObjectData")]
public class SmallObjectStaticData : ScriptableObject {
    [Header("属性定义 (启动即创建)")]
    public List<PropertyDefinition> propertyBlueprints = new List<PropertyDefinition>();
}

[Serializable]
public class SmallObjectDynamicState {
    public float currentHealth;
    // 可以根据需要添加更多动态状态字段
}

public class PropertyDefinition {
    public string name;
    public float initialValue;
}