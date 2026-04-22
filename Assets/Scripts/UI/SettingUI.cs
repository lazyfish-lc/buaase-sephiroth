using UnityEngine;

public class SettingUI : MonoBehaviour
{
    public CanvasGroup SettingCanvas;
    public static SettingUI Instance;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        SettingCanvas.alpha = 0;
        SettingCanvas.interactable = false;
        SettingCanvas.blocksRaycasts = false;
    }
    public void OpenAndClose()
    {
        
        if(SettingCanvas.alpha == 0)
        {
            SettingCanvas.alpha = 1;
            SettingCanvas.interactable = true;
            SettingCanvas.blocksRaycasts = true;
        }
        else
        {
            SettingCanvas.alpha = 0;
            SettingCanvas.interactable = false;
            SettingCanvas.blocksRaycasts = false;
        }
    }
    
}
