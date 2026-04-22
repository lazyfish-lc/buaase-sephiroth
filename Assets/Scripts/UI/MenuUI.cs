using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public CanvasGroup MenuCanvas;
    void Start()
    {
        MenuCanvas.alpha = 0;
        MenuCanvas.interactable = false;
        MenuCanvas.blocksRaycasts = false;
    }

    void Update()
    {
        if(Input.GetButtonDown("Menu"))
        {
            if(MenuCanvas.alpha == 0)
            {
                MenuCanvas.alpha = 1;
                MenuCanvas.interactable = true;
                MenuCanvas.blocksRaycasts = true;
            }
            else
            {
                MenuCanvas.alpha = 0;
                MenuCanvas.interactable = false;
                MenuCanvas.blocksRaycasts = false;
            }
        }
    }
    public void Close()
    {
        MenuCanvas.alpha = 0;
        MenuCanvas.interactable = false;
        MenuCanvas.blocksRaycasts = false;
    }
}
