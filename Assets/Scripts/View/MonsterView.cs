using UnityEngine;
using UnityEngine.UI;
public class MonsterView : MonoBehaviour {
    public Animator animator;
    public SmallObject ownerController;
    public Slider HealthSlider;

    public void SetController(RuntimeAnimatorController controller) {
        if (animator != null && controller != null) {
            animator.runtimeAnimatorController = controller;
        }
    }

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
        UpdateHealthBarLocation(HealthSlider.value);
    }
    public void UpdateHealthBarLocation(float Value) {
        // 默认位置是value为1时血条中心在头顶，所有值的左端是对齐的，计算左右偏移量使其中心在头顶
        // 乘以x缩放值
        float offsetX = (1f - Value) * HealthSlider.GetComponent<RectTransform>().sizeDelta.x/2f * HealthSlider.GetComponent<RectTransform>().localScale.x;
        HealthSlider.GetComponent<RectTransform>().anchoredPosition = new Vector2(offsetX, HealthSlider.GetComponent<RectTransform>().anchoredPosition.y);
    }
}