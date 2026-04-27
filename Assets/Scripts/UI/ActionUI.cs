using UnityEngine;

public class ActionUI : MonoBehaviour
{
    public CanvasGroup ActionCanvas;
    public static ActionUI Instance;

    //public NPCObject currentNPC;
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
        ActionCanvas.alpha = 0;
        ActionCanvas.interactable = false;
        ActionCanvas.blocksRaycasts = false;
    }
    public void OpenAndClose()
    {
        
        if(ActionCanvas.alpha == 0)
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
        ActionCanvas.alpha = 1;
        ActionCanvas.interactable = true;
        ActionCanvas.blocksRaycasts = true;
    }
    public void Close()
    {
        ActionCanvas.alpha = 0;
        ActionCanvas.interactable = false;
        ActionCanvas.blocksRaycasts = false;
    }
        
    /*public void SetCurrentNPC(NPCObject npc)
    {
        currentNPC = npc;
    }
    */
    public void Button1Click()
    {
        Debug.Log("Button 1 Clicked");
        // 在这里添加按钮1的功能逻辑，例如与当前NPC交互
        // if (currentNPC != null) {
        //     currentNPC.Interact();
        // }
    }
    public void Button2Click()
    {
        Debug.Log("Button 2 Clicked");
        // 在这里添加按钮2的功能逻辑，例如打开一个新的UI界面
    }
    public void Button3Click()
    {
        Debug.Log("Button 3 Clicked");
        // 在这里添加按钮3的功能逻辑，例如执行一个特殊技能
    }
    public void Button4Click()
    {
        Debug.Log("Button 4 Clicked");
        // 在这里添加按钮4的功能逻辑，例如显示当前任务信息
    }
    
    
}
