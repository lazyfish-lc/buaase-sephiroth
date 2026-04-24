using UnityEngine;
using UnityEngine.SceneManagement;
public class MenuUI : MonoBehaviour
{
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
        MenuCanvas.alpha = 0;
        MenuCanvas.interactable = false;
        MenuCanvas.blocksRaycasts = false;
    }
    public void OpenAndClose()
    {
        
        if(MenuCanvas.alpha == 0)
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
        MenuCanvas.alpha = 1;
        MenuCanvas.interactable = true;
        MenuCanvas.blocksRaycasts = true;
    }
    public void Close()
    {
        MenuCanvas.alpha = 0;
        MenuCanvas.interactable = false;
        MenuCanvas.blocksRaycasts = false;
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
        //调用SaveManager的保存方法
        Debug.Log("游戏已保存");
        
    }

    public void LoadGame()
    {
        //调用SaveManager的加载方法
        Debug.Log("游戏存档已加载");
        
        
    }
    
}
