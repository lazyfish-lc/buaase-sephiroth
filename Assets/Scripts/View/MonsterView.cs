using UnityEngine;
using UnityEngine.UI;
public class MonsterView : MonoBehaviour {
    public Animator animator;
    public SmallObject ownerController;
    public Slider HealthSlider;

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

    public void OnHitFrame() {
        // 通知 Controller 进行实际的物理碰撞探测
        if (ownerController is PlayerSmallObject player) {
            player.ExecuteDamageDetection();
        } else if (ownerController is MonsterSmallObject monster) {
            monster.ExecuteDamageDetection();
        }
    }
    public float GetMonsterHealth() {
        if (ownerController != null && ownerController.dynamicState.propertyMap.ContainsKey("Health")) {
            return ownerController.dynamicState.propertyMap["Health"].value;
        }
        return 0f;
    }
    void Update() {
        // 更新血条显示
        float health = GetMonsterHealth();
        if (health > 1000f) {
            HealthSlider.value = 1f;
        } else {
            HealthSlider.value = health / 1000f;
        }
    }
    
}