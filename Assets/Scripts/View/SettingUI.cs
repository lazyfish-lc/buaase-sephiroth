using UnityEngine;

public class SettingUI : MonoBehaviour
{
    public CanvasGroup SettingCanvas;
        void Start()
    {
        
    }

       void Update()
    {
        if(Input.GetButtonDown("Setting"))
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
}
