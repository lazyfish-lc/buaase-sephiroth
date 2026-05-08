using System;
using UnityEngine;

public class DogObject : MonsterSmallObject {
    [SerializeField] private float luckThreshold = 50f;
    [SerializeField] private float chaseSpeed = 3.5f;
    [SerializeField] private float berserkSpeedMultiplier = 1.5f;
    [SerializeField] private float berserkAttackCooldownMultiplier = 0.75f;

    public float LuckThreshold => luckThreshold;
    public PlayerSmallObject CurrentPlayer => monsterState?.targetPlayer != null ? monsterState.targetPlayer.GetComponent<PlayerSmallObject>() : null;

    protected override void HandleAIBehavior() {
        if (monsterState == null || monsterState.isDestroyed) return;
        if (monsterView != null && monsterView.IsPlayingAttack()) return;
        if (monsterState.isHurt) return;

        // 先触发标签 Tick，让温顺/狂暴标签自行完成状态切换。
        ILabelOnTick();

        var sensedPlayerTransform = monsterState.targetPlayer;
        var sensedPlayer = sensedPlayerTransform != null ? sensedPlayerTransform.GetComponent<PlayerSmallObject>() : null;

        if (sensedPlayer == null) {
            StopMoving();
            return;
        }

        if (HasLabelOnSelf("Docile")) {
            StopMoving();
            return;
        }

        if (!HasLabelOnSelf("Berserk")) {
            StopMoving();
            return;
        }

        Vector3 direction = (sensedPlayerTransform.position - transform.position).normalized;
        UpdateMonsterOrientation(direction);

        float speed = GetCurrentChaseSpeed();
        if (rb != null) {
            rb.linearVelocity = new Vector2(direction.x, direction.y) * speed;
        }
        monsterView?.UpdateMovement(new Vector2(direction.x, direction.y), true);

        if (monsterState.isPlayerInAttackRange) {
            bool attackedThisFrame = TryAttack();
            if (!attackedThisFrame) {
                if (rb != null) {
                    rb.linearVelocity = new Vector2(direction.x, direction.y) * speed;
                }
                monsterView?.UpdateMovement(new Vector2(direction.x, direction.y), true);
            }
        }
    }

    protected override bool TryAttack() {
        if (monsterState == null || monsterStaticData == null) return false;

        float attackSpeed = 1f;
        if (monsterState.propertyMap != null && monsterState.propertyMap.TryGetValue("AttackSpeed", out var attackSpeedProp)) {
            attackSpeed = Mathf.Max(0.01f, attackSpeedProp.value);
        }

        float cooldownMultiplier = HasLabelOnSelf("Berserk") ? berserkAttackCooldownMultiplier : 1f;
        float interval = monsterStaticData.baseAttackCooldown * cooldownMultiplier / attackSpeed;
        if (monsterState != null && monsterState.isPlayerInAttackRange && Time.time >= monsterState.lastAttackTime + interval) {
            monsterState.lastAttackTime = Time.time;
            if (rb != null) rb.linearVelocity = Vector2.zero;
            monsterView?.PlayAttack();
            return true;
        }

        return false;
    }

    private float GetCurrentChaseSpeed() {
        float speed = chaseSpeed;
        if (monsterState != null && monsterState.propertyMap != null && monsterState.propertyMap.TryGetValue("moveSpeed", out var moveSpeedProp)) {
            speed = moveSpeedProp.value;
        }

        if (HasLabelOnSelf("Berserk")) {
            speed *= berserkSpeedMultiplier;
        }

        return Mathf.Max(0f, speed);
    }

    private float GetPlayerLuck(PlayerSmallObject player) {
        if (player == null || player.playerState == null) {
            return 0f;
        }

        if (player.playerState.propertyMap != null && player.playerState.propertyMap.TryGetValue("Luck", out var luckProp)) {
            return luckProp.value;
        }

        return 50f;
    }

    public void ReplaceLabel(ObjectLabel oldLabel, ObjectLabel newLabel) {
        if (oldLabel == null || newLabel == null) return;
        if (oldLabel.owner != null && oldLabel.owner == this) {
            RemoveLabel(oldLabel);
        }
        if (!HasLabelOnSelf(newLabel.labelName)) {
            AddLabel(newLabel);
        }
    }

    private bool HasLabelOnSelf(string labelName) {
        if (monsterState?.smallObjectLabels == null) return false;
        foreach (var label in monsterState.smallObjectLabels) {
            if (label != null && string.Equals(label.labelName, labelName, StringComparison.OrdinalIgnoreCase)) {
                return true;
            }
        }
        return false;
    }

    private void StopMoving() {
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
        }

        if (monsterView != null && monsterState != null) {
            monsterView.UpdateMovement(OrientationToVector(monsterState.currentOrientation), false);
        }
    }
}