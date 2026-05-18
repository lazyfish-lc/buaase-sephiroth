using UnityEngine;

public class HeightLadder : MonoBehaviour
{
    public int lowLevel = 0;
    public int highLevel = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        HeightMember member = other.GetComponent<HeightMember>();
        if (member == null) return;

        // 进入楼梯：开启过渡态（认为同时在 low 和 high 层）
        member.UpdateCollisionLogic(lowLevel, true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        HeightMember member = other.GetComponent<HeightMember>();
        if (member == null) return;

        // 离开楼梯判定（基于之前讨论的 Y 轴中心判定）
        float exitY = other.bounds.center.y;
        float ladderCenterY = GetComponent<BoxCollider2D>().bounds.center.y;

        if (exitY > ladderCenterY) {
            member.currentLevel = highLevel;
            member.UpdateCollisionLogic(highLevel, false);
        } else {
            member.currentLevel = lowLevel;
            member.UpdateCollisionLogic(lowLevel, false);
        }
    }
}