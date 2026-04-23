using UnityEngine;

public class PlayerView : MonoBehaviour {
    public Animator animator;

    // 更新移动和朝向参数
    public void UpdateMovement(Vector2 dir, bool isMoving) {
        animator.SetBool("IsMoving", isMoving);
        if (isMoving) {
            animator.SetFloat("DirX", dir.x);
            animator.SetFloat("DirY", dir.y);
        }
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
}