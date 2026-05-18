using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NumericalRuleSceneLoader : MonoBehaviour {
    [SerializeField] private string resourcesPath = "NumericalRuleConfigs";
    [SerializeField] private List<SceneNumericalRuleConfig> configs = new List<SceneNumericalRuleConfig>();
    [SerializeField] private bool preferResources = true;
    [SerializeField] private int loadDelayFrames = 1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap() {
        var existing = FindFirstObjectByType<NumericalRuleSceneLoader>();
        if (existing != null) {
            return;
        }

        var go = new GameObject("NumericalRuleSceneLoader");
        go.AddComponent<NumericalRuleSceneLoader>();
        Debug.Log("NumericalRuleSceneLoader: bootstrap created loader instance.");
    }

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("NumericalRuleSceneLoader: Awake and listening to sceneLoaded.");
    }

    private void Start() {
        var activeScene = SceneManager.GetActiveScene();
        if (activeScene.IsValid()) {
            StartCoroutine(LoadRulesAfterDelay(activeScene.name));
        }
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        StartCoroutine(LoadRulesAfterDelay(scene.name));
    }

    private IEnumerator LoadRulesAfterDelay(string sceneName) {
        int frames = Mathf.Max(0, loadDelayFrames);
        for (int i = 0; i < frames; i++) {
            yield return null;
        }
        LoadRulesForScene(sceneName);
    }

    private void LoadRulesForScene(string sceneName) {
        var matched = GetConfigsForScene(sceneName);
        Debug.Log($"场景 {sceneName} 的数值规则配置匹配结果：共 {matched.Count} 条配置");
        if (matched.Count == 0) {
            Debug.Log($"场景 {sceneName} 没有匹配的数值规则配置");
            return;
        }

        NumericalRuleManager.ClearRules();
        foreach (var cfg in matched) {
            if (cfg == null || cfg.ruleStrings == null) {
                continue;
            }

            for (int i = 0; i < cfg.ruleStrings.Count; i++) {
                NumericalRuleManager.BuildAndRegisterRules(cfg.ruleStrings[i]);
            }
        }
        Debug.Log($"已加载场景 {sceneName} 的数值规则配置，共 {matched.Count} 条配置");
    }

    private List<SceneNumericalRuleConfig> GetConfigsForScene(string sceneName) {
        if (preferResources || configs.Count == 0) {
            configs = new List<SceneNumericalRuleConfig>(Resources.LoadAll<SceneNumericalRuleConfig>(resourcesPath));
            Debug.Log($"NumericalRuleSceneLoader: loaded {configs.Count} configs from Resources/{resourcesPath}.");
        }

        var result = new List<SceneNumericalRuleConfig>();
        for (int i = 0; i < configs.Count; i++) {
            var cfg = configs[i];
            if (cfg == null || string.IsNullOrWhiteSpace(cfg.sceneName)) {
                continue;
            }

            if (string.Equals(cfg.sceneName, sceneName, System.StringComparison.Ordinal)) {
                result.Add(cfg);
            }
        }

        return result;
    }
}
