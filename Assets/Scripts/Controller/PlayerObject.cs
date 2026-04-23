using UnityEngine;

public class PlayerSmallObject : SmallObject {
    public Rigidbody2D rb;
    public PlayerView playerView;
    public PlayerObjectDynamicState playerState => dynamicState as PlayerObjectDynamicState;
    public PlayerObjectStaticData playerStaticData => staticData as PlayerObjectStaticData;

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new PlayerObjectDynamicState();
    }

     public override void Start() {
        base.Start();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    public override void OnMoveAction(Vector3 moveDir) {
        Vector3 direction = Vector3.zero;
        direction = moveDir.normalized;
        rb.linearVelocity = new Vector2(direction.x * playerState.moveSpeed, direction.y * playerState.moveSpeed);
        playerState.isRunning = rb.linearVelocity.magnitude > 0.1f;
        UpdateOrientation(moveDir);
        playerView.UpdateMovement(GetAnimationDirection(), playerState.isRunning);

    }

    public override void OnInteractAction() {
        Debug.Log("玩家尝试自我交互（如打开背包）");
    }

    public void Attack() {
        if (playerState == null || playerStaticData == null) {
            Debug.LogWarning("玩家状态或静态数据类型配置错误，无法攻击");
            return;
        }

        float attackSpeed = playerState.propertyMap.ContainsKey("AttackSpeed") ? playerState.propertyMap["AttackSpeed"].value : 1f;
        float cooldown = playerStaticData.baseAttackCooldown / attackSpeed;
        if (Time.time - playerState.lastAttackTime >= cooldown) {
            playerState.lastAttackTime = Time.time;
            Debug.Log("玩家攻击！");
            // 这里可以添加攻击逻辑，比如检测附近的敌人并造成伤害
            playerView.PlayAttack(playerState.isRunning);
        } else {
            Debug.Log("攻击冷却中...");
        }
    }

    public void UpdateOrientation(Vector3 moveDir) {
        if (moveDir.magnitude > 0.1f) {
            if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
                playerState.facingDirection = moveDir.x > 0 ? Orientation.Right : Orientation.Left;
            } else {
                playerState.facingDirection = moveDir.y > 0 ? Orientation.Up : Orientation.Down;
            }
        }
    }

    private Vector2 GetAnimationDirection() {
        if (playerState.facingDirection == Orientation.Up) return new Vector2(0, 1);
        if (playerState.facingDirection == Orientation.Down) return new Vector2(0, -1);
        if (playerState.facingDirection == Orientation.Left) return new Vector2(-1, 0);
        if (playerState.facingDirection == Orientation.Right) return new Vector2(1, 0);
        return new Vector2(0, -1); // 默认朝下
    }
}