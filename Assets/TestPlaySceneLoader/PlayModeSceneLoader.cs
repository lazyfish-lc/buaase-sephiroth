using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayModeSceneLoader
{
    static PlayModeSceneLoader()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // 只在即将进入Play Mode时执行
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // 获取构建设置中的第一个场景路径
            string firstScenePath = EditorBuildSettings.scenes[0].path;

            // 强制设置当前打开的场景为第一个场景。
            // 这会在进入Play Mode前生效。
            EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(firstScenePath);
            // 注意：你无需在此代码中调用 LoadScene，Unity会自动完成
        }
    }
}