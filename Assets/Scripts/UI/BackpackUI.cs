using UnityEngine;

public class BackpackUI : MonoBehaviour
{
    public CanvasGroup BackpackCanvas;
    public static BackpackUI Instance;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        BackpackCanvas.alpha = 0;
        BackpackCanvas.interactable = false;
        BackpackCanvas.blocksRaycasts = false;
    }
    public void OpenAndClose()
    {
        
        if(BackpackCanvas.alpha == 0)
        {
            BackpackCanvas.alpha = 1;
            BackpackCanvas.interactable = true;
            BackpackCanvas.blocksRaycasts = true;
        }
        else
        {
            BackpackCanvas.alpha = 0;
            BackpackCanvas.interactable = false;
            BackpackCanvas.blocksRaycasts = false;
        }
        
    }
}
