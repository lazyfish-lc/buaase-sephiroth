using System;

public static class Chapter3Events {
    public static event Action IceWeaponRevealed;
    public static event Action ClockCorrected;
    public static event Action DiaryRevealed;

    public static void RaiseIceWeaponRevealed() {
        IceWeaponRevealed?.Invoke();
    }

    public static void RaiseClockCorrected() {
        ClockCorrected?.Invoke();
    }

    public static void RaiseDiaryRevealed() {
        DiaryRevealed?.Invoke();
    }
}
