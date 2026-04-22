using UnityEngine;

public class ActionUI : MonoBehaviour
{
    public CanvasGroup ActionCanvas;
        void Start()
    {
        
    }

       void Update()
    {
        if(Input.GetButtonDown("Action"))
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
}
