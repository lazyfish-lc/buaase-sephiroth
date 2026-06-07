using TMPro;
using UnityEngine;

public class Chapter3ClueTarget : MonoBehaviour {
    [Header("Replacement")]
    [SerializeField] private GameObject revealPrefab;
    [SerializeField] private Transform revealSpawnPoint;
    [SerializeField] private bool hideOwnerWhenRevealed = true;

    [Header("Clock")]
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private string correctedClockText = "10:00";

    private bool isClockCorrected;
    private bool isIceWeaponRevealed;
    private bool isDiaryRevealed;

    public void RevealIceWeapon() {
        if (isIceWeaponRevealed) return;
        isIceWeaponRevealed = true;

        if (revealPrefab != null) {
            var spawn = revealSpawnPoint != null ? revealSpawnPoint : transform;
            Instantiate(revealPrefab, spawn.position, spawn.rotation, spawn.parent);
        }

        if (hideOwnerWhenRevealed) {
            gameObject.SetActive(false);
        }

        Chapter3Events.RaiseIceWeaponRevealed();
    }

    public void CorrectClock() {
        isClockCorrected = true;
        if (clockText != null) {
            clockText.text = correctedClockText;
        }
        Chapter3Events.RaiseClockCorrected();
    }

    public void RevealDiary() {
        if (isDiaryRevealed) return;
        isDiaryRevealed = true;

        if (hideOwnerWhenRevealed) {
            gameObject.SetActive(false);
        }

        Chapter3Events.RaiseDiaryRevealed();
    }

    public bool HasCorrectedClock => isClockCorrected;
    public bool HasRevealedIceWeapon => isIceWeaponRevealed;
    public bool HasRevealedDiary => isDiaryRevealed;
}
