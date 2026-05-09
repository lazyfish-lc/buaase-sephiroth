using UnityEngine;
using UnityEngine.UI;
public class KeysetUI : FatherUI
{   
    public static KeysetUI Instance;
    public CanvasGroup KeysetCanvas;
    public static bool isOpen = false;
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        // 初始状态下显示按键指导
        // 判断是否是特定场景或条件下显示按键指导，例如在主菜单或特定关卡
        Debug.Log($"当前场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Map1")
        {
            KeysetCanvas.gameObject.SetActive(true);
        }
        else
        {
            KeysetCanvas.gameObject.SetActive(false);
        }
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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
        KeysetCanvas.gameObject.SetActive(true);
        isOpen = true;
    }
    public void Close()
    {
        PlayCloseSFX();
        KeysetCanvas.gameObject.SetActive(false);
        isOpen = false;
    }
    
}
