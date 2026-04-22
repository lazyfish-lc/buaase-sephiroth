using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public CanvasGroup MenuCanvas;
    public static MenuUI Instance;
    void Awake()
    {
        Instance = this;
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
}
