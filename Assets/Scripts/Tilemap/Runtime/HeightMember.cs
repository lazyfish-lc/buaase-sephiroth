using UnityEngine;

public class HeightMember : MonoBehaviour
{
    public int currentLevel = 0;
    private Collider2D[] myColliders;

    void Awake() {
        myColliders = GetComponentsInChildren<Collider2D>();
    }

    void Start() {
        // 初始状态：在 n 层正常行走
        UpdateCollisionLogic(currentLevel, false);
    }

    // 核心逻辑：根据层级和是否在楼梯上，计算所有的物理开关
    public void UpdateCollisionLogic(int level, bool isOnLadder)
    {
        HeightObstacle[] allObs = Object.FindObjectsOfType<HeightObstacle>();

        foreach (var obs in allObs)
        {
            bool shouldCollide = false;

            if (!isOnLadder)
            {
                // 【普通状态逻辑】
                // 1. 被当前层的 Edge 挡住 (防止掉落)
                if (obs.type == ObstacleType.Edge && obs.levelIndex == level) shouldCollide = true;
                // 2. 被下一层的 Wall 挡住 (防止进山)
                if (obs.type == ObstacleType.Wall && obs.levelIndex == level + 1) shouldCollide = true;
            }
            else
            {
                // 【楼梯状态逻辑 - 你提出的双层模式】
                // 1. 同时被当前层 n 和 目标层 n+1 的 Edge 挡住 (形成安全通道)
                if (obs.type == ObstacleType.Edge && (obs.levelIndex == level || obs.levelIndex == level + 1)) 
                    shouldCollide = true;
                
                // 2. 被再高一层 n+2 的 Wall 挡住 (防止在楼梯上撞进更深的山)
                if (obs.type == ObstacleType.Wall && obs.levelIndex == level + 2) 
                    shouldCollide = true;

                // 注意：此时 level+1 的 Wall 自动变为 Ignore (因为没匹配到 shouldCollide = true)
            }

            // 应用碰撞规则
            foreach (var myCol in myColliders) {
                Physics2D.IgnoreCollision(myCol, obs.col, !shouldCollide);
            }
        }

        // 渲染排序：楼梯上的渲染层级直接按高层算，保证不被山体遮挡
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (isOnLadder)
        {
            // 如果在楼梯上，为了压过正在爬的那面墙，直接跳到下一层的 Order
            // 例如：从 L0 爬 L1，Order 从 5 变成 15
            sr.sortingOrder = (level + 1) * 10 + 5;
        }
        else
        {
            // 在平地行走
            // 例如：在 L0 走，Order 是 5，与 Wall_L1 (Order 5) 进行 Y 轴竞争
            // 例如：在 L1 走，Order 是 15，与 Wall_L2 (Order 15) 进行 Y 轴竞争
            sr.sortingOrder = level * 10 + 5;
        }
    }
}