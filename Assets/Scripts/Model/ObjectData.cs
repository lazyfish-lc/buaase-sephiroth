using System;
using UnityEngine;

// 静态数据 - 存档在项目资源中
[CreateAssetMenu(fileName = "ObjectData", menuName = "Game/ObjectData")]
public class SmallObjectData : ScriptableObject {
    public string objectName;
    public float maxHealth;
    public bool isInteractable;
    public GameObject viewPrefab; // 对应的模型
}

// 动态数据 - 运行时状态
[Serializable]
public class SmallObjectState {
    public float currentHealth;
    public bool isDestroyed = false;
    public bool boolState; // 通用状态：门开/关，树倒/立
    public Vector3 position;
}