using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class GuideUI : FatherUI
{   
    public static GuideUI Instance;
    public CanvasGroup GuideCanvas;
    public TMP_Text GuideText;
    public TMP_Text GuideTitleText;
    public Image GuideImage;
    public TMP_Text PageNumber;
    public static bool isOpen = false;

    [SerializeField] private TutorialContentTable tutorialTable;

    private List<int> sortedVisibleIndices = new List<int>();
    private int currentPage = 1;
    private int totalPages = 0;

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
        GuideCanvas.alpha = 0;
        GuideCanvas.interactable = false;
        GuideCanvas.blocksRaycasts = false;
        Debug.Log($"[GuideUI] Start — tutorialTable={tutorialTable}, GuideText={GuideText}, GuideTitleText={GuideTitleText}, PageNumber={PageNumber}, GuideCanvas={GuideCanvas}");
    }
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void OnEnable()
    {
        Debug.Log($"[GuideUI] OnEnable — gameObject.activeSelf={gameObject.activeSelf}, Instance={Instance}");
        DialogueNodeActionSOActions.TutorialPopupRequested += OnTutorialPopupRequested;
        Debug.Log("[GuideUI] OnEnable — subscribed to TutorialPopupRequested");
    }

    void OnDisable()
    {
        Debug.Log($"[GuideUI] OnDisable");
        DialogueNodeActionSOActions.TutorialPopupRequested -= OnTutorialPopupRequested;
    }

    private void OnTutorialPopupRequested(NPCObject npc, string actionId, int tutorialIndex)
    {
        Debug.Log($"[GuideUI] OnTutorialPopupRequested — actionId={actionId}, tutorialIndex={tutorialIndex}, isOpen={isOpen}");
        if (tutorialIndex < 0)
        {
            Debug.LogWarning("[GuideUI] OnTutorialPopupRequested — tutorialIndex < 0, skipping");
            return;
        }

        RefreshVisibleIndices();
        int page = sortedVisibleIndices.IndexOf(tutorialIndex) + 1;
        Debug.Log($"[GuideUI] OnTutorialPopupRequested — page={page}, totalPages={totalPages}");
        if (page > 0 && page <= totalPages)
        {
            currentPage = page;
            ShowCurrentTutorial();
            if (!isOpen)
            {
                Debug.Log("[GuideUI] OnTutorialPopupRequested — UI not open, calling Open()");
                Open();
            }
        }
        else
        {
            Debug.LogWarning($"[GuideUI] OnTutorialPopupRequested — tutorialIndex {tutorialIndex} not found in sortedVisibleIndices (page={page})");
        }
    }

    /// <summary>
    /// 从菜单打开：按顺序查看所有可查看的历史教程，从第一页开始
    /// </summary>
    public void ShowAllVisibleTutorials()
    {
        Debug.Log("[GuideUI] ShowAllVisibleTutorials — called");
        RefreshVisibleIndices();
        Debug.Log($"[GuideUI] ShowAllVisibleTutorials — totalPages={totalPages}");
        if (totalPages == 0)
        {
            Debug.LogWarning("[GuideUI] ShowAllVisibleTutorials — no visible tutorials, nothing to show");
            return;
        }
        currentPage = 1;
        ShowCurrentTutorial();
    }

    private void RefreshVisibleIndices()
    {
        var visible = DialogueNodeActionSOActions.GetVisibleTutorialIndices();
        Debug.Log($"[GuideUI] RefreshVisibleIndices — count={visible.Count}, values=[{string.Join(", ", visible)}]");
        sortedVisibleIndices = visible
            .OrderBy(i => i)
            .ToList();
        totalPages = sortedVisibleIndices.Count;
        if (currentPage > totalPages) currentPage = totalPages;
        if (currentPage < 1 && totalPages > 0) currentPage = 1;
        Debug.Log($"[GuideUI] RefreshVisibleIndices — sortedVisibleIndices=[{string.Join(", ", sortedVisibleIndices)}], totalPages={totalPages}, currentPage={currentPage}");
    }

    private void ShowCurrentTutorial()
    {
        Debug.Log($"[GuideUI] ShowCurrentTutorial — sortedVisibleIndices.Count={sortedVisibleIndices.Count}, currentPage={currentPage}, totalPages={totalPages}");
        if (sortedVisibleIndices.Count == 0 || currentPage < 1 || currentPage > totalPages)
        {
            Debug.LogWarning("[GuideUI] ShowCurrentTutorial — bail: no indices or page out of range");
            return;
        }

        int tutorialIndex = sortedVisibleIndices[currentPage - 1];
        Debug.Log($"[GuideUI] ShowCurrentTutorial — tutorialIndex={tutorialIndex}, tutorialTable={tutorialTable}, table.tutorials=({(tutorialTable != null ? tutorialTable.tutorials?.Count.ToString() : "null")})");

        if (tutorialTable == null || tutorialTable.tutorials == null)
        {
            Debug.LogWarning("[GuideUI] ShowCurrentTutorial — tutorialTable or tutorials is null");
            return;
        }
        if (tutorialIndex < 0 || tutorialIndex >= tutorialTable.tutorials.Count)
        {
            Debug.LogWarning($"[GuideUI] ShowCurrentTutorial — tutorialIndex {tutorialIndex} out of range (table has {tutorialTable.tutorials.Count} entries)");
            return;
        }

        TutorialContent data = tutorialTable.tutorials[tutorialIndex];
        Debug.Log($"[GuideUI] ShowCurrentTutorial — title={data.title}, body={data.body}, image={data.image}");
        Debug.Log($"[GuideUI] ShowCurrentTutorial — GuideTitleText={GuideTitleText}, GuideText={GuideText}, GuideImage={GuideImage}, PageNumber={PageNumber}");
        GuideTitleText.text = data.title;
        GuideText.text = data.body;
        if (GuideImage != null)
        {
            GuideImage.sprite = data.image;
            GuideImage.enabled = data.image != null;
        }
        UpdatePageNumber();
        Debug.Log("[GuideUI] ShowCurrentTutorial — text & image fields updated");
    }

    public void LeftPage()
    {
        PlayPageTurnSFX();
        if (currentPage > 1)
        {
            currentPage--;
            ShowCurrentTutorial();
        }
    }

    public void RightPage()
    {
        PlayPageTurnSFX();
        if (currentPage < totalPages)
        {
            currentPage++;
            ShowCurrentTutorial();
        }
    }

    public void UpdatePageNumber()
    {
        PageNumber.text = $"PAGE: {currentPage}/{totalPages}";
    }

    public void OpenAndClose()
    {
        
        if(!isOpen)
        {
            RefreshVisibleIndices();
            if (totalPages > 0)
            {
                currentPage = 1;
                ShowCurrentTutorial();
                Open();
            }
        }
        else
        {
            Close();
        }
    }
    public void Open()
    {
        Debug.Log($"[GuideUI] Open — isOpen before={isOpen}");
        PlayOpenSFX();
        GuideCanvas.alpha = 1;
        GuideCanvas.interactable = true;
        GuideCanvas.blocksRaycasts = true;
        isOpen = true;
        Debug.Log($"[GuideUI] Open — isOpen={isOpen}");
    }
    public void Close()
    {
        Debug.Log($"[GuideUI] Close — isOpen={isOpen}");
        PlayCloseSFX();
        GuideCanvas.alpha = 0;
        GuideCanvas.interactable = false;
        GuideCanvas.blocksRaycasts = false;
        isOpen = false;
    }
    
}
