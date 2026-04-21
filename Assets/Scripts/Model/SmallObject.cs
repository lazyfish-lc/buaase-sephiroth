using UnityEngine;

public abstract class SmallObject : MonoBehaviour {
    public SmallObjectData staticData;
    public SmallObjectState dynamicState;
    public BigObject ownerBigObject;

    // 接收来自 IO 子系统的分发
    public void HandleInput(InputEventData eventData) {
        switch (eventData.actionType) {
            case InputActionType.Interact:
                OnInteractAction();
                break;
            case InputActionType.ValueModify:
                OnValueModifyAction(eventData.value);
                break;
            case InputActionType.MoveForward:
            case InputActionType.MoveBackward:
            case InputActionType.MoveLeft:
            case InputActionType.MoveRight:
                OnMoveAction(eventData.actionType);
                break;
        }
        NotifyStateChange();
    }

    // 子类实现具体逻辑
    public abstract void OnInteractAction();
    public virtual void OnValueModifyAction(float delta) { }
    public virtual void OnMoveAction(InputActionType moveDir) { }

    public void NotifyStateChange() {
        ownerBigObject?.OnChildStateChanged(this);
    }
}