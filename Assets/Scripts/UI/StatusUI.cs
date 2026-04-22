using UnityEngine;

public class StatusUI : MonoBehaviour
{
    public CanvasGroup StatusCanvas;
    public static StatusUI Instance;
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    void Start()
    {
        StatusCanvas.alpha = 0;
        StatusCanvas.interactable = false;
        StatusCanvas.blocksRaycasts = false;
    }
    public void OpenAndClose()
    {
        
        if(StatusCanvas.alpha == 0)
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
        StatusCanvas.alpha = 1;
        StatusCanvas.interactable = true;
        StatusCanvas.blocksRaycasts = true;
    }
    public void Close()
    {
        StatusCanvas.alpha = 0;
        StatusCanvas.interactable = false;
        StatusCanvas.blocksRaycasts = false;
    }

    
}
