using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class GridButtonsFloatController : MonoBehaviour
{
    [Header("默认漂浮参数（缓慢）")]
    [Tooltip("默认上下漂浮幅度（像素）")]
    public float normalFloatAmplitude = 5f;
    [Tooltip("完整上下一次所需时间（秒）")]
    public float floatPeriod = 2f;

    [Header("鼠标悬浮时的增幅")]
    [Tooltip("悬浮时漂浮幅度的倍数（2 = 幅度翻倍）")]
    public float hoverAmplitudeMultiplier = 2.5f;

    [Header("幅度平滑变化速度")]
    [Tooltip("悬浮/离开时幅度变化的速度（秒）")]
    public float amplitudeSmoothTime = 0.15f;

    // 可选：轻微的水平漂移（营造更自然的感觉）
    [Header("水平漂移（可选）")]
    [Tooltip("是否启用轻微的水平漂移")]
    public bool enableHorizontalDrift = false;
    [Tooltip("水平漂移幅度（像素）")]
    public float horizontalAmplitude = 2f;
    [Tooltip("水平漂移周期（秒）")]
    public float horizontalPeriod = 3f;

    // 每个按钮的数据
    private class ButtonData
    {
        public RectTransform rectTransform;
        public Vector3 originalPosition;          // 原始局部位置
        public Quaternion originalRotation;       // 原始旋转（保持无旋转）
        public float currentVerticalAmplitude;    // 当前实际垂直幅度
        public float targetVerticalAmplitude;     // 目标垂直幅度
        public float velVertical;                 // 平滑插值用
    }

    private List<ButtonData> buttons = new List<ButtonData>();
    private bool hasRecordedPositions = false;

    void Start()
    {
        // 获取所有子物体中的 Button
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in allButtons)
        {
            RectTransform rt = btn.GetComponent<RectTransform>();
            if (rt == null) continue;

            ButtonData data = new ButtonData
            {
                rectTransform = rt,
                originalPosition = Vector3.zero,  // 占位，等布局完成后记录
                originalRotation = rt.localRotation,
                currentVerticalAmplitude = normalFloatAmplitude,
                targetVerticalAmplitude = normalFloatAmplitude
            };
            buttons.Add(data);
            AddHoverHandler(btn.gameObject, data);
        }

        // 等待一帧，让 Grid Layout Group 完成布局
        StartCoroutine(RecordPositionsAfterLayout());
    }

    private IEnumerator RecordPositionsAfterLayout()
    {
        yield return null; // 等待一帧
        foreach (ButtonData data in buttons)
        {
            if (data.rectTransform != null)
            {
                data.originalPosition = data.rectTransform.localPosition;
                // 确保原始旋转是单位四元数（一般按钮没有旋转）
                data.rectTransform.localRotation = data.originalRotation;
            }
        }
        hasRecordedPositions = true;
    }

    private void AddHoverHandler(GameObject buttonObj, ButtonData data)
    {
        var handler = buttonObj.AddComponent<ButtonFloatHoverHandler>();
        handler.Init(this, data);
    }

    // 由悬浮处理器调用
    private void OnButtonHoverEnter(ButtonData data)
    {
        data.targetVerticalAmplitude = normalFloatAmplitude * hoverAmplitudeMultiplier;
    }

    private void OnButtonHoverExit(ButtonData data)
    {
        data.targetVerticalAmplitude = normalFloatAmplitude;
    }

    void Update()
    {
        if (!hasRecordedPositions) return;

        // 计算正弦波相位（垂直方向）
        float verticalPhase = Time.time * (Mathf.PI * 2f / floatPeriod);
        float verticalOffsetRaw = Mathf.Sin(verticalPhase);

        // 水平方向（可选）
        float horizontalOffsetRaw = 0f;
        if (enableHorizontalDrift)
        {
            float horizontalPhase = Time.time * (Mathf.PI * 2f / horizontalPeriod);
            horizontalOffsetRaw = Mathf.Sin(horizontalPhase);
        }

        foreach (ButtonData data in buttons)
        {
            if (data.rectTransform == null) continue;

            // 平滑过渡当前幅度到目标幅度
            data.currentVerticalAmplitude = Mathf.SmoothDamp(
                data.currentVerticalAmplitude, data.targetVerticalAmplitude,
                ref data.velVertical, amplitudeSmoothTime);

            // 计算最终偏移
            float verticalOffset = verticalOffsetRaw * data.currentVerticalAmplitude;
            float horizontalOffset = enableHorizontalDrift ? horizontalOffsetRaw * horizontalAmplitude : 0f;

            Vector3 newPos = data.originalPosition;
            newPos.y += verticalOffset;
            newPos.x += horizontalOffset;

            data.rectTransform.localPosition = newPos;

            // 注意：不改变旋转，保持按钮原样
        }
    }

    // 内部类：处理鼠标悬浮事件
    private class ButtonFloatHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private GridButtonsFloatController controller;
        private ButtonData buttonData;

        public void Init(GridButtonsFloatController ctrl, ButtonData data)
        {
            controller = ctrl;
            buttonData = data;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            controller.OnButtonHoverEnter(buttonData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            controller.OnButtonHoverExit(buttonData);
        }
    }
}