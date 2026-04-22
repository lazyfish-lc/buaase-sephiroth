using System.Collections.Generic;
using UnityEngine;


public enum InputActionType {
    None,
    Movement,       // 移动（轴向）
    Interact,       // 交互
    SwitchTime,     // 切换时空
    ValueModify     // 数值修改
}

public static class InputConfig {
    public const string HorizontalAxis = "Horizontal";
    public const string VerticalAxis = "Vertical";
    public const string Interact = "Interact";
    public const string SwitchTime = "SwitchTime";
    public const string Scroll = "Mouse ScrollWheel";
}