using UnityEngine;
using UnityEngine.UI;
public class KeysetUI : FatherUI
{   
    public static KeysetUI Instance;
    public CanvasGroup KeysetCanvas;
    public static bool isOpen = false;
    void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        KeysetCanvas.gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void OpenAndClose()
    {
        
        if(!isOpen)
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
        PlayOpenSFX();
        KeysetCanvas.gameObject.SetActive(true);
        isOpen = true;
    }
    public void Close()
    {
        PlayCloseSFX();
        KeysetCanvas.gameObject.SetActive(false);
        isOpen = false;
    }
    
}
