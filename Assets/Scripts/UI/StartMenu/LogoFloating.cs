using UnityEngine;
using UnityEngine.UI;   // 如果需要访问 RectTransform 的 anchoredPosition

[RequireComponent(typeof(RectTransform))]
public class LogoFloating : MonoBehaviour
{
    [Header("飘移动画参数")]
    [Tooltip("上下浮动的幅度（单位：像素）")]
    public float amplitude = 20f;      // 上下移动的最大偏移量

    [Tooltip("完整上下一次所需时间（秒）")]
    public float period = 2f;           // 周期，数值越小摆动越快

    [Tooltip("是否使用世界坐标的 Y 轴（一般 UI 用局部坐标）")]
    public bool useWorldPosition = false;  // 大世界UI可能用，通常保持 false

    private RectTransform rectTransform;
    private Vector3 startPosition;          // 起始位置（localPosition 或 worldPosition）
    private bool useRectTransform = true;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            useRectTransform = true;
            // 记录起始的 localPosition（相对于父物体）
            startPosition = rectTransform.localPosition;
        }
        else
        {
            useRectTransform = false;
            startPosition = useWorldPosition ? transform.position : transform.localPosition;
        }
    }

    void Update()
    {
        // 计算正弦波偏移值：范围 [-1, 1] -> 映射到 [-amplitude, amplitude]
        float offsetY = Mathf.Sin(Time.time * (Mathf.PI * 2f / period)) * amplitude;

        Vector3 newPos;
        if (useRectTransform)
        {
            newPos = startPosition;
            newPos.y = startPosition.y + offsetY;
            rectTransform.localPosition = newPos;
        }
        else
        {
            newPos = useWorldPosition ? transform.position : transform.localPosition;
            newPos.y = startPosition.y + offsetY;
            if (useWorldPosition)
                transform.position = newPos;
            else
                transform.localPosition = newPos;
        }
    }
}