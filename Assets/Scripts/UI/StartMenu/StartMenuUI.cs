using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : FatherUI
{
    public CanvasGroup StartMenuCanvas;
    public void GameStart()
    {
        // 加载游戏场景
        PlayClickSFX();
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
