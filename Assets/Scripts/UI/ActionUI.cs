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
            ActionCanvas.alpha = 1;
            ActionCanvas.interactable = true;
            ActionCanvas.blocksRaycasts = true;
        }
        else
        {
            ActionCanvas.alpha = 0;
            ActionCanvas.interactable = false;
            ActionCanvas.blocksRaycasts = false;
        }
    }
        
    
}
