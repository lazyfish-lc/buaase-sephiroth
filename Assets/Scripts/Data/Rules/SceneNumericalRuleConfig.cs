using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SceneNumericalRuleConfig", menuName = "Game/SceneNumericalRuleConfig")]
public class SceneNumericalRuleConfig : ScriptableObject {
    public string sceneName;
    public List<string> ruleStrings = new List<string>();
}
