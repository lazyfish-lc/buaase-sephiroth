using UnityEngine;
using TMPro;
public class BackpackUI : MonoBehaviour
{
    public CanvasGroup BackpackCanvas;
    public static BackpackUI Instance;

    public TMP_Text PageNumber;

    private int currentPage = 1;
    private int totalPages = 100;

     
    public bool isOpen = false;
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
        BackpackCanvas.alpha = 0;
        BackpackCanvas.interactable = false;
        BackpackCanvas.blocksRaycasts = false;
        UpdatePageNumber();
    }
    public void OpenAndClose()
    {
        
        if(BackpackCanvas.alpha == 0)
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
        isOpen = true;
        BackpackCanvas.alpha = 1;
        BackpackCanvas.interactable = true;
        BackpackCanvas.blocksRaycasts = true;
    }
    public void Close()
    {
        isOpen = false;
        BackpackCanvas.alpha = 0;
        BackpackCanvas.interactable = false;
        BackpackCanvas.blocksRaycasts = false;
    }

    public void UpdatePageNumber()
    {
        PageNumber.text = $"PAGE: {currentPage}/{totalPages}";
    }
    public void RightPage()
    {
        if(currentPage < totalPages)
        {
            currentPage++;
            UpdatePageNumber();
        }
    }
    public void LeftPage()
    {
        if(currentPage > 1)
        {
            currentPage--;
            UpdatePageNumber();
        }
    }

}
