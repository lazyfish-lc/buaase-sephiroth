using UnityEngine;

public class BackpackUI : MonoBehaviour
{
    public CanvasGroup BackpackCanvas;
        void Start()
    {
        
    }

       void Update()
    {
        if(Input.GetButtonDown("Backpack"))
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
}
