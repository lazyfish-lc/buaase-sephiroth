using UnityEngine;

public class NPCView : MonoBehaviour {
    public Animator animator;

    public void SetController(RuntimeAnimatorController controller) {
        if (animator != null && controller != null) {
            animator.runtimeAnimatorController = controller;
        }
    }
    
}