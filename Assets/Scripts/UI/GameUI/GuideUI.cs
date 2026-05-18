using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class GuideUI : FatherUI
{   
    public static GuideUI Instance;
    public CanvasGroup GuideCanvas;
    public TMP_Text GuideText;
    public TMP_Text GuideTitleText;
    public Image GuideImage;
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
        GuideCanvas.gameObject.SetActive(false);
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
        GuideCanvas.gameObject.SetActive(true);
        isOpen = true;
    }
    public void Close()
    {
        PlayCloseSFX();
        GuideCanvas.gameObject.SetActive(false);
        isOpen = false;
    }
    
}
