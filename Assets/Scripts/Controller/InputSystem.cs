using UnityEngine;
using System;

public class IOSubsystem : MonoBehaviour {
    public static IOSubsystem Instance;
    
    [Header("Settings")]
    public LayerMask interactableLayer;
    public SmallObject playerObject; // 引用玩家对象

    void Awake() { Instance = this; }

    void Update() {
        // 1. 处理映射表中的按键
        foreach (var mapping in InputConfig.KeyMap) {
            if (Input.GetKey(mapping.Key)) {
                TriggerAction(mapping.Value, Input.GetKeyDown(mapping.Key));
            }
        }

        // 2. 处理鼠标滚轮（数值修改玩法）
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f) {
            TriggerValueAction(scroll);
        }
    }

    private void TriggerAction(InputActionType action, bool isFirstDown) {
        InputEventData data = new InputEventData {
            actionType = action,
            mousePosition = Input.mousePosition
        };

        // 确定接收者
        if (IsMovementAction(action)) {
            // 移动类行为发给玩家
            playerObject?.HandleInput(data);
        } else if (isFirstDown) {
            // 交互类行为（仅在按下那一帧触发）发给鼠标指向的对象
            data.target = GetObjectUnderMouse();
            data.target?.HandleInput(data);
            
            // 全局行为（如切换时间）可以发给一个全局管理器或玩家
            if (action == InputActionType.SwitchTime) {
                GameSceneManager.Instance?.ToggleTimeVision();
            }
        }
    }

    private void TriggerValueAction(float scrollDelta) {
        InputEventData data = new InputEventData {
            actionType = InputConfig.ScrollAction,
            value = scrollDelta,
            target = GetObjectUnderMouse()
        };
        data.target?.HandleInput(data);
    }

    private SmallObject GetObjectUnderMouse() {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer)) {
            return hit.collider.GetComponent<SmallObject>();
        }
        return null;
    }

    private bool IsMovementAction(InputActionType action) {
        return action == InputActionType.MoveForward || action == InputActionType.MoveBackward ||
               action == InputActionType.MoveLeft || action == InputActionType.MoveRight;
    }
}