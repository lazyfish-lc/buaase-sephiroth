using UnityEngine;

// 定义障碍类型：Wall（侧面墙，挡住楼下的人），Edge（边缘，防止楼上的人掉下来）
public enum ObstacleType { Wall, Edge }

public class HeightObstacle : MonoBehaviour 
{
    public int levelIndex;     // 该障碍属于第几层山？
    public ObstacleType type;  // 是墙还是边缘？
    
    public Collider2D col;

    void Awake() {
        col = GetComponent<Collider2D>();
    }
}