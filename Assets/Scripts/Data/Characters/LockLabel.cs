using System;
using UnityEngine;

[Serializable]
public class LockLabel : ObjectLabel {
    // 自定义类名显示：同类所有实例共享该名称
    public override string labelName => "Lock";
    // 标识当前是否被锁定
    public bool IsLocked { get; private set; } = true;
    // 创建的子物体，用于阻挡其他对象
    private GameObject blockingChild;

    // 在挂载时添加阻挡组件
    public override void OnAttach(ILabelOwner owner) {
        base.OnAttach(owner);
        var small = owner as SmallObject;
        if (small == null) return;

        // 创建或复用一个子物体来承载 BoxCollider2D，大小尽量与 small 一致
        if (blockingChild == null) {
            blockingChild = new GameObject("LockBlock");
            blockingChild.transform.SetParent(small.transform, false);
            blockingChild.transform.localPosition = Vector3.zero;
            blockingChild.transform.localRotation = Quaternion.identity;
            blockingChild.transform.localScale = Vector3.one;

            var box = blockingChild.AddComponent<BoxCollider2D>();
            box.isTrigger = false;

            // 尝试拷贝 small 上的 Collider2D 的尺寸和偏移
            var ownCollider = small.GetComponent<Collider2D>();
            if (ownCollider is BoxCollider2D ownBox) {
                box.offset = ownBox.offset;
                box.size = ownBox.size;
            } else if (ownCollider is CircleCollider2D ownCircle) {
                box.offset = ownCircle.offset;
                var dia = ownCircle.radius * 2f;
                box.size = new Vector2(dia, dia);
            } else if (ownCollider != null) {
                // 其它 collider：使用其 bounds 大小的局部近似
                var b = ownCollider.bounds.size;
                var localSize = new Vector2(
                    b.x / Mathf.Max(0.0001f, small.transform.lossyScale.x),
                    b.y / Mathf.Max(0.0001f, small.transform.lossyScale.y)
                );
                box.size = localSize;
                box.offset = Vector2.zero;
            } else {
                // 没有 Collider，尽量使用 SpriteRenderer 的 bounds
                var sr = small.GetComponent<SpriteRenderer>();
                if (sr != null) {
                    var b = sr.bounds.size;
                    var localSize = new Vector2(
                        b.x / Mathf.Max(0.0001f, small.transform.lossyScale.x),
                        b.y / Mathf.Max(0.0001f, small.transform.lossyScale.y)
                    );
                    box.size = localSize;
                    box.offset = Vector2.zero;
                }
            }

            // 将阻挡逻辑放在子物体上，确保碰撞回调在阻挡体上触发
            var behaviour = blockingChild.AddComponent<LockingBehaviour>();
            behaviour.SetLabel(this);
        }

        IsLocked = true;

        // 如果挂载到门对象上，保证门处于关闭状态并刷新门的锁碰撞体
        var door = owner as global::DoorSmallObject;
        if (door != null && door.doorState != null) {
            door.doorState.isOpen = false;
            door.RefreshLockCollider();
            door.NotifyStateChange();
        }
    }

    // 在卸载时移除组件引用并解锁
    public override void OnDetach(ILabelOwner owner) {
        base.OnDetach(owner);
        var small = owner as SmallObject;
        if (small == null) return;

        // 销毁创建的阻挡子物体（若存在）
        if (blockingChild != null) {
            UnityEngine.Object.Destroy(blockingChild);
            blockingChild = null;
        }

        // 若 small 本体上意外存在关联的 LockingBehaviour，且归属于本标签，也一并清理
        var behaviour = small.gameObject.GetComponent<LockingBehaviour>();
        if (behaviour != null && behaviour.Label == this) {
            UnityEngine.Object.Destroy(behaviour);
        }

        IsLocked = false;

        // 如果从门对象上移除，恢复门为开启状态并刷新门的锁碰撞体
        var door = owner as global::DoorSmallObject;
        if (door != null && door.doorState != null) {
            door.doorState.isOpen = true;
            door.RefreshLockCollider();
            door.NotifyStateChange();
        }
    }

    // 对外调用以解锁并从对象上移除标签
    public void Open() {
        Unlock();
    }

    // 对外调用以解锁并从对象上移除标签
    public void Unlock() {
        if (owner != null) {
            // Detach 会从 small.dynamicState.smallObjectLabels 中移除并解绑事件
            Detach();
        }
        // 同时销毁阻挡子物体
        if (blockingChild != null) {
            UnityEngine.Object.Destroy(blockingChild);
            blockingChild = null;
        }
        IsLocked = false;
    }
}

// 辅助组件：挂在被锁住的 SmallObject 上，负责在碰撞时阻止其他 SmallObject 穿过
public class LockingBehaviour : MonoBehaviour {
    public LockLabel Label { get; private set; }

    public void SetLabel(LockLabel label) {
        Label = label;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (Label == null || !Label.IsLocked) return;
        TryBlockCollider(collision.collider, collision.GetContact(0).normal);
    }

    private void OnCollisionStay2D(Collision2D collision) {
        if (Label == null || !Label.IsLocked) return;
        TryBlockCollider(collision.collider, collision.GetContact(0).normal);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (Label == null || !Label.IsLocked) return;
        TryBlockCollider(other, Vector2.zero);
    }

    private void OnTriggerStay2D(Collider2D other) {
        if (Label == null || !Label.IsLocked) return;
        TryBlockCollider(other, Vector2.zero);
    }

    private void TryBlockCollider(Collider2D collider, Vector2 contactNormal) {
        var otherSmall = collider.GetComponent<SmallObject>();
        if (otherSmall == null) return;

        // 尝试阻止其他对象的运动：将其 Rigidbody2D 速度清零并短暂回退一点以避免穿透
        var rb = otherSmall.GetComponent<Rigidbody2D>();
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            if (contactNormal != Vector2.zero) {
                // 将被阻挡对象轻微推回到碰撞方向外
                rb.position = rb.position + contactNormal * 0.05f;
            }
        }
    }
}
