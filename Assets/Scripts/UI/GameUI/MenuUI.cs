using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUI : FatherUI
{
    public static MenuUI Instance;
    public bool isOpen = false;
    public CanvasGroup MenuCanvas;
    private bool isLoadingStartMenu;
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void Start()
    {
        MenuCanvas.gameObject.SetActive(false);
    }
    public void OpenAndClose()
    {
        if(!isOpen)
        {
            Open();
        }
        else
        {
            Close();
        }
        
    }
    public void Open()
    {
        PlayOpenSFX();
        isOpen = true;
        MenuCanvas.gameObject.SetActive(true);
    }
    public void Close()
    {
        PlayCloseSFX();
        isOpen = false;
        MenuCanvas.gameObject.SetActive(false);
    }
    public void OpenSetting()
    {
        SettingUI.Instance.OpenAndClose();
        PlayClickSFX();
    }
    public void OpenStartMenu()
    {
        if (isLoadingStartMenu)
        {
            return;
        }
        isLoadingStartMenu = true;
        PlayClickSFX();
        Time.timeScale = 1f; // 确保时间流逝正常
        if (SettingUI.Instance != null)
        {
            SettingUI.Instance.Close();
        }
        if (BackpackUI.Instance != null)
        {
            BackpackUI.Instance.Close();
        }
        if (ActionUI.Instance != null)
        {
            ActionUI.Instance.Close();
        }
        if (StatusUI.Instance != null)
        {
            StatusUI.Instance.Close();
        }
        if (PropertyUI.Instance != null)
        {
            PropertyUI.Instance.Close();
        }
        if (LabelUI.Instance != null)
        {
            LabelUI.Instance.Close();
        }
        if (KeysetUI.Instance != null)
        {
            KeysetUI.Instance.Close();
        }
        
        Close();
        FindFirstObjectByType<LoadingUI>().LoadScene("StartMenuScene");
    }
    public void Quit()
    {
        Application.Quit();
        PlayClickSFX();
    }
    public void RestartGame()
    {
        PlayClickSFX();
        HintUI.PendingHintMessage = "已重新开始";
        //重启当前场景
        FindFirstObjectByType<LoadingUI>().LoadScene(SceneManager.GetActiveScene().name);
    }
    public void OpenGuide()
    {
        GuideUI.Instance.OpenAndClose();
        PlayClickSFX();
    }
    public void SaveGame()
    {
        PlayClickSFX();
        if (SaveManager.Instance == null) {
            Debug.LogWarning("SaveManager 未配置，无法保存游戏");
            return;
        }

        SaveManager.Instance.SaveGame();
        Debug.Log("游戏已保存");
        HintUI.Instance?.ShowHint("存档成功");
    }
    public void LoadGame()
    {
        PlayClickSFX();
        if (SaveManager.Instance == null) {
            Debug.LogWarning("SaveManager 未配置，无法加载存档");
            return;
        }

        if (!SaveManager.Instance.HasSaveFile())
        {
            Debug.LogWarning("没有可用的存档");
            return;
        }

        // 设置跨场景提示，场景重载后 HintUI 会自动显示
        HintUI.PendingHintMessage = "存档加载成功";
        SaveManager.Instance.LoadGame();
        Debug.Log("游戏存档已加载");

        // 同一场景直接加载（无 loading 画面），立即显示提示
        if (!LoadingUI.isLoading)
        {
            HintUI.Instance?.ShowHint("存档加载成功");
            HintUI.PendingHintMessage = null;
        }
    }
    
}
