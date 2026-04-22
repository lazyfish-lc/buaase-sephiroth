using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    public CanvasGroup StartMenuCanvas;
    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void GameStart()
    {
        // 加载游戏场景
        SceneManager.LoadScene("GameScene");
    }

    public void GameLoad()
    {
        // 继续游戏逻辑（例如加载存档）
        Debug.Log("继续游戏被调用");
        //SceneManager.LoadScene("LoadScene");
    }

    public void GameSettings()
    {
        // 打开设置界面
        Debug.Log("设置被调用");
        //SceneManager.LoadScene("SettingsScene");

    }

    public void GameExit()
    {
        // 退出游戏
        Debug.Log("退出游戏被调用");
        Application.Quit();
    }
}
