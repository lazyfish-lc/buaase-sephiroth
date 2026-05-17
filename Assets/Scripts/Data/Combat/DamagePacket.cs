using UnityEngine;

public class DamagePacket {
    public SmallObject attacker;    // 攻击者
    public float damageValue;         // 伤害数值
    public Vector2 knockbackForce;  // 击退力度

    public DamagePacket(SmallObject attacker, float damage, Vector2 knockback) {
        this.attacker = attacker;
        this.damageValue = damage;
        this.knockbackForce = knockback;
    }
}