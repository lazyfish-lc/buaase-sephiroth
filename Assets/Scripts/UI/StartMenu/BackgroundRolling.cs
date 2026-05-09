using UnityEngine;
using UnityEngine.UI;

public class BackgroundRolling : MonoBehaviour
{
    [Header("滚动速度（每秒移动的UV单位）")]
    public float scrollSpeed = 0.2f;   // 正数：向下滚动（图片从上往下移动）
                                       // 负数：向上滚动

    private RawImage rawImage;
    private Rect uvRect;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        if (rawImage == null)
        {
            Debug.LogError("需要 RawImage 组件！");
            return;
        }
        // 保存初始 uvRect
        uvRect = rawImage.uvRect;
    }

    void Update()
    {
        // 每帧增加 Y 偏移（竖直方向滚动）
        uvRect.y += scrollSpeed * Time.deltaTime;

        // 可选：将偏移限制在 [0,1] 范围内（因为 Wrap Mode = Repeat，其实不限制也可以）
        // 但限制可以避免浮点数过大，精度问题可忽略
        uvRect.y %= 1f;

        // 应用新的 uvRect
        rawImage.uvRect = uvRect;
    }
}