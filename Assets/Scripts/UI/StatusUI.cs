using UnityEngine;
using UnityEngine.UI;
public class StatusUI : MonoBehaviour
{
    public CanvasGroup StatusCanvas;
    public static StatusUI Instance;
    public PlayerSmallObject player;

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

    public float GetPlayerHealth()
    {
        if (player != null)
        {
            return player.dynamicState.propertyMap.ContainsKey("HP") ? player.playerState.propertyMap["HP"].value : 0f;
        }
        return 0f;
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
    }
    void Update()
    {
        if (player != null)
        {
            // 超过1000血即为100%，根据实际情况调整
            float HP = GetPlayerHealth(); // 默认值，防止属性缺失导致错误
            if (HP > 1000f)
            {
                HPSlider.value = 1f;
            }
            else
            {
                HPSlider.value = HP / 1000f;
            }
        }
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
