using UnityEngine;

public class ProjectileObject : MonoBehaviour {
    private SmallObject owner;
    private Vector2 moveDir;
    private float damageValue;
    public float speed = 10f;
    public Rigidbody2D rb;

    public void Init(SmallObject owner, Vector2 dir, float damage) {
        this.owner = owner;
        this.moveDir = dir;
        this.damageValue = damage;
        rb.linearVelocity = moveDir * speed;
    }

    private void Update()
    {
        // 可以添加一些生命周期管理，比如飞行一定时间后销毁
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        SmallObject target = other.GetComponent<SmallObject>();
        if (target != null && owner.IsEnemy(target)) {
            target.ReceiveDamage(new DamagePacket(owner, damageValue, moveDir * 5f));
            Destroy(gameObject);
        }
    }
}