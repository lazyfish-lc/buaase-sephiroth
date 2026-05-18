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

    [Header("提示动画设置")]
    public float displayDuration = 2f;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.5f;
    public float mergeWindowDuration = 0.5f;   // 合并窗口：这段时间内的同类型事件合并为一条

    private Coroutine currentHintCoroutine;
    private bool subscribedToPlayer;
    private bool isWaitingForPlayer;
    private readonly Queue<string> messageQueue = new Queue<string>();

    // 合并缓冲区
    private readonly List<string> pendingAddedNames = new List<string>();
    private readonly List<string> pendingRemovedNames = new List<string>();
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
        pendingAddedNames.Add(displayName);
        mergeAddedCoroutine = RestartMergeCoroutine(mergeAddedCoroutine, FlushAdded);
    }

    private void OnItemRemoved(string itemName, int amount)
    {
        if (string.IsNullOrWhiteSpace(itemName)) return;
        string displayName = GetDisplayName(itemName);
        Debug.Log($"[HintUI] 加入合并缓冲区 - 失去物品：{displayName}");
        pendingRemovedNames.Add(displayName);
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
        if (pendingAddedNames.Count == 0) return;
        string merged = $"获得物品：{string.Join("、", pendingAddedNames)}";
        Debug.Log($"[HintUI] 合并输出 - {merged}");
        ShowHint(merged);
        pendingAddedNames.Clear();
    }

    private void FlushRemoved()
    {
        if (pendingRemovedNames.Count == 0) return;
        string merged = $"失去物品：{string.Join("、", pendingRemovedNames)}";
        Debug.Log($"[HintUI] 合并输出 - {merged}");
        ShowHint(merged);
        pendingRemovedNames.Clear();
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
