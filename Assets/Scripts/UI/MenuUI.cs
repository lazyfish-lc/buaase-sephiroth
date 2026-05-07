using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUI : MonoBehaviour
{
    public bool isOpen = false;
    public CanvasGroup MenuCanvas;
    public static MenuUI Instance;
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
        isOpen = true;
        MenuCanvas.gameObject.SetActive(true);
    }
    public void Close()
    {
        isOpen = false;
        MenuCanvas.gameObject.SetActive(false);
    }
    public void OpenSetting()
    {
        SettingUI.Instance.OpenAndClose();
    }
    public void OpenStartMenu()
    {
        if (isLoadingStartMenu)
        {
            return;
        }

        isLoadingStartMenu = true;
        Time.timeScale = 1f;

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

        Close();
        SceneManager.LoadScene("StartMenuScene");
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void RestartGame()
    {
        //重启当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveGame()
    {
        if (SaveManager.Instance == null) {
            Debug.LogWarning("SaveManager 未配置，无法保存游戏");
            return;
        }

        SaveManager.Instance.SaveGame();
        Debug.Log("游戏已保存");
    }

    public void LoadGame()
    {
        if (SaveManager.Instance == null) {
            Debug.LogWarning("SaveManager 未配置，无法加载存档");
            return;
        }

        SaveManager.Instance.LoadGame();
        Debug.Log("游戏存档已加载");
        
    }
    
}
