using System;

public static class Chapter3Events {
    // 原有线索事件
    public static event Action IceWeaponRevealed;
    public static event Action ClockCorrected;
    public static event Action DiaryRevealed;

    // 多阶段调查事件
    public static event Action HallExplored;           // 大厅初步调查完成
    public static event Action WifeRoomExplored;       // 妻子房间调查完成
    public static event Action AssistantRoomEntered;   // 助手房间进入
    public static event Action TrueMotiveRevealed;     // 虚伪的标签被移除（真相揭露）
    public static event Action DiaryTalked;            // 玩家与助手房间的日记对话完毕

    public static void RaiseIceWeaponRevealed() {
        IceWeaponRevealed?.Invoke();
    }

    public static void RaiseClockCorrected() {
        ClockCorrected?.Invoke();
    }

    public static void RaiseDiaryRevealed() {
        DiaryRevealed?.Invoke();
    }

    public static void RaiseHallExplored() {
        HallExplored?.Invoke();
    }

    public static void RaiseWifeRoomExplored() {
        WifeRoomExplored?.Invoke();
    }

    public static void RaiseAssistantRoomEntered() {
        AssistantRoomEntered?.Invoke();
    }

    public static void RaiseTrueMotiveRevealed() {
        TrueMotiveRevealed?.Invoke();
    }

    public static void RaiseDiaryTalked() {
        DiaryTalked?.Invoke();
    }
}
