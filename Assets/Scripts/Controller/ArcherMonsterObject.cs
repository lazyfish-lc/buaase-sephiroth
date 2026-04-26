using UnityEngine;

public class ArcherMonsterObject : MonsterSmallObject {
    [Header("远程特有配置")]
    public GameObject castingEffectPrefab; // 留在原地的生成动画预制体
    public GameObject projectilePrefab;    // 飞出去的法球预制体
    
    private ArcherMonsterObjectStaticData AData => (ArcherMonsterObjectStaticData)staticData;
    private bool isCasting = false;

    protected override bool TryAttack() {
        float attackSpeed = monsterState.propertyMap.ContainsKey("AttackSpeed") ? monsterState.propertyMap["AttackSpeed"].value : 1.0f;
        float interval = AData.baseAttackCooldown / attackSpeed;

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
        GameObject effectGO = Instantiate(castingEffectPrefab, hitPoint.position, Quaternion.identity);
        // 如果需要特效跟随怪物移动，可以将 effectGO 设为 hitPoint 的子物体
        // effectGO.transform.SetParent(hitPoint); 
        
        CastingEffectView effectView = effectGO.GetComponent<CastingEffectView>();
        effectView.owner = this; // 建立引用

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
        castingEffectPrefab.SetActive(false); // 确保施法特效被销毁
        base.OnDeath();
    }
}