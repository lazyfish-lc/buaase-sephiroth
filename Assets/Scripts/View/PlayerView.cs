using UnityEngine;

public class PlayerView : MonoBehaviour {
    public Animator animator;
    public SmallObject ownerController;

    // 更新移动和朝向参数
    public void UpdateMovement(Vector2 dir, bool isMoving) {
        animator.SetBool("IsRunning", isMoving);
        animator.SetFloat("DirX", dir.x);
        animator.SetFloat("DirY", dir.y);
    }

    // 播放攻击动画
    public void PlayAttack(bool isMoving) {
        animator.SetTrigger("Attack");
        if (isMoving) {
            animator.SetFloat("RunAmount", 1f);
        } else {
            animator.SetFloat("RunAmount", 0f);
        }
    }

    public void OnHitFrame() {
        // 通知 Controller 进行实际的物理碰撞探测
        if (ownerController is PlayerSmallObject player) {
            player.ExecuteDamageDetection();
        } else if (ownerController is MonsterSmallObject monster) {
            monster.ExecuteDamageDetection();
        }
    }
}