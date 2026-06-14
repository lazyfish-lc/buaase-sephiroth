using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EndUI : MonoBehaviour
{
    public static EndUI Instance;

    [Header("黑色遮罩")]
    public CanvasGroup blackOverlay;        // 黑色全屏遮罩（CanvasGroup 控制透明度）

    [Header("结局文字")]
    public TMP_Text endingText;             // 用于显示结局文字的 TextMeshPro 文本
    public CanvasGroup textCanvasGroup;     // 文字的 CanvasGroup（用于淡入淡出）

    [Header("谢幕滚动图片")]
    public RawImage creditsRawImage;               // 谢幕图片（RawImage）
    public float creditsScrollSpeed = 60f;          // 预留参数：滚动速度（像素/秒）

    [Header("预设结局文字")]
    [TextArea(3, 10)]
    public string[] endingTexts = new string[]
    {
        "结局一：预设文字未配置",
        "结局二：预设文字未配置",
        "结局三：预设文字未配置",
    };

    [Header("时间设置")]
    public float blackFadeInDuration = 1f;   // 黑屏淡入时长
    public float textFadeInDuration = 1f;    // 文字淡入时长
    public float textDisplayDuration = 3f;   // 文字显示保持时长
    public float textFadeOutDuration = 1f;   // 文字淡出时长
    public float blackFadeOutDuration = 1f;  // 黑屏淡出时长
    public float creditsScrollDuration = 0f;  // 谢幕滚动总时长（0 = 按 speed 自动计算）

    [Header("场景切换")]
    public string startMenuSceneName = "StartMenuScene"; // 谢幕结束后切换的目标场景

    /// <summary>
    /// 当前结局编号（只读，由 PendingEndingId 在 Start 时赋值）。
    /// 外部应在加载此场景前设置 EndUI.PendingEndingId。
    /// </summary>
    public int endingId { get; private set; }

    /// <summary>
    /// 跨场景传递的结局编号。加载 End 场景前设置此值，
    /// EndUI.Start 会读取并自动播放对应结局。
    /// </summary>
    public static int PendingEndingId = 0;

    private bool hasPlayed = false;

    void Awake()
    {
        // 激活自身及所有父级（从根到叶）
        ActivateHierarchy();

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        Debug.Log($"[EndUI] Awake — 单例已就绪, activeInHierarchy={gameObject.activeInHierarchy}, PendingEndingId={PendingEndingId}");

        // 初始隐藏所有 UI
        if (blackOverlay != null)
        {
            blackOverlay.alpha = 0f;
            blackOverlay.interactable = false;
            blackOverlay.blocksRaycasts = false;
        }
        else Debug.LogWarning("[EndUI] blackOverlay 未赋值！");

        if (textCanvasGroup != null)
        {
            textCanvasGroup.alpha = 0f;
        }
        else Debug.LogWarning("[EndUI] textCanvasGroup 未赋值！");

        if (endingText != null)
        {
            endingText.text = "";
        }
        else Debug.LogWarning("[EndUI] endingText 未赋值！");

        if (creditsRawImage != null)
        {
            creditsRawImage.gameObject.SetActive(false);

            // 诊断：检查谢幕图片是否有纹理
            if (creditsRawImage.texture == null)
                Debug.LogWarning("[EndUI] creditsRawImage 没有纹理！请在 Inspector 中拖入谢幕图片");
        }
        else Debug.LogWarning("[EndUI] creditsRawImage 未赋值！");

        // 不在这里 StartCoroutine —— 交给 TryPlay
    }

    void Start()
    {
        TryPlay();
    }

    /// <summary>
    /// 激活从根到自身的整条层级链。
    /// </summary>
    private void ActivateHierarchy()
    {
        if (gameObject.activeInHierarchy) return;

        // 收集层级链（从根到自身）
        var chain = new System.Collections.Generic.List<GameObject>();
        Transform t = transform;
        while (t != null)
        {
            chain.Add(t.gameObject);
            t = t.parent;
        }
        // 反转：从根到叶依次激活
        chain.Reverse();
        foreach (var go in chain)
        {
            if (!go.activeSelf)
            {
                go.SetActive(true);
                Debug.Log($"[EndUI] 激活: {go.name}");
            }
        }
    }

    /// <summary>
    /// 尝试启动播放（仅当物体在层级中激活且尚未播放时）。
    /// </summary>
    private void TryPlay()
    {
        if (hasPlayed) return;
        if (!gameObject.activeInHierarchy) return; // 必须等层级激活

        hasPlayed = true;
        endingId = PendingEndingId;
        Debug.Log($"[EndUI] TryPlay — 开始播放结局 {endingId}, 文字: {GetEndingText(endingId)}");
        StartCoroutine(PlayEndingSequence());
    }

    private IEnumerator PlayEndingSequence()
    {
        Debug.Log("[EndUI] 协程启动 — 等待加载完成...");

        // ========== 1. 等待加载完成 ==========
        while (LoadingUI.isLoading)
        {
            yield return null;
        }
        // 再等一帧确保场景完全就绪
        yield return null;

        Debug.Log("[EndUI] 加载已完成 — 开始黑屏淡入");

        // ========== 2. 获取结局文字 ==========
        string text = GetEndingText(endingId);
        Debug.Log($"[EndUI] 结局文字: \"{text}\"");

        // ========== 3. 黑屏淡入 ==========
        if (blackOverlay != null)
        {
            // 确保文字层在黑屏之上渲染（sibling index 越大越靠前）
            if (textCanvasGroup != null && textCanvasGroup.transform.GetSiblingIndex() <= blackOverlay.transform.GetSiblingIndex())
            {
                textCanvasGroup.transform.SetAsLastSibling();
                Debug.Log("[EndUI] 已调整文字层级到黑屏之上");
            }

            blackOverlay.interactable = true;
            blackOverlay.blocksRaycasts = true;
            yield return StartCoroutine(FadeCanvasGroup(blackOverlay, 0f, 1f, blackFadeInDuration));
        }
        Debug.Log("[EndUI] 黑屏淡入完成 — 开始文字淡入");

        // ========== 4. 文字缓慢淡入 ==========
        if (endingText != null)
        {
            endingText.text = text;
        }
        if (textCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeInDuration));
        }
        Debug.Log("[EndUI] 文字淡入完成 — 保持显示");

        // ========== 5. 保持显示 ==========
        yield return new WaitForSeconds(textDisplayDuration);

        // ========== 6. 文字淡出 ==========
        Debug.Log("[EndUI] 开始文字淡出");
        if (textCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, textFadeOutDuration));
        }

        // ========== 7. 黑屏淡出 ==========
        Debug.Log("[EndUI] 开始黑屏淡出");
        if (blackOverlay != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(blackOverlay, 1f, 0f, blackFadeOutDuration));
            blackOverlay.interactable = false;
            blackOverlay.blocksRaycasts = false;
            // 淡出完成后彻底关闭黑屏物体，避免遮挡后续谢幕
            blackOverlay.gameObject.SetActive(false);
            Debug.Log("[EndUI] 黑屏物体已关闭");
        }

        // ========== 8. 谢幕滚动（图片位置从下往上移动） ==========
        Debug.Log("[EndUI] 开始谢幕滚动");
        if (creditsRawImage != null)
        {
            creditsRawImage.gameObject.SetActive(true);
            creditsRawImage.color = Color.white;
            creditsRawImage.enabled = true;

            RectTransform rt = creditsRawImage.rectTransform;

            // 获取屏幕尺寸
            float screenW = ((RectTransform)creditsRawImage.canvas.transform).rect.width;
            float screenH = ((RectTransform)creditsRawImage.canvas.transform).rect.height;

            // 按纹理比例计算显示尺寸（宽度撑满屏幕）
            float texAspect = 1f;
            if (creditsRawImage.texture != null)
                texAspect = (float)creditsRawImage.texture.width / creditsRawImage.texture.height;
            float displayW = screenW;
            float displayH = displayW / texAspect;

            // 锚定顶部居中，pivot 在顶部
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(displayW, displayH);

            // 初始位置：图片顶部 = 屏幕顶部（anchoredPosition.y = 0）
            rt.anchoredPosition = Vector2.zero;

            // 滚动到图片底部到达屏幕顶部结束
            float scrollDistance = displayH - screenH;
            if (scrollDistance <= 0f) scrollDistance = displayH; // 图片比屏幕短则完全滚出

            // 计算滚动时长
            float duration = creditsScrollDuration;
            if (duration <= 0f)
                duration = scrollDistance / Mathf.Max(creditsScrollSpeed, 1f);

            Debug.Log($"[EndUI] 谢幕 — screen={screenW}x{screenH}, display={displayW:F0}x{displayH:F0}, scrollDist={scrollDistance:F0}px, duration={duration:F1}s");

            // 确保渲染在最上层 + 修复父级 CanvasGroup
            creditsRawImage.transform.SetAsLastSibling();
            CanvasGroup parentCg = creditsRawImage.GetComponentInParent<CanvasGroup>();
            if (parentCg != null && parentCg.alpha < 1f)
            {
                parentCg.alpha = 1f;
                Debug.Log($"[EndUI] 已修复父级 CanvasGroup '{parentCg.name}' alpha → 1");
            }

            // 逐帧移动图片向上
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                rt.anchoredPosition = new Vector2(0f, scrollDistance * t);
                yield return null;
            }
            rt.anchoredPosition = new Vector2(0f, scrollDistance);

            creditsRawImage.gameObject.SetActive(false);
            Debug.Log("[EndUI] 谢幕滚动结束");
        }
        else
        {
            Debug.LogWarning("[EndUI] creditsRawImage 为空，跳过谢幕");
        }

        // ========== 9. 切换到 StartMenu 场景 ==========
        Debug.Log($"[EndUI] 切换到场景: {startMenuSceneName}");
        SceneManager.LoadScene(startMenuSceneName);
    }

    /// <summary>
    /// 根据结局编号获取对应的预设文字。
    /// </summary>
    private string GetEndingText(int endingId)
    {
        if (endingTexts == null || endingTexts.Length == 0)
        {
            return "结局";
        }
        if (endingId >= 0 && endingId < endingTexts.Length)
        {
            return endingTexts[endingId];
        }
        // 超出范围时返回最后一个结局文字作为兜底
        return endingTexts[endingTexts.Length - 1];
    }

    void Update()
    {
        // 兜底：如果 Awake/Start 都没成功启动，每帧重试直到成功
        if (!hasPlayed)
        {
            TryPlay();
        }
    }

    /// <summary>
    /// 通用的 CanvasGroup 淡入/淡出协程。
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            cg.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}
