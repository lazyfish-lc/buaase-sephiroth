using System.Collections.Generic;
using UnityEngine;


public enum InputActionType {
    None,
    Movement,       // 移动（轴向）
    Interact,       // 交互
    SwitchTime,     // 切换时空
    ValueModify,     // 数值修改
    Attack,          // 攻击
}

public static class InputConfig {
    public const string Horizontal = "Horizontal";
    public const string Vertical = "Vertical";
    public const string Interact = "Interact";
    public const string SwitchTime = "SwitchTime";
    public const string Scroll = "Mouse ScrollWheel";
    public const string Settings = "Settings";
    public const string Menu = "Menu";
    public const string Backpack = "Backpack";
    public const string Attack = "Attack";
    public const string ActionClick = "ActionClick";
    public const string Label = "Label";
    public const KeyCode ShowPropertyKey = KeyCode.P;
}