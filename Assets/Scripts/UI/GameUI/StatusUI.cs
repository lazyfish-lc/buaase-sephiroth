using UnityEngine;
using UnityEngine.UI;
public class StatusUI : FatherUI
{   
    public static StatusUI Instance;
    public CanvasGroup StatusCanvas;
    public static bool isOpen = false;
    public Slider HPSlider;
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
    public float GetPlayerHealth()
    {
        if (UIManager.Instance.player != null)
        {
            return UIManager.Instance.player.dynamicState.propertyMap.ContainsKey("Health") ? UIManager.Instance.player.playerState.propertyMap["Health"].value : 0f;
        }
        return 0f;
    }
    void Update()
    {
        if (UIManager.Instance.player != null)
        {
            // 超过1000血即为100%，根据实际情况调整
            float Health = GetPlayerHealth(); // 默认值，防止属性缺失导致错误
            if (Health > 1000f)
            {
                HPSlider.value = 1f;
            }
            else
            {
                HPSlider.value = Health / 1000f;
            }
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
        StatusCanvas.gameObject.SetActive(true);
        isOpen = true;
    }
    public void Close()
    {
        PlayCloseSFX();
        StatusCanvas.gameObject.SetActive(false);
        isOpen = false;
    }
    
}
