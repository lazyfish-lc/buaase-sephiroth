using UnityEngine;
using UnityEngine.Tilemaps;

public class DoorSmallObject : SmallObject, IDialogueActionReceiver {
    // 当对象被上锁时使用的碰撞体（由编辑器或运行时赋值）
    public BoxCollider2D lockCollider;
    // 展示门处于被锁定时的瓦片图层
    public Tilemap lockedTilemap;
    // 展示门处于开启时的瓦片图层
    public Tilemap openTilemap;
    // 展示门处于被锁定时的瓦片头顶图层
    public Tilemap lockedTilemapTop;
    // 展示门处于开启时的瓦片头顶图层
    public Tilemap openTilemapTop;
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
        Debug.Log("DoorSmallObject Awake: " + gameObject.name + ", 初始状态: " + (doorState != null && doorState.isOpen ? "开启" : "关闭"));
        // 根据初始状态刷新锁碰撞体
        RefreshLockCollider();
    }

    public override void OnInteractAction(InputEventData data) {
        if (doorState == null) {
            Debug.LogWarning("门状态类型配置错误，无法交互");
            return;
        }

        // OpenDoor();
        // NotifyStateChange();
    }

    public string DialogueActionId => doorStaticData != null && !string.IsNullOrWhiteSpace(doorStaticData.doorName)
        ? doorStaticData.doorName
        : staticData != null && !string.IsNullOrWhiteSpace(staticData.objectName)
            ? staticData.objectName
            : gameObject.name;

    public void ReceiveDialogueAction() {
        OpenDoor();
        NotifyStateChange();
    }

    private global::LockLabel FindLockLabel() {
        if (dynamicState?.smallObjectLabels == null) return null;

        foreach (var label in dynamicState.smallObjectLabels) {
            if (label is global::LockLabel lockLabel) {
                return lockLabel;
            }
        }

        return null;
    }

    public void ToggleDoor() {
        if (doorState == null) return;

        doorState.isOpen = !doorState.isOpen;
        Debug.Log("门状态: " + (doorState.isOpen ? "开启" : "关闭"));
        RefreshLockCollider();
    }

    public void OpenDoor() {
        if (doorState == null) return;
        if (doorState.isOpen) return;

        var lockLabel = FindLockLabel();
        if (lockLabel == null) {
            Debug.LogWarning($"{gameObject.name} 未找到 LockLabel，无法通过标签打开门");
            return;
        }

        lockLabel.Open();
        Debug.Log("门通过 LockLabel 打开");
    }

    public void CloseDoor() {
        if (doorState == null) return;
        if (!doorState.isOpen) return;

        doorState.isOpen = false;
        Debug.Log("门状态: 关闭");
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
        // 同步瓦片显示
        RefreshTilemaps();
    }

    // 返回当前对象是否被 LockLabel 锁住
    private bool IsLocked() {
        if (dynamicState == null || dynamicState.smallObjectLabels == null) return false;
        foreach (var lbl in dynamicState.smallObjectLabels) {
            if (lbl is global::LockLabel lockLbl && lockLbl.IsLocked) return true;
        }
        return false;
    }

    // 根据门的锁定/开启状态显示对应的 Tilemap
    public void RefreshTilemaps() {
        bool showLock = IsLocked();
        if (lockedTilemap != null) lockedTilemap.gameObject.SetActive(showLock);
        if (openTilemap != null) openTilemap.gameObject.SetActive(!showLock);
        if (lockedTilemapTop != null) lockedTilemapTop.gameObject.SetActive(showLock);
        if (openTilemapTop != null) openTilemapTop.gameObject.SetActive(!showLock);
    }
}