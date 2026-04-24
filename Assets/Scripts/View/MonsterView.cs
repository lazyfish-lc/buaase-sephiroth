using UnityEngine;

public class MonsterView : MonoBehaviour {
    public Animator animator;

    public void UpdateMovement(Vector2 dir, bool isMoving) {
        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("DirX", dir.x);
        animator.SetFloat("DirY", dir.y);
        
    }

    public void PlayAttack() {
        animator.SetTrigger("Attack");
    }

    public bool IsPlayingAttack() {
        return animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") 
            && animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f;
    }
    
}