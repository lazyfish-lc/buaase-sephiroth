using System;
using UnityEngine;

public class Chapter3_SceneController : MonoBehaviour {
    public static Chapter3_SceneController Instance { get; private set; }

    public event Action<bool> AccusationPromptStateChanged;

    [Header("Progress Flags")]
    [SerializeField] private bool hasIceWeapon;
    [SerializeField] private bool hasClockCorrected;
    [SerializeField] private bool hasDiaryRevealed;
    [SerializeField] private bool accusationPromptShown;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable() {
        Chapter3Events.IceWeaponRevealed += HandleIceWeaponRevealed;
        Chapter3Events.ClockCorrected += HandleClockCorrected;
        Chapter3Events.DiaryRevealed += HandleDiaryRevealed;
    }

    private void OnDisable() {
        Chapter3Events.IceWeaponRevealed -= HandleIceWeaponRevealed;
        Chapter3Events.ClockCorrected -= HandleClockCorrected;
        Chapter3Events.DiaryRevealed -= HandleDiaryRevealed;
    }

    private void HandleIceWeaponRevealed() {
        hasIceWeapon = true;
        TryOfferAccuseOption();
    }

    private void HandleClockCorrected() {
        hasClockCorrected = true;
        TryOfferAccuseOption();
    }

    private void HandleDiaryRevealed() {
        hasDiaryRevealed = true;
        TryOfferAccuseOption();
    }

    private void TryOfferAccuseOption() {
        if (accusationPromptShown) return;
        if (!hasIceWeapon || !hasClockCorrected || !hasDiaryRevealed) return;

        accusationPromptShown = true;
        Debug.Log("Chapter3_SceneController: 所有关键证据已收集，可以弹出指认或继续调查提示。");
        AccusationPromptStateChanged?.Invoke(true);
    }

    public bool HasAllKeyEvidence() {
        return hasIceWeapon && hasClockCorrected && hasDiaryRevealed;
    }

    public void ResetProgress() {
        hasIceWeapon = false;
        hasClockCorrected = false;
        hasDiaryRevealed = false;
        accusationPromptShown = false;
        AccusationPromptStateChanged?.Invoke(false);
    }

    /// <summary>
    /// 构建存档数据
    /// </summary>
    public Chapter3ClueSaveData GetClueSaveData() {
        return new Chapter3ClueSaveData {
            hasIceWeapon = hasIceWeapon,
            hasClockCorrected = hasClockCorrected,
            hasDiaryRevealed = hasDiaryRevealed,
            accusationPromptShown = accusationPromptShown
        };
    }

    /// <summary>
    /// 从存档恢复线索状态，并重新评估是否可弹出指认提示
    /// </summary>
    public void ApplyClueSaveData(Chapter3ClueSaveData data) {
        if (data == null) return;

        hasIceWeapon = data.hasIceWeapon;
        hasClockCorrected = data.hasClockCorrected;
        hasDiaryRevealed = data.hasDiaryRevealed;
        accusationPromptShown = data.accusationPromptShown;

        // 如果已经触发过指认提示，重新通知 UI
        if (accusationPromptShown) {
            AccusationPromptStateChanged?.Invoke(true);
        }
    }
}
