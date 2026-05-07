using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingSignObject : SmallObject {
    [Header("Falling Sign Components")]
    [SerializeField] private Rigidbody2D signRigidbody;
    [SerializeField] private Collider2D signCollider;
    [SerializeField] private Collider2D detectionTrigger;

    [Header("Drop Settings")]
    [SerializeField] private float dropDelay = 0.25f;
    [SerializeField] private float fallDuration = 2.0f;
    [SerializeField] private float impactDamage = 25f;
    [SerializeField] private float impactKnockback = 35f;
    [SerializeField] private bool dropOnlyOnce = true;
    [SerializeField] private float detectionCheckInterval = 0.05f;

    private bool isArmed;
    private bool isDropping;
    private bool hasDropped;
    private bool hasLanded;
    private bool hasDealtDamage;
    private bool wasPlayerInsideDetectionZone;
    private float nextDetectionCheckTime;
    private readonly List<Collider2D> overlapBuffer = new List<Collider2D>(8);

    protected override void Awake() {
        base.Awake();
        CacheComponents();
        ResetSignState();
        wasPlayerInsideDetectionZone = IsPlayerInsideDetectionZone();
    }

    protected override SmallObjectDynamicState CreateDynamicState() {
        return new SmallObjectDynamicState();
    }

    public override void OnInteractAction(InputEventData eventData) {
        ArmSign();
    }

    private void CacheComponents() {
        if (signRigidbody == null) {
            signRigidbody = GetComponent<Rigidbody2D>();
        }

        if (signCollider == null) {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders) {
                if (col != null && !col.isTrigger) {
                    signCollider = col;
                    break;
                }
            }
        }

        if (detectionTrigger == null) {
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);
            foreach (var col in colliders) {
                if (col != null && col.isTrigger && col != signCollider) {
                    detectionTrigger = col;
                    break;
                }
            }
        }

        if (signRigidbody == null) {
            Debug.LogWarning($"{gameObject.name} 缺少 Rigidbody2D，招牌无法掉落");
        }

        if (signCollider == null) {
            Debug.LogWarning($"{gameObject.name} 未找到实体碰撞体，招牌掉落后将无法造成碰撞伤害");
        }

        if (detectionTrigger == null) {
            Debug.LogWarning($"{gameObject.name} 未找到触发器碰撞体，无法检测玩家离开");
        }
    }

    private void ResetSignState() {
        isArmed = true;
        isDropping = false;
        hasDropped = false;
        hasLanded = false;
        hasDealtDamage = false;

        if (signRigidbody != null) {
            signRigidbody.bodyType = RigidbodyType2D.Kinematic;
            signRigidbody.gravityScale = 0f;
            signRigidbody.linearVelocity = Vector2.zero;
            signRigidbody.angularVelocity = 0f;
            signRigidbody.freezeRotation = false;
        }

        if (signCollider != null) {
            signCollider.enabled = false;
        }

        if (detectionTrigger != null) {
            detectionTrigger.isTrigger = true;
            detectionTrigger.enabled = true;
        }
    }

    private void ArmSign() {
        if (hasDropped && dropOnlyOnce) return;
        isArmed = true;
    }

    private void Update() {
        if (detectionTrigger == null || isDropping || hasLanded || (hasDropped && dropOnlyOnce)) return;
        if (Time.time < nextDetectionCheckTime) return;

        nextDetectionCheckTime = Time.time + detectionCheckInterval;

        bool isPlayerInside = IsPlayerInsideDetectionZone();
        if (isPlayerInside == wasPlayerInsideDetectionZone) return;

        if (isPlayerInside) {
            HandleDetectionZoneChanged(true);
        } else {
            HandleDetectionZoneChanged(false);
        }

        wasPlayerInsideDetectionZone = isPlayerInside;
    }

    private void HandleDetectionZoneChanged(bool entered) {
        var player = FindPlayerInsideDetectionZone();
        if (player == null) return;

        HandlePlayerTrigger(player, entered ? "进入" : "退出");
    }

    private void HandlePlayerTrigger(PlayerSmallObject player, string triggerType) {
        if (!isArmed || isDropping || (hasDropped && dropOnlyOnce)) return;

        float luck = GetPlayerLuck(player);
        if (Mathf.Approximately(luck, 0f)) {
            Debug.Log($"{gameObject.name} 检测到玩家{triggerType}触发区，Luck=0，开始掉落");
            StartCoroutine(DropAfterDelay());
        }
    }

    private bool IsPlayerInsideDetectionZone() {
        return FindPlayerInsideDetectionZone() != null;
    }

    private PlayerSmallObject FindPlayerInsideDetectionZone() {
        if (detectionTrigger == null) return null;

        overlapBuffer.Clear();
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = true;
        detectionTrigger.Overlap(filter, overlapBuffer);
        foreach (var collider in overlapBuffer) {
            var player = GetPlayerFromCollider(collider);
            if (player != null) {
                return player;
            }
        }

        return null;
    }

    private PlayerSmallObject GetPlayerFromCollider(Collider2D other) {
        if (other == null) return null;

        if (other.CompareTag("Player")) {
            return other.GetComponentInParent<PlayerSmallObject>() ?? other.GetComponent<PlayerSmallObject>();
        }

        return other.GetComponentInParent<PlayerSmallObject>() ?? other.GetComponent<PlayerSmallObject>();
    }

    private IEnumerator DropAfterDelay() {
        if (isDropping || (hasDropped && dropOnlyOnce)) {
            yield break;
        }

        isDropping = true;
        yield return new WaitForSeconds(dropDelay);
        BeginDrop();
    }

    private void BeginDrop() {
        if (signRigidbody == null) {
            return;
        }

        if (signCollider != null) {
            signCollider.enabled = true;
        }

        signRigidbody.bodyType = RigidbodyType2D.Dynamic;
        signRigidbody.gravityScale = 1f;
        signRigidbody.linearVelocity = Vector2.zero;
        signRigidbody.angularVelocity = 0f;

        hasDropped = true;
        StartCoroutine(LandAfterFall());
    }

    private IEnumerator LandAfterFall() {
        if (fallDuration > 0f) {
            yield return new WaitForSeconds(fallDuration);
        }

        LandSign();
    }

    private void LandSign() {
        if (hasLanded) {
            return;
        }

        hasLanded = true;
        isDropping = false;

        if (signRigidbody != null) {
            signRigidbody.linearVelocity = Vector2.zero;
            signRigidbody.angularVelocity = 0f;
            signRigidbody.gravityScale = 0f;
            signRigidbody.bodyType = RigidbodyType2D.Static;
        }

        if (signCollider != null) {
            signCollider.enabled = true;
        }

        if (detectionTrigger != null) {
            detectionTrigger.enabled = false;
        }
    }

    private float GetPlayerLuck(PlayerSmallObject player) {
        if (player == null || player.playerState == null) {
            return 0f;
        }

        if (player.playerState.propertyMap != null && player.playerState.propertyMap.TryGetValue("Luck", out var luckProp)) {
            return luckProp.value;
        }

        Debug.LogWarning($"玩家 {player.name} 缺少 Luck 属性，招牌判定按默认值 0 处理");
        return 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (!hasDropped || hasLanded || hasDealtDamage) return;

        SmallObject target = collision.collider.GetComponentInParent<SmallObject>();
        if (target == null || target == this) return;

        if (target is PlayerSmallObject) {
            DealDamageToTarget(target, collision);
        }
    }

    private void DealDamageToTarget(SmallObject target, Collision2D collision) {
        Vector2 knockback = Vector2.down * impactKnockback;
        if (collision.contactCount > 0) {
            Vector2 contactNormal = collision.GetContact(0).normal;
            knockback = (-contactNormal).normalized * impactKnockback;
        }

        target.ReceiveDamage(new DamagePacket(this, impactDamage, knockback));
        hasDealtDamage = true;

        if (dropOnlyOnce) {
            Destroy(gameObject, 0.05f);
        }
    }
}