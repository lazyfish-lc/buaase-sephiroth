using UnityEngine;

public class GameSceneManager : MonoBehaviour {
    public static GameSceneManager Instance;
    public GameObject pastMapRoot;
    public GameObject presentMapRoot;
    public bool isPresent = true;

    void Awake() { Instance = this; }

    public void ToggleTimeVision() {
        isPresent = !isPresent;
        pastMapRoot.SetActive(!isPresent);
        presentMapRoot.SetActive(isPresent);
    }
}