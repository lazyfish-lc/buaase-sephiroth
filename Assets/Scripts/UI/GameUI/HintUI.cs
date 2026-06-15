using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class HintUI : FatherUI
{   
    public static HintUI Instance;
    public CanvasGroup HintCanvas;
    public TMP_Text HintText;
    public static bool isOpen = false;

    /// <summary>
    /// 跨场景传递的待显示提示。场景重载前设置，新 HintUI 的 Start 会消费它。
    /// </summary>
    public static string PendingHintMessage;

    [Header("提示动画设置")]
    public float displayDuration = 2f;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    public float mergeWindowDuration = 0.5f;   // 合并窗口：这段时间内的同类型事件合并为一条

    private Coroutine currentHintCoroutine;
    private bool subscribedToPlayer;
    private bool isWaitingForPlayer;
    private readonly Queue<string> messageQueue = new Queue<string>();

    // 合并缓冲区 — 按物品名称统计数量
    private readonly Dictionary<string, int> pendingAddedCounts = new Dictionary<string, int>();
    private readonly Dictionary<string, int> pendingRemovedCounts = new Dictionary<string, int>();
    private Coroutine mergeAddedCoroutine;
    private Coroutine mergeRemovedCoroutine;

    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        Debug.Log("[HintUI] Start 执行");
        InitHintCanvas();
        TrySubscribeToPlayerEvents();

        // 跨场景待显示消息：等待 loading 结束再弹
        if (!string.IsNullOrEmpty(PendingHintMessage))
        {
            string msg = PendingHintMessage;
            PendingHintMessage = null;
            StartCoroutine(ShowAfterLoading(msg));
        }
    }

    private IEnumerator ShowAfterLoading(string message)
    {
        // 等待加载界面完全关闭
        while (LoadingUI.isLoading)
        {
            yield return null;
        }
        // 再等一帧确保场景完全就绪
        yield return null;
        ShowHint(message);
    }

    void OnEnable()
    {
        // 兜底：如果 Start 时 player 未就绪以致未订阅成功，
        // 当 GameObject 被激活时重试
        if (!subscribedToPlayer)
        {
            Debug.Log("[HintUI] OnEnable 尝试重订阅");
            TrySubscribeToPlayerEvents();
        }
    }

    private void InitHintCanvas()
    {
        if (HintCanvas != null)
        {
            HintCanvas.alpha = 0f;
            HintCanvas.interactable = false;
            HintCanvas.blocksRaycasts = false;
        }
        else
        {
            Debug.LogError("[HintUI] HintCanvas 未赋值！");
        }
    }

    private void TrySubscribeToPlayerEvents()
    {
        if (subscribedToPlayer) return;
        if (UIManager.Instance != null && UIManager.Instance.player != null)
        {
            SubscribeToPlayerEvents();
        }
        else if (!isWaitingForPlayer)
        {
            isWaitingForPlayer = true;
            Debug.LogWarning($"[HintUI] 玩家未就绪，启动协程等待 (UIManager={UIManager.Instance != null}, player={UIManager.Instance?.player != null})");
            StartCoroutine(TrySubscribeToPlayer());
        }
    }

    private void SubscribeToPlayerEvents()
    {
        var p = UIManager.Instance.player;
        p.ItemAddedToBackpack += OnItemAdded;
        p.ItemRemovedFromBackpack += OnItemRemoved;
        subscribedToPlayer = true;
        Debug.Log("[HintUI] 已成功订阅玩家背包事件");
    }

    private IEnumerator TrySubscribeToPlayer()
    {
        float timeout = 10f;
        float elapsed = 0f;
        while (UIManager.Instance == null || UIManager.Instance.player == null)
        {
            elapsed += Time.deltaTime;
            if (elapsed > timeout)
            {
                Debug.LogError("[HintUI] 等待玩家超时！UIManager 或 player 始终为 null，取消订阅");
                isWaitingForPlayer = false;
                yield break;
            }
            yield return null;
        }
        isWaitingForPlayer = false;
        if (!subscribedToPlayer)
        {
            SubscribeToPlayerEvents();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
        if (UIManager.Instance != null && UIManager.Instance.player != null)
        {
            UIManager.Instance.player.ItemAddedToBackpack -= OnItemAdded;
            UIManager.Instance.player.ItemRemovedFromBackpack -= OnItemRemoved;
        }
        subscribedToPlayer = false;
    }

    private void OnItemAdded(Item item)
    {
        if (item == null || string.IsNullOrWhiteSpace(item.itemName)) return;
        string displayName = GetDisplayName(item.itemName);
        Debug.Log($"[HintUI] 加入合并缓冲区 - 获得物品：{displayName}");
        if (pendingAddedCounts.ContainsKey(displayName))
            pendingAddedCounts[displayName]++;
        else
            pendingAddedCounts[displayName] = 1;
        mergeAddedCoroutine = RestartMergeCoroutine(mergeAddedCoroutine, FlushAdded);
    }

    private void OnItemRemoved(string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName)) return;
        string displayName = GetDisplayName(itemName);
        Debug.Log($"[HintUI] 加入合并缓冲区 - 失去物品：{displayName} x{amount}");
        if (pendingRemovedCounts.ContainsKey(displayName))
            pendingRemovedCounts[displayName] += amount;
        else
            pendingRemovedCounts[displayName] = amount;
        mergeRemovedCoroutine = RestartMergeCoroutine(mergeRemovedCoroutine, FlushRemoved);
    }

    private Coroutine RestartMergeCoroutine(Coroutine current, System.Action flushAction)
    {
        if (current != null) StopCoroutine(current);
        return StartCoroutine(MergeCooldown(flushAction));
    }

    private IEnumerator MergeCooldown(System.Action flushAction)
    {
        yield return new WaitForSeconds(mergeWindowDuration);
        flushAction();
    }

    private void FlushAdded()
    {
        if (pendingAddedCounts.Count == 0) return;
        string merged = "获得物品：" + FormatCounts(pendingAddedCounts);
        Debug.Log($"[HintUI] 合并输出 - {merged}");
        PlayItemGetSFX();
        ShowHint(merged);
        pendingAddedCounts.Clear();
    }

    private void FlushRemoved()
    {
        if (pendingRemovedCounts.Count == 0) return;
        string merged = "失去物品：" + FormatCounts(pendingRemovedCounts);
        Debug.Log($"[HintUI] 合并输出 - {merged}");
        PlayItemUseSFX();
        ShowHint(merged);
        pendingRemovedCounts.Clear();
    }

    /// <summary>
    /// 将计数字典格式化为 "名称*数量、名称*数量"，数量为 1 时省略 *1
    /// </summary>
    private static string FormatCounts(Dictionary<string, int> counts)
    {
        var parts = new List<string>();
        foreach (var kv in counts)
        {
            if (kv.Value == 1)
                parts.Add(kv.Key);
            else
                parts.Add($"{kv.Key}*{kv.Value}");
        }
        return string.Join("、", parts);
    }

    private string GetDisplayName(string itemName)
    {
        if (DisplayTable.Instance != null)
        {
            var info = DisplayTable.Instance.GetDisplayInfo(itemName, "Item");
            if (info != null && !string.IsNullOrWhiteSpace(info.displayname))
            {
                return info.displayname;
            }
        }
        // 回退：DisplayTable 不可用时使用原始名称
        return itemName;
    }

    public void ShowHint(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return;
        messageQueue.Enqueue(message);
        if (currentHintCoroutine == null)
        {
            currentHintCoroutine = StartCoroutine(ConsumeQueue());
        }
    }

    private IEnumerator ConsumeQueue()
    {
        if (HintText == null || HintCanvas == null)
        {
            Debug.LogError("[HintUI] HintText 或 HintCanvas 未赋值！");
            messageQueue.Clear();
            currentHintCoroutine = null;
            yield break;
        }

        while (messageQueue.Count > 0)
        {
            string message = messageQueue.Dequeue();
            PlayHintUISFX();
            HintText.text = message;

            // === 淡入 ===
            float elapsed = 0f;
            HintCanvas.interactable = true;
            HintCanvas.blocksRaycasts = true;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                HintCanvas.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            HintCanvas.alpha = 1f;
            isOpen = true;

            // === 保持显示 ===
            yield return new WaitForSeconds(displayDuration);

            // === 淡出 ===
            elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                HintCanvas.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
        }

        HintCanvas.alpha = 0f;
        HintCanvas.interactable = false;
        HintCanvas.blocksRaycasts = false;
        isOpen = false;
        currentHintCoroutine = null;
    }

    public void OpenAndClose()
    {
        if(!isOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    public void Open()
    {
        PlayOpenSFX();
        HintCanvas.alpha = 1f;
        HintCanvas.interactable = true;
        HintCanvas.blocksRaycasts = true;
        isOpen = true;
    }

    public void Close()
    {
        PlayCloseSFX();
        HintCanvas.alpha = 0f;
        HintCanvas.interactable = false;
        HintCanvas.blocksRaycasts = false;
        isOpen = false;
    }
}
