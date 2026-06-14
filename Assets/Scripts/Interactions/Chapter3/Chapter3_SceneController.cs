using System;
using UnityEngine;

public class Chapter3_SceneController : MonoBehaviour {
    public static Chapter3_SceneController Instance { get; private set; }

    // ─── 事件 ─────────────────────────────────────────────
    /// <summary>指认选择可用时触发，参数 = 阶段索引 (0-4)</summary>
    public event Action<int> EndingChoiceAvailable;
    /// <summary>结局被触发时，参数 = 结局索引 (0=A ~ 4=E)</summary>
    public event Action<int> EndingTriggered;

    // ─── 阶段/结局常量 ─────────────────────────────────────
    public const int STAGE_HALL      = 0; // 大厅 — 助手指认 → Ending A
    public const int STAGE_WIFE      = 1; // 妻子房间 → Ending B
    public const int STAGE_WRITER    = 2; // 作家房间 — 冰武器+时钟 → Ending C
    public const int STAGE_ASSISTANT = 3; // 助手 — 与带虚伪标签的日记对话 → Ending D
    public const int STAGE_TRUTH     = 4; // 真相 — 与无虚伪标签的日记对话 → Ending E
    public const int STAGE_COUNT     = 5;

    public const int ENDING_A = 0; // 大厅指认 — 最差结局
    public const int ENDING_B = 1; // 妻子指认
    public const int ENDING_C = 2; // 作家指认（缺动机）
    public const int ENDING_D = 3; // 助手指认（不完整）
    public const int ENDING_E = 4; // 真相指认（完整结局）

    // ─── 跨场景持久化 ──────────────────────────────────────
    // 场景切换时 MonoBehaviour 被销毁，用静态字段保活进度
    private static Chapter3ClueSaveData persistentData = new Chapter3ClueSaveData();

    // ─── Inspector 调试查看 ────────────────────────────────
    [Header("Progress Flags")]
    [SerializeField] private bool hasIceWeapon;
    [SerializeField] private bool hasClockCorrected;
    [SerializeField] private int currentEndingStage;
    [SerializeField] private bool endingTriggered;
    [SerializeField] private int triggeredEndingIndex = -1;
    [SerializeField] private bool fakeLabelRemoved;
    [SerializeField] private bool assistantRoomEntered;

    // ─── 物品名称常量 ──────────────────────────────────────
    public const string ITEM_FROSTBOLT          = "Frostbolt";
    public const string ITEM_FAST_FORWARD_CLOCK = "FastForwardClock";
    public const string ITEM_ASSISTANT_DIARY    = "AssistantDiary";
    public const string ITEM_WIFES_TESTIMONY    = "WifesTestimony";
    public const string ITEM_ASSISTANT_MANUSCRIPT = "AssistantManuscript";
    public const string ITEM_ASSISTANT_PLAN     = "AssistantPlan";

    // ─── 属性访问 ──────────────────────────────────────────
    public int CurrentEndingStage => currentEndingStage;
    public bool HasEndingTriggered => endingTriggered;
    public int TriggeredEndingIndex => triggeredEndingIndex;
    public bool HasIceWeapon => hasIceWeapon;
    public bool HasClockCorrected => hasClockCorrected;
    public bool FakeLabelRemoved => fakeLabelRemoved;

    // ========================================================
    // 生命周期
    // ========================================================
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ApplyPersistentData();
    }

    private void OnEnable() {
        Chapter3Events.IceWeaponRevealed   += HandleIceWeaponRevealed;
        Chapter3Events.ClockCorrected      += HandleClockCorrected;
        Chapter3Events.DiaryRevealed       += HandleDiaryRevealed;
        Chapter3Events.HallExplored        += HandleHallExplored;
        Chapter3Events.WifeRoomExplored    += HandleWifeRoomExplored;
        Chapter3Events.AssistantRoomEntered += HandleAssistantRoomEntered;
        Chapter3Events.TrueMotiveRevealed  += HandleTrueMotiveRevealed;
        Chapter3Events.DiaryTalked         += HandleDiaryTalked;
    }

    private void OnDisable() {
        Chapter3Events.IceWeaponRevealed   -= HandleIceWeaponRevealed;
        Chapter3Events.ClockCorrected      -= HandleClockCorrected;
        Chapter3Events.DiaryRevealed       -= HandleDiaryRevealed;
        Chapter3Events.HallExplored        -= HandleHallExplored;
        Chapter3Events.WifeRoomExplored    -= HandleWifeRoomExplored;
        Chapter3Events.AssistantRoomEntered -= HandleAssistantRoomEntered;
        Chapter3Events.TrueMotiveRevealed  -= HandleTrueMotiveRevealed;
        Chapter3Events.DiaryTalked         -= HandleDiaryTalked;

        SyncToPersistent();
    }

    private void OnDestroy() {
        if (Instance == this) {
            SyncToPersistent();
        }
    }

    // ========================================================
    // 事件处理 — 线索发现
    // ========================================================

    /// <summary>
    /// 公开静态方法 — MeltLabel / Chapter3ClueTarget 可直接调用，不依赖事件订阅
    /// </summary>
    public static void NotifyIceWeaponRevealed() {
        if (Instance != null) {
            Instance.HandleIceWeaponRevealed();
        }
    }

    /// <summary>
    /// 公开静态方法 — FastForwardLabel / Chapter3ClueTarget 可直接调用，不依赖事件订阅
    /// </summary>
    public static void NotifyClockCorrected() {
        if (Instance != null) {
            Instance.HandleClockCorrected();
        }
    }

    /// <summary>
    /// 公开静态方法 — VainLabel / FakeLabel 可直接调用，不依赖事件订阅
    /// </summary>
    public static void NotifyTrueMotiveRevealed() {
        if (Instance != null) {
            Instance.HandleTrueMotiveRevealed();
        }
    }

    private void HandleIceWeaponRevealed() {
        if (hasIceWeapon) return;
        hasIceWeapon = true;
        TryAddClueItemToPlayer(ITEM_FROSTBOLT);
        TryAdvanceToStage(STAGE_WRITER);
    }

    private void HandleClockCorrected() {
        if (hasClockCorrected) return;
        hasClockCorrected = true;
        TryAddClueItemToPlayer(ITEM_FAST_FORWARD_CLOCK);
        TryAdvanceToStage(STAGE_WRITER);
    }

    private void HandleDiaryRevealed() {
        // 此事件由妻子/女仆等NPC的标签剥离触发，仅添加物品，不推进阶段
        TryAddClueItemToPlayer(ITEM_ASSISTANT_DIARY);
    }

    private void HandleHallExplored() {
        TryAdvanceToStage(STAGE_HALL);
    }

    private void HandleWifeRoomExplored() {
        TryAddClueItemToPlayer(ITEM_WIFES_TESTIMONY);
        TryAdvanceToStage(STAGE_WIFE);
    }

    private void HandleAssistantRoomEntered() {
        assistantRoomEntered = true;
        SyncToPersistent();
        // 仅标记房间已进入，阶段推进由 HandleDiaryTalked 驱动
    }

    private void HandleTrueMotiveRevealed() {
        fakeLabelRemoved = true;
        SyncToPersistent();
        // 仅标记虚伪标签已移除，阶段推进由 HandleDiaryTalked 驱动
    }

    /// <summary>
    /// 玩家与助手房间的日记对话完毕 → 根据虚伪标签是否已移除，推进到对应阶段
    /// </summary>
    private void HandleDiaryTalked() {
        if (!assistantRoomEntered) {
            Debug.Log("Chapter3_SceneController: 收到 DiaryTalked 但未进入助手房间，忽略。");
            return;
        }
        if (fakeLabelRemoved) {
            TryAdvanceToStage(STAGE_TRUTH);
        } else {
            TryAdvanceToStage(STAGE_ASSISTANT);
        }
    }

    // ========================================================
    // 阶段推进逻辑
    // ========================================================
    /// <summary>
    /// 推进调查阶段（仅推进计数器，不弹出指认选择）。
    /// 指认选择由对话节点的 exit action → Chapter3EndingTrigger → TryOfferEndingChoiceExternal 负责弹出。
    /// </summary>
    private void TryAdvanceToStage(int stage) {
        if (endingTriggered) return;
        if (currentEndingStage >= stage + 1) return;
        if (!ArePrerequisitesMetForStage(stage)) return;

        currentEndingStage = stage + 1;
        SyncToPersistent();
        Debug.Log($"Chapter3_SceneController: 调查进度推进到阶段 {stage} ({GetStageName(stage)})");
    }

    private bool ArePrerequisitesMetForStage(int stage) {
        var player = FindFirstObjectByType<PlayerSmallObject>();
        switch (stage) {
            case STAGE_HALL:
                return true;
            case STAGE_WIFE:
                return true;
            case STAGE_WRITER:
                return PlayerHasItem(player, ITEM_FROSTBOLT)
                    || PlayerHasItem(player, ITEM_FAST_FORWARD_CLOCK);
            case STAGE_ASSISTANT:
                return PlayerHasItem(player, ITEM_FROSTBOLT)
                    || PlayerHasItem(player, ITEM_FAST_FORWARD_CLOCK);
            case STAGE_TRUTH:
                return (PlayerHasItem(player, ITEM_FROSTBOLT)
                     || PlayerHasItem(player, ITEM_FAST_FORWARD_CLOCK))
                    && fakeLabelRemoved;
            default:
                return false;
        }
    }

    private static bool PlayerHasItem(PlayerSmallObject player, string itemName) {
        return player != null && player.HasItemInBackpack(itemName);
    }

    public bool IsStagePrompted(int stage) {
        return stage switch {
            0 => persistentData.stage0Prompted,
            1 => persistentData.stage1Prompted,
            2 => persistentData.stage2Prompted,
            3 => persistentData.stage3Prompted,
            4 => persistentData.stage4Prompted,
            _ => false
        };
    }

    private void MarkStagePrompted(int stage) {
        switch (stage) {
            case 0: persistentData.stage0Prompted = true; break;
            case 1: persistentData.stage1Prompted = true; break;
            case 2: persistentData.stage2Prompted = true; break;
            case 3: persistentData.stage3Prompted = true; break;
            case 4: persistentData.stage4Prompted = true; break;
        }
        SyncToPersistent();
    }

    // ========================================================
    // 玩家选择处理
    // ========================================================
    public void AccuseAtStage(int stage) {
        if (endingTriggered) return;

        int endingIndex = GetEndingIndexForStage(stage);
        endingTriggered = true;
        triggeredEndingIndex = endingIndex;
        persistentData.endingTriggered = true;
        persistentData.triggeredEndingIndex = endingIndex;
        SyncToPersistent();

        Debug.Log($"Chapter3_SceneController: 玩家指认！阶段={stage}, 结局={GetEndingName(endingIndex)}");
        EndingTriggered?.Invoke(endingIndex);
    }

    public void ContinueInvestigation(int stage) {
        // 最后一个结局（真相）不允许继续调查，重定向到指认
        if (stage == STAGE_TRUTH) {
            Debug.Log("Chapter3_SceneController: 真相阶段不允许继续调查，强制指认。");
            AccuseAtStage(stage);
            return;
        }
        Debug.Log($"Chapter3_SceneController: 玩家选择继续调查（阶段 {stage}）");
    }

    public static int GetEndingIndexForStage(int stage) {
        return stage switch {
            STAGE_HALL      => ENDING_A,
            STAGE_WIFE      => ENDING_B,
            STAGE_WRITER    => ENDING_C,
            STAGE_ASSISTANT => ENDING_D,
            STAGE_TRUTH     => ENDING_E,
            _               => ENDING_A
        };
    }

    // ─── 公开接口（供 Chapter3EndingTrigger 等外部调用） ────

    /// <summary>
    /// 为指定阶段弹出指认选择 — 这是弹指认选择的唯一途径。
    /// 由对话节点的 exit action → Chapter3EndingTrigger 调用。
    /// 不检查 IsStagePrompted，由对话 writer 确保 trigger 放在正确的叙事时机。
    /// </summary>
    public void TryOfferEndingChoiceExternal(int stage) {
        if (endingTriggered) return;
        if (stage < 0 || stage >= STAGE_COUNT) return;
        if (!ArePrerequisitesMetForStage(stage)) {
            Debug.LogWarning($"Chapter3_SceneController: 阶段 {stage} 前置条件不满足，无法弹出指认选择。");
            return;
        }

        if (IsStagePrompted(stage)) {
            Debug.Log($"Chapter3_SceneController: 阶段 {stage} 的指认选择已弹出过（防止重复触发）。");
            return;
        }

        MarkStagePrompted(stage);
        Debug.Log($"Chapter3_SceneController: 弹出阶段 {stage} ({GetStageName(stage)}) 的指认选择");
        EndingChoiceAvailable?.Invoke(stage);
    }

    // ========================================================
    // 工具方法
    // ========================================================
    private string GetStageName(int stage) {
        return stage switch {
            0 => "大厅",
            1 => "妻子房间",
            2 => "作家（冰武器+时钟）",
            3 => "助手（带虚伪标签的日记）",
            4 => "真相（无虚伪标签的日记）",
            _ => $"未知({stage})"
        };
    }

    private string GetEndingName(int ending) {
        return ending switch {
            0 => "结局A",
            1 => "结局B",
            2 => "结局C",
            3 => "结局D",
            4 => "结局E",
            _ => $"未知({ending})"
        };
    }

    private void TryAddClueItemToPlayer(string itemName) {
        if (string.IsNullOrWhiteSpace(itemName)) return;

        var player = FindFirstObjectByType<PlayerSmallObject>();
        if (player == null) {
            Debug.LogWarning("Chapter3_SceneController: 无法找到 PlayerSmallObject，无法添加线索物品。");
            return;
        }

        if (player.HasItemInBackpack(itemName)) {
            Debug.Log($"Chapter3_SceneController: 玩家背包中已有 '{itemName}'，跳过添加。");
            return;
        }

        player.AddItemToBackpack(Item.Create(itemName));
        Debug.Log($"Chapter3_SceneController: 已向玩家背包添加线索物品 '{itemName}'。");
    }

    public bool HasAllKeyEvidence() {
        return hasIceWeapon && hasClockCorrected;
    }

    public void ResetProgress() {
        hasIceWeapon = false;
        hasClockCorrected = false;
        currentEndingStage = 0;
        endingTriggered = false;
        triggeredEndingIndex = -1;
        fakeLabelRemoved = false;
        assistantRoomEntered = false;

        persistentData = new Chapter3ClueSaveData();
        Debug.Log("Chapter3_SceneController: 所有进度已重置。");
    }

    // ========================================================
    // 跨场景持久化
    // ========================================================
    private void ApplyPersistentData() {
        var d = persistentData;
        hasIceWeapon = d.hasIceWeapon;
        hasClockCorrected = d.hasClockCorrected;
        currentEndingStage = d.endingStage;
        endingTriggered = d.endingTriggered;
        triggeredEndingIndex = d.triggeredEndingIndex;
        fakeLabelRemoved = d.fakeLabelRemoved;
        assistantRoomEntered = d.assistantRoomEntered;

        Debug.Log($"Chapter3_SceneController: 从持久数据恢复，当前阶段={currentEndingStage}");
    }

    private void SyncToPersistent() {
        persistentData.hasIceWeapon = hasIceWeapon;
        persistentData.hasClockCorrected = hasClockCorrected;
        persistentData.endingStage = currentEndingStage;
        persistentData.endingTriggered = endingTriggered;
        persistentData.triggeredEndingIndex = triggeredEndingIndex;
        persistentData.fakeLabelRemoved = fakeLabelRemoved;
        persistentData.assistantRoomEntered = assistantRoomEntered;
    }

    // ========================================================
    // 存档接口
    // ========================================================
    public Chapter3ClueSaveData GetClueSaveData() {
        SyncToPersistent();
        return new Chapter3ClueSaveData {
            hasIceWeapon        = hasIceWeapon,
            hasClockCorrected   = hasClockCorrected,
            accusationPromptShown = persistentData.stage0Prompted
                                 || persistentData.stage1Prompted
                                 || persistentData.stage2Prompted
                                 || persistentData.stage3Prompted
                                 || persistentData.stage4Prompted,
            endingStage         = currentEndingStage,
            stage0Prompted      = persistentData.stage0Prompted,
            stage1Prompted      = persistentData.stage1Prompted,
            stage2Prompted      = persistentData.stage2Prompted,
            stage3Prompted      = persistentData.stage3Prompted,
            stage4Prompted      = persistentData.stage4Prompted,
            endingTriggered     = endingTriggered,
            triggeredEndingIndex = triggeredEndingIndex,
            fakeLabelRemoved    = fakeLabelRemoved,
            assistantRoomEntered = assistantRoomEntered
        };
    }

    public void ApplyClueSaveData(Chapter3ClueSaveData data) {
        if (data == null) return;

        hasIceWeapon        = data.hasIceWeapon;
        hasClockCorrected   = data.hasClockCorrected;
        currentEndingStage  = data.endingStage;
        endingTriggered     = data.endingTriggered;
        triggeredEndingIndex = data.triggeredEndingIndex;
        fakeLabelRemoved    = data.fakeLabelRemoved;
        assistantRoomEntered = data.assistantRoomEntered;

        persistentData.hasIceWeapon        = data.hasIceWeapon;
        persistentData.hasClockCorrected   = data.hasClockCorrected;
        persistentData.endingStage         = data.endingStage;
        persistentData.stage0Prompted      = data.stage0Prompted;
        persistentData.stage1Prompted      = data.stage1Prompted;
        persistentData.stage2Prompted      = data.stage2Prompted;
        persistentData.stage3Prompted      = data.stage3Prompted;
        persistentData.stage4Prompted      = data.stage4Prompted;
        persistentData.endingTriggered     = data.endingTriggered;
        persistentData.triggeredEndingIndex = data.triggeredEndingIndex;
        persistentData.fakeLabelRemoved    = data.fakeLabelRemoved;
        persistentData.assistantRoomEntered = data.assistantRoomEntered;

        Debug.Log($"Chapter3_SceneController: 从存档恢复，阶段={currentEndingStage}, 结局已触发={endingTriggered}, fakeLabelRemoved={fakeLabelRemoved}");
    }
}
