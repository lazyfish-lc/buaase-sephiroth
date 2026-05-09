using UnityEngine;
public class CustomCursor : MonoBehaviour
{
    // 在 Inspector 中拖入你的像素图
    public Texture2D cursorImage; 
    // 设置点击的有效位置（以图片左上角为起点）
    public Vector2 hotSpot = Vector2.zero; 
    // 在 Inspector 里随时切换，体验不同的效果
    public CursorMode mode = CursorMode.Auto; 

    void Start()
    {
        // 注意：如果使用 ForceSoftware 模式，建议配合较大的图片（如 128x128）以查看尺寸变化
        if (cursorImage != null)
        {
            Cursor.SetCursor(cursorImage, hotSpot, mode);
        }
    }
}