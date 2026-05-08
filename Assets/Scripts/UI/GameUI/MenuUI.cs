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
        //重启当前场景
        FindFirstObjectByType<LoadingUI>().LoadScene(SceneManager.GetActiveScene().name);
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
    }
    public void LoadGame()
    {
        PlayClickSFX();
        if (SaveManager.Instance == null) {
            Debug.LogWarning("SaveManager 未配置，无法加载存档");
            return;
        }

        SaveManager.Instance.LoadGame();
        Debug.Log("游戏存档已加载");
    }
    
}
