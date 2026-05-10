using UnityEngine;

public class GlowLabelRegistry : MonoBehaviour {
    public static GlowLabelRegistry Instance;

    [Header("GlowLabel prefab")]
    public GameObject spotLightPrefab;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
