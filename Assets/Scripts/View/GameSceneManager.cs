using UnityEngine;

public class GameSceneManager : MonoBehaviour {
    public static GameSceneManager Instance;
    public GameObject pastMapRoot;
    public GameObject presentMapRoot;
    public bool isPresent = true;

    void Awake() { Instance = this; }

    public void SetTimeVision(bool present) {
        isPresent = present;
        if (pastMapRoot != null) pastMapRoot.SetActive(!isPresent);
        if (presentMapRoot != null) presentMapRoot.SetActive(isPresent);
    }

    public void ToggleTimeVision() {
        isPresent = !isPresent;
        if (pastMapRoot != null) pastMapRoot.SetActive(!isPresent);
        if (presentMapRoot != null) presentMapRoot.SetActive(isPresent);
    }
}