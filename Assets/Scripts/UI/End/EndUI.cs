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

    [Header("谢幕信息文字")]
    public TMP_Text creditsInfoText;                // 谢幕信息文字（TMP_Text，循环淡入淡出）
    public CanvasGroup creditsInfoCanvasGroup;      // 谢幕信息的 CanvasGroup
    public string[] creditsInfoTexts = new string[] // 谢幕信息内容（按顺序循环切换）
    {
        "策划：\n    XXX",
        "程序：\n    XXX",
        "美术：\n    XXX",
        "音乐：\n    XXX",
        "特别鸣谢：\n    XXX",
        "感谢游玩！",
    };
    public float creditsInfoFadeIn = 1f;            // 谢幕信息淡入时间
    public float creditsInfoDisplay = 2f;           // 谢幕信息保持时间
    public float creditsInfoFadeOut = 1f;           // 谢幕信息淡出时间

    [Header("预设结局文字")]
    [TextArea(3, 10)]
    public string[] endingTexts = new string[]
    {
        //预设结局文字，长度应至少与可能的结局编号数量一致，超出范围时使用最后一条作为兜底
    };

    [Header("时间设置")]
    public float blackFadeInDuration = 1f;   // 黑屏淡入时长
    public float textFadeInDuration = 1f;    // 文字淡入时长
    public float textDisplayDuration = 3f;   // 文字显示保持时长
    public float textFadeOutDuration = 1f;   // 文字淡出时长
    public float blackFadeOutDuration = 1f;  // 黑屏淡出时长
    public float creditsScrollDuration = 0f;  // 谢幕滚动总时长（0 = 按 speed 自动计算）

    [Header("结局后黑幕")]
    public float finalBlackDuration = 2f;   // 谢幕结束后黑屏停留时长

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
            if (creditsRawImage.texture == null)
                Debug.LogWarning("[EndUI] creditsRawImage 没有纹理！");
        }
        else Debug.LogWarning("[EndUI] creditsRawImage 未赋值！");

        // 初始隐藏谢幕信息
        if (creditsInfoText != null)
            creditsInfoText.gameObject.SetActive(false);
        if (creditsInfoCanvasGroup != null)
        {
            creditsInfoCanvasGroup.alpha = 0f;
            creditsInfoCanvasGroup.gameObject.SetActive(false);
        }

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
            endingText.color = Color.white;
            endingText.enabled = true;
            endingText.gameObject.SetActive(true);

            // 自动修复 Rect：全屏居中，留边距
            RectTransform textRt = endingText.rectTransform;
            textRt.anchorMin = new Vector2(0f, 0f);
            textRt.anchorMax = new Vector2(1f, 1f);
            textRt.offsetMin = new Vector2(80f, 80f);
            textRt.offsetMax = new Vector2(-80f, -80f);

            Debug.Log($"[EndUI] 文字渲染诊断 — font={endingText.font?.name ?? "NULL"}, " +
                      $"size={endingText.fontSize}, color={endingText.color}, " +
                      $"alignment={endingText.alignment}, textLen={text.Length}, " +
                      $"endingText.activeSelf={endingText.gameObject.activeSelf}, " +
                      $"fontAtlas={endingText.font?.atlasTexture?.name ?? "NULL"}");

            // 检查字体是否支持中文
            if (endingText.font != null && text.Length > 0)
            {
                bool hasChar = endingText.font.HasCharacter(text[0]);
                Debug.Log($"[EndUI] 首个字符 '{text[0]}' (U+{(int)text[0]:X4}) 字体匹配={(hasChar ? "OK" : "缺失!")}");
                if (!hasChar)
                    Debug.LogError($"[EndUI] ❌ 当前字体 '{endingText.font.name}' 不支持中文字符！请换用支持中文的 TMP 字体。");
            }
        }
        else Debug.LogError("[EndUI] ❌ endingText 未赋值！");

        if (textCanvasGroup != null)
        {
            // 确保 CanvasGroup 所在物体激活
            textCanvasGroup.gameObject.SetActive(true);
            textCanvasGroup.transform.SetAsLastSibling();

            Debug.Log($"[EndUI] textCanvasGroup 状态 — activeSelf={textCanvasGroup.gameObject.activeSelf}, " +
                      $"alpha 淡入前={textCanvasGroup.alpha}, siblingIndex={textCanvasGroup.transform.GetSiblingIndex()}");

            yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeInDuration));

            Debug.Log($"[EndUI] textCanvasGroup 淡入后 alpha={textCanvasGroup.alpha}");
        }
        else Debug.LogError("[EndUI] ❌ textCanvasGroup 未赋值！");

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

            // 初始化谢幕信息文字
            if (creditsInfoText != null && creditsInfoCanvasGroup != null && creditsInfoTexts != null && creditsInfoTexts.Length > 0)
            {
                creditsInfoText.gameObject.SetActive(true);
                creditsInfoText.text = creditsInfoTexts[0];
                creditsInfoText.color = Color.white;
                creditsInfoText.enabled = true;
                creditsInfoCanvasGroup.gameObject.SetActive(true);
                creditsInfoCanvasGroup.alpha = 0f;
                creditsInfoCanvasGroup.transform.SetAsLastSibling();
                Debug.Log($"[EndUI] 谢幕信息就绪 — {creditsInfoTexts.Length} 条信息");
            }

            // 逐帧移动图片 + 顺序播放谢幕文字（仅一轮）
            float elapsed = 0f;
            int infoIndex = 0;
            float infoTimer = 0f;
            int infoPhase = 0; // 0=淡入, 1=保持, 2=淡出
            bool infoFinished = false; // 全部文字播完标记

            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;

                // 图片滚动
                float t = elapsed / duration;
                rt.anchoredPosition = new Vector2(0f, scrollDistance * t);

                // 谢幕信息文字顺序播放（仅一轮，播完停留在最后一条淡出状态）
                if (!infoFinished && creditsInfoText != null && creditsInfoCanvasGroup != null && creditsInfoTexts != null && creditsInfoTexts.Length > 0)
                {
                    infoTimer += dt;

                    if (infoPhase == 0) // 淡入
                    {
                        float fadeT = infoTimer / Mathf.Max(creditsInfoFadeIn, 0.01f);
                        creditsInfoCanvasGroup.alpha = Mathf.Clamp01(fadeT);
                        if (fadeT >= 1f)
                        {
                            infoTimer = 0f;
                            infoPhase = 1;
                        }
                    }
                    else if (infoPhase == 1) // 保持显示
                    {
                        if (infoTimer >= creditsInfoDisplay)
                        {
                            infoTimer = 0f;
                            infoPhase = 2;
                        }
                    }
                    else // 淡出
                    {
                        float fadeT = infoTimer / Mathf.Max(creditsInfoFadeOut, 0.01f);
                        creditsInfoCanvasGroup.alpha = 1f - Mathf.Clamp01(fadeT);
                        if (fadeT >= 1f)
                        {
                            infoIndex++;
                            if (infoIndex >= creditsInfoTexts.Length)
                            {
                                // 全部播完，保持隐藏
                                creditsInfoCanvasGroup.alpha = 0f;
                                infoFinished = true;
                            }
                            else
                            {
                                infoTimer = 0f;
                                infoPhase = 0;
                                creditsInfoText.text = creditsInfoTexts[infoIndex];
                            }
                        }
                    }
                }

                yield return null;
            }
            rt.anchoredPosition = new Vector2(0f, scrollDistance);

            creditsRawImage.gameObject.SetActive(false);
            if (creditsInfoCanvasGroup != null) creditsInfoCanvasGroup.alpha = 0f;
            Debug.Log("[EndUI] 谢幕滚动结束");

            // ========== 9. 黑屏淡入 + 停 BGM ==========
            Debug.Log("[EndUI] 谢幕结束，黑屏淡入");
            if (blackOverlay != null)
            {
                blackOverlay.gameObject.SetActive(true);
                blackOverlay.alpha = 0f;
                blackOverlay.interactable = true;
                blackOverlay.blocksRaycasts = true;
                blackOverlay.transform.SetAsLastSibling();
                yield return StartCoroutine(FadeCanvasGroup(blackOverlay, 0f, 1f, blackFadeInDuration));
            }

            // 停止 BGM
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.Stop();
                Debug.Log("[EndUI] BGM 已停止");
            }

            // 黑屏停留
            yield return new WaitForSeconds(finalBlackDuration);

            // ========== 10. 切换到 StartMenu 场景 ==========
            Debug.Log($"[EndUI] 切换到场景: {startMenuSceneName}");
            SceneManager.LoadScene(startMenuSceneName);
        }
        else
        {
            Debug.LogWarning("[EndUI] creditsRawImage 为空，跳过谢幕");
            // 无谢幕时直接过渡到黑屏
            if (blackOverlay != null)
            {
                blackOverlay.gameObject.SetActive(true);
                blackOverlay.alpha = 0f;
                blackOverlay.interactable = true;
                blackOverlay.blocksRaycasts = true;
                blackOverlay.transform.SetAsLastSibling();
                yield return StartCoroutine(FadeCanvasGroup(blackOverlay, 0f, 1f, blackFadeInDuration));
            }
            if (AudioManager.Instance != null) AudioManager.Instance.Stop();
            yield return new WaitForSeconds(finalBlackDuration);
            SceneManager.LoadScene(startMenuSceneName);
        }
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
