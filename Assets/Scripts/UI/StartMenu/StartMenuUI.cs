using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : FatherUI
{
    public CanvasGroup StartMenuCanvas;
    public void GameStart()
    {
        PlayClickSFX();

        // ─── 新游戏：清除所有持久化状态 ──────────────────
        // 1. 清除教程触发记录
        DialogueNodeActionSOActions.ApplyTriggeredTutorialIds(null);
        DialogueNodeActionSOActions.ApplyVisibleTutorialIndices(null);

        // 2. 清除第三章线索进度（静态永久数据）
        Chapter3_SceneController.ResetStaticProgress();

        // 3. 清除玩家跨场景状态缓存
        PlayerSceneStateCache.Clear();

        // 4. 清除 SaveManager 的自动保存场景状态，防止恢复旧进度
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ClearAllSceneStates();
        }

        // 加载游戏场景
        FindFirstObjectByType<LoadingUI>().LoadScene("Map1");
    }
    public void GameLoad()
    {
        PlayClickSFX();
        // 继续游戏逻辑（例如加载存档）
        Debug.Log("继续游戏被调用");
        if (SaveManager.Instance == null) {
            Debug.LogWarning("SaveManager 未配置，无法加载存档");
            return;
        }

        if (!SaveManager.Instance.HasSaveFile())
        {
            Debug.LogWarning("没有可用的存档");
            return;
        }

        HintUI.PendingHintMessage = "存档加载成功";
        SaveManager.Instance.LoadGame();
    }
    public void GameSettings()
    {
        // 打开主设置界面
        Debug.Log("主设置被调用");
        PlayClickSFX();
        //FindFirstObjectByType<LoadingUI>().LoadScene("SettingsScene");

    }
    public void GameExit()
    {
        // 退出游戏
        Debug.Log("退出游戏被调用");
        PlayClickSFX();
        Application.Quit();
    }
}
