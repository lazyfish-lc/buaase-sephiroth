using UnityEngine;

public struct InputEventData {
    public InputActionType actionType;
    public float value;             // 用于数值修改（滚轮增量）或移动权重
    public Vector3 mousePosition;   // 鼠标位置
    public SmallObject target;      // 射线检测到的目标对象
}