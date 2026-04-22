using UnityEngine;

public class PlayerSmallObject : SmallObject {
    public float moveSpeed = 5f;
    public Rigidbody rb;

     public override void Start() {
        base.Start();
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    public override void OnMoveAction(Vector3 moveDir) {
        Vector3 direction = Vector3.zero;
        direction = moveDir.normalized;
        rb.linearVelocity = new Vector3(direction.x * moveSpeed, rb.linearVelocity.y * moveSpeed, direction.z);
    }

    public override void OnInteractAction() {
        Debug.Log("玩家尝试自我交互（如打开背包）");
    }
}