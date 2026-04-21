using UnityEngine;

public class PlayerSmallObject : SmallObject {
    public float moveSpeed = 5f;

    public override void OnMoveAction(InputActionType moveDir) {
        Vector3 direction = Vector3.zero;
        if (moveDir == InputActionType.MoveForward) direction = Vector3.forward;
        if (moveDir == InputActionType.MoveBackward) direction = Vector3.back;
        if (moveDir == InputActionType.MoveLeft) direction = Vector3.left;
        if (moveDir == InputActionType.MoveRight) direction = Vector3.right;
        
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }

    public override void OnInteractAction() {
        Debug.Log("玩家尝试自我交互（如打开背包）");
    }
}