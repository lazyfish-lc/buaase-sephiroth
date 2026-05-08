using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NumericalRuleSceneLoader : MonoBehaviour {
    [SerializeField] private string resourcesPath = "NumericalRuleConfigs";
    [SerializeField] private List<SceneNumericalRuleConfig> configs = new List<SceneNumericalRuleConfig>();
    [SerializeField] private bool preferResources = true;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap() {
        var existing = FindFirstObjectByType<NumericalRuleSceneLoader>();
        if (existing != null) {
            return;
        }

        var go = new GameObject("NumericalRuleSceneLoader");
        go.AddComponent<NumericalRuleSceneLoader>();
    }

    private void Awake() {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        StartCoroutine(LoadRulesAfterFrame(scene.name));
    }

    private IEnumerator LoadRulesAfterFrame(string sceneName) {
        yield return null;
        LoadRulesForScene(sceneName);
    }

    private void LoadRulesForScene(string sceneName) {
        var matched = GetConfigsForScene(sceneName);
        if (matched.Count == 0) {
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
    }

    private List<SceneNumericalRuleConfig> GetConfigsForScene(string sceneName) {
        if (preferResources || configs.Count == 0) {
            configs = new List<SceneNumericalRuleConfig>(Resources.LoadAll<SceneNumericalRuleConfig>(resourcesPath));
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
