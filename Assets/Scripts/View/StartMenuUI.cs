using UnityEngine;

public class StartMenuUI : MonoBehaviour
{
    public CanvasGroup StartMenuCanvas;
        void Start()
    {
        
    }

       void Update()
    {
        if(Input.GetButtonDown("StartMenu"))
        {
            if(StartMenuCanvas.alpha == 0)
            {
                StartMenuCanvas.alpha = 1;
                StartMenuCanvas.interactable = true;
                StartMenuCanvas.blocksRaycasts = true;
            }
            else
            {
                StartMenuCanvas.alpha = 0;
                StartMenuCanvas.interactable = false;
                StartMenuCanvas.blocksRaycasts = false;
            }
        }
    }
}
