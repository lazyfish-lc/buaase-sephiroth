using UnityEngine;

public class ActionUI : MonoBehaviour
{
    public CanvasGroup ActionCanvas;
    public static ActionUI Instance;
    void Awake()
    {
        Instance = this;
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
        
    
}
