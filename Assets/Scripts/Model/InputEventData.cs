using UnityEngine;

public struct InputEventData {
    public InputActionType actionType;
    public float value;             // 数值增量
    public Vector3 moveVector;      // 移动向量
    public string propertyName;     // 解决问题2：属性名称映射
    public SmallObject target;
}