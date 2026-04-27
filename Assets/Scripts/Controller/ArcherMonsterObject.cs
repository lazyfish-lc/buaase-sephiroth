using UnityEngine;

public class ArcherMonsterObject : MonsterSmallObject {
    [Header("远程特有配置")]
    public GameObject castingEffectPrefab; // 留在原地的生成动画预制体
    public GameObject projectilePrefab;    // 飞出去的法球预制体
    
    private ArcherMonsterObjectStaticData AData => (ArcherMonsterObjectStaticData)staticData;
    private bool isCasting = false;
    private GameObject currentCastingEffect;

    protected override bool TryAttack() {
        float attackSpeed = monsterState.propertyMap.ContainsKey("AttackSpeed") ? monsterState.propertyMap["AttackSpeed"].value : 1.0f;
        float interval = AData.baseAttackCooldown / attackSpeed;

        if (monsterState.targetPlayer == null) {
            return false;
        }

        if (hitPoint == null) {
            Debug.LogError($"{name} 的 hitPoint 未绑定，无法生成施法特效");
            return false;
        }

        if (castingEffectPrefab == null) {
            Debug.LogError($"{name} 的 castingEffectPrefab 未绑定，无法生成施法特效");
            return false;
        }

        if (Time.time >= monsterState.lastAttackTime + interval) {
            monsterState.lastAttackTime = Time.time;
            StartCoroutine(PerformRangedSequence());
            return true;
        }

        return false;
    }

    private System.Collections.IEnumerator PerformRangedSequence() {
        isCasting = true;
        monsterView.UpdateMovement(OrientationToVector(monsterState.currentOrientation), false); // 施法过程中保持静止

        // 1. 在原地生成“施法特效”
        if (!castingEffectPrefab.activeSelf) {
            Debug.LogWarning($"{name} 的 castingEffectPrefab 当前处于失活，已自动恢复激活");
            castingEffectPrefab.SetActive(true);
        }

        GameObject effectGO = Instantiate(castingEffectPrefab, hitPoint.position, Quaternion.identity);
        if (effectGO == null) {
            Debug.LogError($"{name} 实例化 castingEffectPrefab 失败");
            isCasting = false;
            yield break;
        }

        // 运行时实例强制激活，避免引用对象历史状态影响显示
        effectGO.SetActive(true);
        currentCastingEffect = effectGO;

        if (!effectGO.activeSelf) {
            Debug.LogWarning($"{name} 生成的施法特效处于失活状态：{effectGO.name}");
        }
        // 如果需要特效跟随怪物移动，可以将 effectGO 设为 hitPoint 的子物体
        // effectGO.transform.SetParent(hitPoint); 
        
        CastingEffectView effectView = effectGO.GetComponent<CastingEffectView>();
        if (effectView == null) {
            Debug.LogError($"{effectGO.name} 缺少 CastingEffectView，动画事件不会触发");
        } else {
            effectView.owner = this; // 建立引用
        }
        Debug.Log($"施法特效：{effectGO.name} 已生成，等待动画事件触发发射");
        // 2. 怪物保持静止，直到施法僵直结束
        yield return new WaitForSeconds(AData.castLockTime);
        
        isCasting = false;
    }

    // 由 CastingEffectView 的动画事件回调
    public void FireProjectile() {
        if (monsterState.targetPlayer == null) return;

        // 生成真正的法球
        GameObject ballGO = Instantiate(projectilePrefab, hitPoint.position, Quaternion.identity);
        ProjectileObject projectile = ballGO.GetComponent<ProjectileObject>();
        
        Vector2 fireDir = (monsterState.targetPlayer.position + new Vector3(0, 0.5f, 0) - hitPoint.position).normalized;
        float damage = monsterState.propertyMap["ATK"].value;
        
        projectile.Init(this, fireDir, damage);
    }

    protected override void HandleAIBehavior() {
        if (isCasting) {
            rb.linearVelocity = Vector2.zero; // 施法过程中保持静止
            return;
        }
        base.HandleAIBehavior();
    }

    protected override void OnDeath() {
        Debug.Log($"{staticData.objectName} 已死亡，销毁所有相关特效");
        currentCastingEffect?.SetActive(false); // 确保施法特效被移除
        base.OnDeath();
    }
}