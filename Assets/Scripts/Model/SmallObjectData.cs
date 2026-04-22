using System;
using UnityEngine;
using System.Collections.Generic;

// 静态数据 - 存档在项目资源中
[CreateAssetMenu(fileName = "SmallObjectData", menuName = "Game/SmallObjectData")]
public class SmallObjectStaticData : ScriptableObject {
    public string objectName;
    public BigObject ownerBigObject;
    [Header("属性定义 (启动即创建)")]
    public List<PropertyDefinition> propertyBlueprints = new List<PropertyDefinition>();
}

[Serializable]
public class SmallObjectDynamicState {
    public float currentHealth;
    // 可以根据需要添加更多动态状态字段
    public List<ObjectLabel> smallObjectLabels = new List<ObjectLabel>();
}

public class PropertyDefinition {
    public string name;
    public float initialValue;
}