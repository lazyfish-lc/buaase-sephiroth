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
    /// 根据结局编号获取对应的硬编码结局文字。
    /// </summary>
    private string GetEndingText(int endingId)
    {
        switch (endingId)
        {
            case 0: return
                "结局 A：草率的正义\n\n"
                + "你相信了助手的自首。\n"
                + "案件以「为爱杀人」的名义结案，助手被带走。\n\n"
                + "然而，作家妻子始终没有不在场证明，\n"
                + "座钟的异常无人追问，地上的水渍也无人深究。\n\n"
                + "真正的凶手，依然逍遥法外。\n"
                + "—— 有些真相，一旦错过就不再。";

            case 1: return
                "结局 B：沉默的替罪羊\n\n"
                + "你认定妻子因感情破裂而杀害了作家。\n"
                + "内向而不幸的她，在审讯中百口莫辩。\n\n"
                + "疯女仆的证词只能证明她七点半到家，\n"
                + "却无法为她洗脱「没有不在场证明」的嫌疑。\n\n"
                + "真相被掩埋在冰冷的玫瑰花瓣之下。\n"
                + "—— 偏见，有时比凶器更锋利。";

            case 2: return
                "结局 C：未完成的拼图\n\n"
                + "你揭穿了惊人的手法——\n"
                + "冰弩箭是凶器，快进标签制造了假不在场证明。\n"
                + "助手就是真凶，无可辩驳。\n\n"
                + "但为什么？\n"
                + "作案手法已经水落石出，\n"
                + "可真正的动机，还隐藏在黑暗之中……\n\n"
                + "—— 知道「如何」，却不知道「为何」。";

            case 3: return
                "结局 D：被掩埋的声音\n\n"
                + "助手的原稿和日记揭示了惊人的事实——\n"
                + "那些轰动文坛的作品，竟全部出自助手之手。\n"
                + "作家只是一个窃取者，而助手是影子写手。\n\n"
                + "你以「嫉妒与报复」结案。\n"
                + "可是，日记里还有一些字句，\n"
                + "被一层若有若无的「虚伪」所笼罩……\n\n"
                + "—— 真相之上，还有真相。";

            case 4: return
                "结局 E：恶意的形状\n\n"
                + "你移走了那层「虚伪」的标签。\n"
                + "真相的最后一角，终于露出全貌。\n\n"
                + "助手从未被剽窃。那些手稿是他精心伪造的——\n"
                + "花费数年模仿笔迹，只为栽赃。\n"
                + "没有什么影子写手，只有纯粹的恨。\n\n"
                + "作家太善良、太优秀、太无私了。\n"
                + "而正是这光芒，刺痛了助手的自卑与嫉妒。\n\n"
                + "「好好好，我一定要让你付出代价。」\n\n"
                + "凶器会融化，钟表会倒转，\n"
                + "但恶意——永远不会自己消失。\n"
                + "—— 完整真相，终见天日。";

            default: return "结局";
        }
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
