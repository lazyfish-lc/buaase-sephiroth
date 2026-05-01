using UnityEngine;

public class DoorSmallObject : SmallObject {
    // 当对象被上锁时使用的碰撞体（由编辑器或运行时赋值）
    public BoxCollider2D lockCollider;
    public DoorStaticData doorStaticData => staticData as DoorStaticData;
    public DoorObjectDynamicState doorState => dynamicState as DoorObjectDynamicState;
    
    protected override SmallObjectDynamicState CreateDynamicState() {
        return new DoorObjectDynamicState();
    }

    protected override void Awake() {
        base.Awake();
        if (doorState != null) {
            doorState.isOpen = doorStaticData.initiallyOpen;
        }
        // 根据初始状态刷新锁碰撞体
        RefreshLockCollider();
    }

    public override void OnInteractAction(InputEventData data) {
        if (doorState == null) {
            Debug.LogWarning("门状态类型配置错误，无法交互");
            return;
        }

        doorState.isOpen = !doorState.isOpen;
        Debug.Log("门状态: " + (doorState.isOpen ? "开启" : "关闭"));
        // 切换门状态后刷新锁碰撞体的可用性
        RefreshLockCollider();
        NotifyStateChange();
    }

    public override void ReceiveDamage(DamagePacket packet) {
        if (doorState == null) {
            Debug.LogWarning("门状态类型配置错误，无法受击");
            return;
        }

        if (doorState.isDestroyed) return;

        base.ReceiveDamage(packet);
    }

    protected override void OnDeath() {
        doorState.isDestroyed = true;
        Debug.Log($"{staticData.objectName} 已被摧毁");
        gameObject.SetActive(false);
        NotifyStateChange();
    }
    
    public void SetExistence(bool exists) {
        if (doorState == null) return;
        doorState.isDestroyed = !exists;
        this.gameObject.SetActive(exists);
    }

    // 外部调用刷新 lockCollider 的启用状态：当门关闭（isOpen == false）时启用碰撞体以阻挡
    public void RefreshLockCollider() {
        if (lockCollider == null || doorState == null) return;
        // 碰撞体始终保持激活，以便可以在打开时作为触发器检测
        lockCollider.enabled = true;
        // 门打开时不阻挡（触发器）；门关闭时阻挡实体碰撞
        lockCollider.isTrigger = doorState.isOpen;
    }
}