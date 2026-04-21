using System.Collections.Generic;
using UnityEngine;

public enum InputActionType {
    None,
    MoveForward, MoveBackward, MoveLeft, MoveRight, // 移动
    Interact,       // 交互（开门、砍树）
    SwitchTime,     // 切换过去/现在
    ValueModify,    // 数值修改
    TagDragStart,   // 标签抓取
    TagDragEnd      // 标签放置
}

public static class InputConfig {
    // 静态按键映射表：物理按键 -> 逻辑行为
    public static Dictionary<KeyCode, InputActionType> KeyMap = new Dictionary<KeyCode, InputActionType>() {
        { KeyCode.W, InputActionType.MoveForward },
        { KeyCode.S, InputActionType.MoveBackward },
        { KeyCode.A, InputActionType.MoveLeft },
        { KeyCode.D, InputActionType.MoveRight },
        { KeyCode.E, InputActionType.Interact },
        { KeyCode.T, InputActionType.SwitchTime },
        { KeyCode.Mouse0, InputActionType.Interact } // 左键也可以是交互
    };

    // 鼠标滚轮特殊处理逻辑映射
    public static InputActionType ScrollAction = InputActionType.ValueModify;
}