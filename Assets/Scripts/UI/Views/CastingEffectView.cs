using UnityEngine;

public class CastingEffectView : MonoBehaviour {
    // 引用它的控制器
    public ArcherMonsterObject owner;

    // 由“生成动画”的第 N 帧触发
    public void OnFireFrame() {
        Debug.Log("CastingEffectView: OnFireFrame triggered");
        if (owner != null) {
            owner.FireProjectile(); // 通知怪物：可以发射法球了
        }
    }

    // 由动画最后一帧触发（或使用 Destroy(gameObject, duration)）
    public void OnAnimationEnd() {
        Destroy(gameObject);
    }
}